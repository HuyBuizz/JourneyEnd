using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

struct Bounds2D { public float2 Min; public float2 Max; }

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct PointSetupSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<PointSetupSystemDone>()) return;

        var em = state.EntityManager;
        var physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (spawner, spawnerEntity) in
                 SystemAPI.Query<RefRO<SpawnPointSpawner>>()
                          .WithEntityAccess())
        {
            // Gom bounds toàn bộ SpawnZone phù hợp loại spawner
            Bounds2D bounds = new Bounds2D { Min = new float2(float.MaxValue), Max = new float2(float.MinValue) };
            float2 yBounds = new float2(float.MaxValue, float.MinValue);

            foreach (var (pc, l2w, entity) in
                     SystemAPI.Query<RefRO<PhysicsCollider>, RefRO<LocalToWorld>>()
                              .WithAll<SpawnZone>()
                              .WithEntityAccess())
            {
                if (!pc.ValueRO.IsValid) continue;

                var spawnZone = em.GetComponentData<SpawnZone>(entity);
                if ((int)spawnZone.spawnZoneType != (int)spawner.ValueRO.spawnerType) continue;

                var world = l2w.ValueRO.Value;
                var rt = new RigidTransform(math.normalize(new quaternion(world)), world.c3.xyz);
                var aabb = pc.ValueRO.Value.Value.CalculateAabb(rt);

                bounds.Min = math.min(bounds.Min, aabb.Min.xz);
                bounds.Max = math.max(bounds.Max, aabb.Max.xz);

                yBounds.x = math.min(yBounds.x, aabb.Min.y);
                yBounds.y = math.max(yBounds.y, aabb.Max.y);
            }

            if (bounds.Min.x > bounds.Max.x || bounds.Min.y > bounds.Max.y)
            {
                ecb.DestroyEntity(spawnerEntity);
                continue;
            }

            // --- Logic spawn riêng theo type ---
            switch (spawner.ValueRO.spawnerType)
            {
                case SpawnerAuthoring.SpawnerType.FlamePoint:
                    SpawnFlamePoint(em, physics, ecb, spawner, spawnerEntity, bounds, yBounds);
                    break;

                case SpawnerAuthoring.SpawnerType.SpawnPoint:
                    SpawnCustomPoint(em, physics, ecb, spawner, spawnerEntity, bounds, yBounds);
                    break;

                default:
                    SpawnCustomPoint(em, physics, ecb, spawner, spawnerEntity, bounds, yBounds);
                    break;
            }

            ecb.DestroyEntity(spawnerEntity);
        }

        ecb.Playback(em);
        ecb.Dispose();

        if (!SystemAPI.HasSingleton<PointSetupSystemDone>())
        {
            em.CreateEntity(typeof(PointSetupSystemDone));
        }
    }

    #region --- Logic spawn riêng từng type ---

    private void SpawnFlamePoint(EntityManager em, PhysicsWorldSingleton physics, EntityCommandBuffer ecb,
                                 RefRO<SpawnPointSpawner> spawner, Entity spawnerEntity,
                                 Bounds2D bounds, float2 yBounds)
    {
        float margin = math.max(0f, spawner.ValueRO.margin);
        float density = math.max(0.0001f, spawner.ValueRO.pointDensity);
        float minDistance = math.sqrt(1f / density); // Minimum distance between points
        var filter = CollisionFilter.Default;

        float minX = bounds.Min.x + margin;
        float maxX = bounds.Max.x - margin;
        float minZ = bounds.Min.y + margin;
        float maxZ = bounds.Max.y - margin;
        if (maxX <= minX || maxZ <= minZ) return;

        // Initialize Poisson Disc Sampling
        var points = new NativeList<float2>(Allocator.Temp);
        var activeList = new NativeList<float2>(Allocator.Temp);
        uint seed = (uint)spawnerEntity.Index;
        var rand = Unity.Mathematics.Random.CreateFromIndex(seed);

        // Start with a random point
        float2 firstPoint = new float2(
            rand.NextFloat(minX, maxX),
            rand.NextFloat(minZ, maxZ)
        );
        points.Add(firstPoint);
        activeList.Add(firstPoint);

        // Poisson Disc Sampling parameters
        int maxAttempts = 30; // Max attempts to find a valid point around a sample
        float minDistanceSqr = minDistance * minDistance;

        // Generate points using Poisson Disc Sampling
        while (activeList.Length > 0)
        {
            int activeIndex = rand.NextInt(0, activeList.Length);
            float2 center = activeList[activeIndex];
            bool foundValidPoint = false;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Generate a random point in an annulus (minDistance to 2*minDistance)
                float angle = rand.NextFloat(0f, 2f * math.PI);
                float radius = rand.NextFloat(minDistance, 2f * minDistance);
                float2 candidate = center + new float2(math.cos(angle), math.sin(angle)) * radius;

                // Check if candidate is within bounds
                if (candidate.x < minX || candidate.x > maxX || candidate.y < minZ || candidate.y > maxZ)
                    continue;

                // Check minimum distance to existing points
                bool isValid = true;
                for (int j = 0; j < points.Length; j++)
                {
                    if (math.distancesq(candidate, points[j]) < minDistanceSqr)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    points.Add(candidate);
                    activeList.Add(candidate);
                    foundValidPoint = true;
                    break;
                }
            }

            if (!foundValidPoint)
            {
                activeList.RemoveAt(activeIndex); // Remove center if no valid point found
            }
        }

        // Spawn entities at valid points using raycasts
        var prefabLT = em.GetComponentData<LocalTransform>(spawner.ValueRO.prefab);
        for (int i = 0; i < points.Length; i++)
        {
            float2 point = points[i];
            float3 from = new float3(point.x, yBounds.y + 0.5f, point.y);
            float3 to = new float3(point.x, yBounds.x - 0.5f, point.y);

            var input = new RaycastInput { Start = from, End = to, Filter = filter };
            if (physics.CollisionWorld.CastRay(input, out var hit))
            {
                var hitEntity = physics.Bodies[hit.RigidBodyIndex].Entity;
                if (!em.HasComponent<SpawnZone>(hitEntity)) continue;
                if (hit.SurfaceNormal.y < 0.2f) continue;

                var e = ecb.Instantiate(spawner.ValueRO.prefab);
                prefabLT.Position = hit.Position;
                prefabLT.Scale = 1f;
                ecb.SetComponent(e, prefabLT);
            }
        }

        points.Dispose();
        activeList.Dispose();
    }

    private void SpawnCustomPoint(EntityManager em, PhysicsWorldSingleton physics, EntityCommandBuffer ecb,
                                 RefRO<SpawnPointSpawner> spawner, Entity spawnerEntity,
                                 Bounds2D bounds, float2 yBounds)
    {
        float margin = math.max(0f, spawner.ValueRO.margin);
        float density = math.max(0.0001f, spawner.ValueRO.pointDensity);
        float spacing = math.sqrt(1f / density); // Khoảng cách giữa các điểm trong lưới

        var filter = CollisionFilter.Default;
        float minX = bounds.Min.x + margin;
        float maxX = bounds.Max.x - margin;
        float minZ = bounds.Min.y + margin;
        float maxZ = bounds.Max.y - margin;
        if (maxX <= minX || maxZ <= minZ) return;

        // Tính toán điểm bắt đầu và số lượng điểm trên mỗi chiều
        float startX = math.ceil(minX / spacing) * spacing; // Đảm bảo bắt đầu từ điểm lưới gần nhất
        float startZ = math.ceil(minZ / spacing) * spacing;
        int nx = (int)math.floor((maxX - startX) / spacing) + 1;
        int nz = (int)math.floor((maxZ - startZ) / spacing) + 1;

        var prefabLT = em.GetComponentData<LocalTransform>(spawner.ValueRO.prefab);

        // Tạo các điểm spawn trên lưới đều
        for (int ix = 0; ix < nx; ix++)
        {
            float x = startX + ix * spacing;
            for (int iz = 0; iz < nz; iz++)
            {
                float z = startZ + iz * spacing;

                // Tạo raycast từ trên xuống để tìm bề mặt hợp lệ
                float3 from = new float3(x, yBounds.y + 0.5f, z);
                float3 to = new float3(x, yBounds.x - 0.5f, z);

                var input = new RaycastInput { Start = from, End = to, Filter = filter };
                if (physics.CollisionWorld.CastRay(input, out var hit))
                {
                    var hitEntity = physics.Bodies[hit.RigidBodyIndex].Entity;
                    if (!em.HasComponent<SpawnZone>(hitEntity)) continue;
                    if (hit.SurfaceNormal.y < 0.2f) continue;

                    var e = ecb.Instantiate(spawner.ValueRO.prefab);
                    prefabLT.Position = hit.Position;
                    prefabLT.Scale = 1f;
                    ecb.SetComponent(e, prefabLT);
                }
            }
        }
    }

    #endregion
}

public struct PointSetupSystemDone : IComponentData { }
