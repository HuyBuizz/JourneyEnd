using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

[BurstCompile]
[UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
public unsafe partial struct PlayerPushBodiesWhenProxyMovesSystem : ISystem
{
    [BurstCompile] public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<PlayerProxyTag>();
    }

    [BurstCompile] public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (lt, col, vel, ent) in
                 SystemAPI.Query<RefRO<LocalTransform>, RefRO<PhysicsCollider>, RefRO<PhysicsVelocity>>()
                          .WithAll<PlayerProxyTag>()
                          .WithEntityAccess())
        {
            float3 from = lt.ValueRO.Position;
            float3 to   = from + vel.ValueRO.Linear * dt;

            var input = new ColliderCastInput {
                Collider = (Unity.Physics.Collider*)col.ValueRO.Value.GetUnsafePtr(),
                Orientation = lt.ValueRO.Rotation,
                Start = from, End = to
            };
            var hits = new NativeList<ColliderCastHit>(Allocator.Temp);
            if (world.CastCollider(input, ref hits))
            {
                for (int i=0; i<hits.Length; i++)
                {
                    var hit = hits[i];
                    var body = world.Bodies[hit.RigidBodyIndex];
                    var other = body.Entity;
                    if (!state.EntityManager.HasComponent<PhysicsMass>(other)) continue;
                    var pm = state.EntityManager.GetComponentData<PhysicsMass>(other);
                    if (pm.InverseMass <= 0f) continue;

                    float3 n = hit.SurfaceNormal;
                    float  along = math.max(0, math.dot(vel.ValueRO.Linear, n));
                    if (along <= 0f) continue;

                    var ov = state.EntityManager.GetComponentData<PhysicsVelocity>(other);
                    ov.Linear += n * along * 0.6f; // hệ số tuỳ chỉnh
                    ecb.SetComponent(other, ov);
                }
            }
            hits.Dispose();
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
