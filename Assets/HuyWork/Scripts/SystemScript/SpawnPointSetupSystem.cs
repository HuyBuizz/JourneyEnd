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
public partial struct SpawnPointSetupSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
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

                case SpawnerAuthoring.SpawnerType.Car:
                    break;
                
                case SpawnerAuthoring.SpawnerType.Human:
                    break;

                default:
                    SpawnCustomPoint(em, physics, ecb, spawner, spawnerEntity, bounds, yBounds);
                    break;
            }

            ecb.DestroyEntity(spawnerEntity);
        }

        ecb.Playback(em);
        ecb.Dispose();
    }

    #region --- Logic spawn riêng từng type ---

    private void SpawnFlamePoint(EntityManager em, PhysicsWorldSingleton physics, EntityCommandBuffer ecb,
                                 RefRO<SpawnPointSpawner> spawner, Entity spawnerEntity,
                                 Bounds2D bounds, float2 yBounds)
    {
        float margin = math.max(0f, spawner.ValueRO.margin);
        float density = math.max(0.0001f, spawner.ValueRO.pointDensity);
        float spacing = math.sqrt(1f / density);
        float jitterK = 0.2f;

        var filter = CollisionFilter.Default;
        float minX = bounds.Min.x + margin;
        float maxX = bounds.Max.x - margin;
        float minZ = bounds.Min.y + margin;
        float maxZ = bounds.Max.y - margin;
        if (maxX <= minX || maxZ <= minZ) return;

        float startX = math.floor(minX / spacing) * spacing;
        float startZ = math.floor(minZ / spacing) * spacing;
        int nx = (int)math.floor((maxX - startX) / spacing) + 1;
        int nz = (int)math.floor((maxZ - startZ) / spacing) + 1;

        var prefabLT = em.GetComponentData<LocalTransform>(spawner.ValueRO.prefab);

        for (int ix = 0; ix < nx; ix++)
        {
            float x = startX + ix * spacing;
            for (int iz = 0; iz < nz; iz++)
            {
                float z = startZ + iz * spacing;
                uint seed = math.hash(new int3(ix, iz, spawnerEntity.Index));
                var rand = Unity.Mathematics.Random.CreateFromIndex(seed);
                float jx = (rand.NextFloat() - 0.5f) * jitterK * spacing;
                float jz = (rand.NextFloat() - 0.5f) * jitterK * spacing;

                float3 from = new float3(x + jx, yBounds.y + 0.5f, z + jz);
                float3 to = new float3(x + jx, yBounds.x - 0.5f, z + jz);

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

    private void SpawnCar(EntityManager em, PhysicsWorldSingleton physics, EntityCommandBuffer ecb,
                          RefRO<SpawnPointSpawner> spawner, Entity spawnerEntity,
                          Bounds2D bounds, float2 yBounds)
    { 
        // TODO: logic riêng cho car
    }

    private void SpawnHuman(EntityManager em, PhysicsWorldSingleton physics, EntityCommandBuffer ecb,
                            RefRO<SpawnPointSpawner> spawner, Entity spawnerEntity,
                            Bounds2D bounds, float2 yBounds)
    {
        // TODO: logic riêng cho human
    }

    private void SpawnCustomPoint(EntityManager em, PhysicsWorldSingleton physics, EntityCommandBuffer ecb,
                                  RefRO<SpawnPointSpawner> spawner, Entity spawnerEntity,
                                  Bounds2D bounds, float2 yBounds)
    {
        // TODO: logic riêng cho custom type
    }

    #endregion
}
