using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

[BurstCompile]
[UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
public unsafe partial struct PlayerProxyMovementSystem : ISystem
{
    [BurstCompile] public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<PlayerInputState>();
        state.RequireForUpdate<PlayerProxyTag>();
    }

    [BurstCompile] public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        if (dt <= 0f) return;

        var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var input = SystemAPI.GetSingleton<PlayerInputState>();

        foreach (var (ltRW, colRO, cfg, stRW, lastRW, ent) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRO<PhysicsCollider>, RefRO<ProxyMoveConfig>,
                                 RefRW<ProxyRuntimeState>, RefRW<LastPos>>()
                          .WithAll<PlayerProxyTag>()
                          .WithEntityAccess())
        {
            float3 pos = ltRW.ValueRO.Position;
            float3 vel = stRW.ValueRO.Velocity;

            // Ground check (ray ngắn xuống)
            bool grounded = IsGrounded(ref world, (Unity.Physics.Collider*)colRO.ValueRO.Value.GetUnsafePtr(),
                                       ltRW.ValueRO.Rotation, pos, cfg.ValueRO.Skin, cfg.ValueRO.MaxSlopeDeg);

            // Horizontal
            float spd = cfg.ValueRO.MoveSpeed * (input.Sprint ? cfg.ValueRO.SprintMultiplier : 1f);
            float3 wish = input.MoveDirWorld; // đã là phẳng
            float3 horiz = new float3(vel.x, 0, vel.z);
            float3 target = wish * spd;
            float accel = grounded ? 20f : 6f;
            horiz = math.lerp(horiz, target, math.saturate(accel * dt));

            // Vertical
            float vy = vel.y;
            if (grounded) { vy = input.Jump ? cfg.ValueRO.JumpSpeed : math.min(vy, 0f); }
            vy -= cfg.ValueRO.Gravity * dt;
            vel = new float3(horiz.x, vy, horiz.z);

            // Integrate + slide
            float3 from = pos;
            float3 to   = pos + vel * dt;
            var hits = new NativeList<ColliderCastHit>(Allocator.Temp);

            for (int i=0; i<cfg.ValueRO.MaxSlideIters; i++)
            {
                var ci = new ColliderCastInput {
                    Collider = (Unity.Physics.Collider*)colRO.ValueRO.Value.GetUnsafePtr(),
                    Orientation = ltRW.ValueRO.Rotation,
                    Start = from, End = to
                };
                hits.Clear();
                if (world.CastCollider(ci, ref hits))
                {
                    var h = hits[0];
                    float3 n = h.SurfaceNormal;
                    float3 hitPos = h.Position - n * cfg.ValueRO.Skin;
                    from = hitPos;
                    vel = ProjectOnPlane(vel, n);              // slide trên mặt
                    to = from + vel * dt * (1f - (i+1)*0.25f); // giảm nhẹ bước
                }
                else break;
            }
            hits.Dispose();

            // write-back
            ltRW.ValueRW.Position = to;
            stRW.ValueRW.Velocity = vel;
            stRW.ValueRW.IsGrounded = grounded;
            lastRW.ValueRW.Value = to;

            // để hệ “đẩy” đọc vận tốc
            var pv = SystemAPI.GetComponentRW<PhysicsVelocity>(ent);
            pv.ValueRW.Linear = vel;
            pv.ValueRW.Angular = float3.zero;
        }

        // consume Jump (1 frame)
        var inp = SystemAPI.GetSingletonRW<PlayerInputState>();
        inp.ValueRW.Jump = false;
    }

    static float3 ProjectOnPlane(float3 v, float3 n) => v - n * math.dot(v, n);

    static bool IsGrounded(ref PhysicsWorld world, Unity.Physics.Collider* col,
                           quaternion rot, float3 pos, float skin, float maxSlopeDeg)
    {
        var ray = new RaycastInput {
            Start = pos, End = pos - new float3(0, skin*3f, 0), Filter = CollisionFilter.Default
        };
        if (world.CastRay(ray, out var hit))
        {
            float cos = math.dot(hit.SurfaceNormal, new float3(0,1,0));
            float ang = math.degrees(math.acos(math.saturate(cos)));
            return ang <= maxSlopeDeg;
        }
        return false;
    }
}
