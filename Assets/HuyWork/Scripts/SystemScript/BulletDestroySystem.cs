using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[BurstCompile]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct ProjectileSystem : ISystem
{
    private ComponentLookup<Bullet> _bulletLookup; // cache

    [BurstCompile]
    private struct SprayTriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<Bullet> BulletLookup;
        public EntityCommandBuffer ECB;

        public void Execute(TriggerEvent ev)
        {
            var a = ev.EntityA;
            var b = ev.EntityB;

            if (BulletLookup.HasComponent(a)) ECB.DestroyEntity(a);
            if (BulletLookup.HasComponent(b)) ECB.DestroyEntity(b);
        }
    }

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        _bulletLookup = state.GetComponentLookup<Bullet>(true); // init 1 lần
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // refresh lookups mỗi frame
        _bulletLookup.Update(ref state);

        // ECB từ EndSimulation => tự playback đúng thời điểm, không phải Complete/Dispose thủ công
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var sim = SystemAPI.GetSingleton<SimulationSingleton>();

        var job = new SprayTriggerJob
        {
            BulletLookup = _bulletLookup,
            ECB = ecb
        };

        state.Dependency = job.Schedule(sim, state.Dependency);
    }
}
