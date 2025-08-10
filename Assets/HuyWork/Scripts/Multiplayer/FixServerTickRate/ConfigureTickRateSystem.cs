using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ConfigureTickRateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var e = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(e, new ClientServerTickRate
        {
            SimulationTickRate = 30,          // thử 30 hoặc 20
            NetworkTickRate     = 30,
            MaxSimulationStepsPerFrame = 3,   // hạn chế bù tick
            // MaxSimulationStepBatchSize = 1, // nếu field có trong bản bạn dùng
        });
        state.Enabled = false;
    }
}
