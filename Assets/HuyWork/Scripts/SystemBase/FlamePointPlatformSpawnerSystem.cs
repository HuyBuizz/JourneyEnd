using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

// Gom 2D bounds theo XZ
struct Bounds2D { public float2 Min; public float2 Max; }

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct FlamePointPlatformSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlamePointFloorSpawner>();
        state.RequireForUpdate<PhysicsWorldSingleton>(); 
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;
        var spawner = SystemAPI.GetSingleton<FlamePointFloorSpawner>();
        var prefabLT = em.GetComponentData<LocalTransform>(spawner.prefab);

        var physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        // 1) Gom bounds theo nhóm (nếu có PlatformGroup), nếu không: nhóm mặc định = 0
        var groupsXZ = new NativeParallelHashMap<int, Bounds2D>(16, Allocator.Temp);
        var groupsY = new NativeParallelHashMap<int, float2>(16, Allocator.Temp); // yMin, yMax

        foreach (var (pc, l2w, e) in
                 SystemAPI.Query<RefRO<PhysicsCollider>, RefRO<LocalToWorld>>()
                          .WithAll<FlamePointPlatform>()
                          .WithEntityAccess())
        {
            if (!pc.ValueRO.IsValid) continue;

            var world = l2w.ValueRO.Value;
            var rt = new RigidTransform(math.normalize(new quaternion(world)), world.c3.xyz);
            var aabb = pc.ValueRO.Value.Value.CalculateAabb(rt);

            int gid = em.HasComponent<PlatformGroup>(e) ? em.GetComponentData<PlatformGroup>(e).Id : 0;

            // XZ bounds
            Bounds2D b;
            if (groupsXZ.TryGetValue(gid, out b))
            {
                b.Min = math.min(b.Min, aabb.Min.xz);
                b.Max = math.max(b.Max, aabb.Max.xz);
                groupsXZ[gid] = b;

                var y = groupsY[gid];
                y.x = math.min(y.x, aabb.Min.y);
                y.y = math.max(y.y, aabb.Max.y);
                groupsY[gid] = y;
            }
            else
            {
                groupsXZ.TryAdd(gid, new Bounds2D { Min = aabb.Min.xz, Max = aabb.Max.xz });
                groupsY.TryAdd(gid, new float2(aabb.Min.y, aabb.Max.y));
            }
        }

        // Không tìm thấy platform nào ⇒ thoát
        if (groupsXZ.IsEmpty)
        {
            // vẫn hủy singleton để không chạy lại
            var sEnt = SystemAPI.GetSingletonEntity<FlamePointFloorSpawner>();
            em.DestroyEntity(sEnt);
            groupsXZ.Dispose();
            groupsY.Dispose();
            return;
        }

        // 2) Tính bước lưới toàn cục theo mật độ
        float margin = math.max(0f, spawner.margin);
        float density = math.max(0.0001f, spawner.pointDensity);
        float spacing = math.sqrt(1f / density);     // ≈ khoảng cách mục tiêu giữa 2 điểm
        float jitterK = 0.2f;                         // 20% jitter, có thể expose ra spawner nếu thích

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Nếu bạn có category riêng cho Floor, hãy set filter CollidesWith tương ứng.
        var filter = CollisionFilter.Default;

        // 3) Duyệt từng nhóm sàn, canh lưới theo trục thế giới
        using (var kv = groupsXZ.GetKeyValueArrays(Allocator.Temp))
        {
            for (int idx = 0; idx < kv.Keys.Length; idx++)
            {
                int gid = kv.Keys[idx];
                var b = kv.Values[idx];
                var yMM = groupsY[gid];
                float yTop = yMM.y;
                float yBottom = yMM.x;

                // Áp dụng margin vào viền ngoài của toàn bộ sàn (nhóm)
                float minX = b.Min.x + margin;
                float maxX = b.Max.x - margin;
                float minZ = b.Min.y + margin;
                float maxZ = b.Max.y - margin;
                if (maxX <= minX || maxZ <= minZ) continue;

                // Căn lưới theo spacing toàn cục để các platform ghép không bị lệch pha
                float startX = math.floor(minX / spacing) * spacing;
                float startZ = math.floor(minZ / spacing) * spacing;

                int nx = (int)math.floor((maxX - startX) / spacing) + 1;
                int nz = (int)math.floor((maxZ - startZ) / spacing) + 1;

                for (int ix = 0; ix < nx; ix++)
                {
                    float x = startX + ix * spacing;
                    if (x < minX || x > maxX) continue;

                    for (int iz = 0; iz < nz; iz++)
                    {
                        float z = startZ + iz * spacing;
                        if (z < minZ || z > maxZ) continue;

                        // Jitter nhẹ để bớt đều "quá sạch"
                        uint seed = math.hash(new int3(ix, iz, gid));
                        var rand = Unity.Mathematics.Random.CreateFromIndex(seed);
                        float jx = (rand.NextFloat() - 0.5f) * jitterK * spacing;
                        float jz = (rand.NextFloat() - 0.5f) * jitterK * spacing;

                        float3 from = new float3(x + jx, yTop + 0.5f, z + jz);
                        float3 to = new float3(x + jx, yBottom - 0.5f, z + jz);

                        var input = new RaycastInput { Start = from, End = to, Filter = filter };

                        // Raycast: chỉ spawn nếu bắn trúng một FlamePointPlatform (đúng nhóm nếu có)
                        if (physics.CollisionWorld.CastRay(input, out var hit))
                        {
                            var hitEntity = physics.Bodies[hit.RigidBodyIndex].Entity;

                            if (!em.HasComponent<FlamePointPlatform>(hitEntity))
                                continue;

                            int hitGroup = em.HasComponent<PlatformGroup>(hitEntity)
                                         ? em.GetComponentData<PlatformGroup>(hitEntity).Id
                                         : 0;
                            if (hitGroup != gid)
                                continue;

                            // Mặt trên (tránh mặt hông/đáy), cho phép dốc nhẹ nếu cần
                            if (hit.SurfaceNormal.y < 0.2f)
                                continue;

                            var e = ecb.Instantiate(spawner.prefab);
                            //
                            prefabLT.Position = hit.Position;
                            prefabLT.Scale = 1f;
                            ecb.SetComponent(e, prefabLT);
                        }
                    }
                }
            }
        }

        ecb.Playback(em);
        ecb.Dispose();

        // 4) Hủy singleton để chạy 1 lần
        var singletonEntity = SystemAPI.GetSingletonEntity<FlamePointFloorSpawner>();
        em.DestroyEntity(singletonEntity);

        groupsXZ.Dispose();
        groupsY.Dispose();
    }
}

public struct PlatformGroup : IComponentData
{
    public int Id;
}