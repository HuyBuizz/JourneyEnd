using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct FlamePointWallSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlamePointWallSpawner>();   // spawner riêng
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<Wall>();          // phải có ít nhất 1 tường
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var em      = state.EntityManager;
        var cfg     = SystemAPI.GetSingleton<FlamePointWallSpawner>();
        var physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        // Giữ scale/rotation của prefab
        var prefabLT = em.GetComponentData<LocalTransform>(cfg.prefab);

        float spacing = math.sqrt(1f / cfg.pointDensity);
        float margin  = math.max(0f, cfg.margin);
        float jitterK = math.clamp(cfg.jitterK, 0f, 1f);

        var ecb    = new EntityCommandBuffer(Allocator.Temp);
        var filter = CollisionFilter.Default;

        foreach (var (pc, l2w) in
                 SystemAPI.Query<RefRO<PhysicsCollider>, RefRO<LocalToWorld>>()
                          .WithAll<Wall>())
        {
            if (!pc.ValueRO.IsValid) continue;

            // AABB theo world
            var world = l2w.ValueRO.Value;
            var rt    = new RigidTransform(math.normalize(new quaternion(world)), world.c3.xyz);
            var aabb  = pc.ValueRO.Value.Value.CalculateAabb(rt);

            float3 size = aabb.Max - aabb.Min;

            // Chọn trục “mỏng nhất” làm pháp tuyến xấp xỉ của tường
            int nAxis = 0; float minExtent = size.x;
            if (size.y < minExtent) { minExtent = size.y; nAxis = 1; }
            if (size.z < minExtent) { minExtent = size.z; nAxis = 2; }

            // Hai trục còn lại là trục lưới (u, v)
            int uAxis = (nAxis == 0) ? 1 : 0;
            int vAxis = (nAxis == 2) ? 1 : 2;
            if (uAxis == vAxis) vAxis = 2;

            // Dải u,v sau margin
            float uMin = Get(aabb.Min, uAxis) + margin;
            float uMax = Get(aabb.Max, uAxis) - margin;
            float vMin = Get(aabb.Min, vAxis) + margin;
            float vMax = Get(aabb.Max, vAxis) - margin;
            if (uMax <= uMin || vMax <= vMin) continue;

            // Căn lưới
            float uStart = math.floor(uMin / spacing) * spacing;
            float vStart = math.floor(vMin / spacing) * spacing;
            int nu = (int)math.floor((uMax - uStart) / spacing) + 1;
            int nv = (int)math.floor((vMax - vStart) / spacing) + 1;

            // Biên ray theo trục n
            float nFront = Get(aabb.Max, nAxis) + 0.5f;
            float nBack  = Get(aabb.Min, nAxis) - 0.5f;

            for (int iu = 0; iu < nu; iu++)
            {
                float u = uStart + iu * spacing;
                if (u < uMin || u > uMax) continue;

                for (int iv = 0; iv < nv; iv++)
                {
                    float v = vStart + iv * spacing;
                    if (v < vMin || v > vMax) continue;

                    // jitter
                    uint seed = math.hash(new int3(iu, iv, nAxis));
                    var rnd   = Unity.Mathematics.Random.CreateFromIndex(seed);
                    float ju  = (rnd.NextFloat() - 0.5f) * jitterK * spacing;
                    float jv  = (rnd.NextFloat() - 0.5f) * jitterK * spacing;

                    float3 pFront = float3.zero, pBack = float3.zero;
                    Set(ref pFront, uAxis, u + ju);
                    Set(ref pFront, vAxis, v + jv);
                    Set(ref pFront, nAxis, nFront);
                    pBack = pFront; Set(ref pBack, nAxis, nBack);

                    var input = new RaycastInput { Start = pFront, End = pBack, Filter = filter };
                    if (!physics.CollisionWorld.CastRay(input, out var hit)) continue;

                    // Lọc mặt tường: |normal.y| nhỏ (gần thẳng đứng)
                    if (math.abs(hit.SurfaceNormal.y) > cfg.normalYAbsMax) continue;

                    // Spawn: giữ nguyên scale/rotation prefab, chỉ đổi vị trí
                    var e  = ecb.Instantiate(cfg.prefab);
                    var lt = prefabLT; lt.Position = hit.Position;
                    ecb.SetComponent(e, lt);
                }
            }
        }

        ecb.Playback(em);
        ecb.Dispose();

        // Tuỳ bạn: nếu muốn one‑shot, huỷ singleton này (KHÔNG ảnh hưởng platform spawner)
        var sEnt = SystemAPI.GetSingletonEntity<FlamePointWallSpawner>();
        em.DestroyEntity(sEnt);
    }

    // Helpers lấy/đặt theo trục
    static float Get(in float3 v, int axis) => axis == 0 ? v.x : (axis == 1 ? v.y : v.z);
    static void  Set(ref float3 v, int axis, float val)
    {
        if (axis == 0) v.x = val; else if (axis == 1) v.y = val; else v.z = val;
    }
}
