using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.VisualScripting;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(SpawnPointNeighborBuildSystem))]
public partial struct SpawnPrefabSystem : ISystem
{
    public bool hasSpawned; // đảm bảo spawn 1 lần

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnPrefabSingleton>();
        hasSpawned = false;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (hasSpawned) return;

        var em = state.EntityManager;
        var singleton = SystemAPI.GetSingleton<SpawnPrefabSingleton>();

        // Spawn cho SpawnPointTag0
        var q0 = SystemAPI.QueryBuilder()
                    .WithAll<SpawnPointTag0, LocalTransform>()
                    .Build();
        var entities0 = q0.ToEntityArray(Allocator.Temp);
        var transforms0 = q0.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        for (int i = 0; i < entities0.Length; i++)
        {
            var e = em.Instantiate(singleton.prefab0);
            em.SetComponentData(e, new LocalTransform { Position = transforms0[i].Position, Rotation = transforms0[i].Rotation, Scale = transforms0[i].Scale });
        }

        entities0.Dispose();
        transforms0.Dispose();

        // Spawn cho SpawnPointTag3x3
        var q3 = SystemAPI.QueryBuilder()
                    .WithAll<SpawnPointTag3x3, LocalTransform>()
                    .Build();
        var entities3 = q3.ToEntityArray(Allocator.Temp);
        var transforms3 = q3.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        for (int i = 0; i < entities3.Length; i++)
        {
            var e = em.Instantiate(singleton.prefab3x3);
            em.SetComponentData(e, new LocalTransform { Position = transforms3[i].Position, Rotation = transforms3[i].Rotation, Scale = transforms3[i].Scale });
        }

        entities3.Dispose();
        transforms3.Dispose();

        // Spawn cho SpawnPointTag5x5
        var q5 = SystemAPI.QueryBuilder()
                    .WithAll<SpawnPointTag5x5, LocalTransform>()
                    .Build();
        var entities5 = q5.ToEntityArray(Allocator.Temp);
        var transforms5 = q5.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        for (int i = 0; i < entities5.Length; i++)
        {
            var e = em.Instantiate(singleton.prefab5x5);
            em.SetComponentData(e, new LocalTransform { Position = transforms5[i].Position, Rotation = transforms5[i].Rotation, Scale = transforms5[i].Scale });
        }

        entities5.Dispose();
        transforms5.Dispose();

        hasSpawned = true; // đánh dấu đã spawn xong
    }
}
