using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct SpawnComboPrefabByTagSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnPrefabSingleton>();
        state.RequireForUpdate<SpawnPointNeighborBuildSystemDone>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<SpawnComboPrefabByTagSystemDone>()) return;

        var em = state.EntityManager;
        var singleton = SystemAPI.GetSingleton<ComboPrefabSingleton>();

        // ==== Spawn SpawnPointTag0 ====
        {
            if (singleton.prefab0 == Entity.Null) { return; }

            var q0 = SystemAPI.QueryBuilder()
                        .WithAll<SpawnPointTag0, LocalTransform>()
                        .Build();
            var entities0 = q0.ToEntityArray(Allocator.Temp);
            var transforms0 = q0.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            UnityEngine.Debug.Log($"Spawn SpawnPointTag0: {entities0.Length}");
            for (int i = 0; i < entities0.Length; i++)
            {
                var e = em.Instantiate(singleton.prefab0);
                em.SetComponentData(e, transforms0[i]);
            }
            entities0.Dispose();
            transforms0.Dispose();
        }

        // ==== Spawn SpawnPointTag3x3 ====
        {
            if (singleton.prefab3x3 == Entity.Null) { return; }

            var q3 = SystemAPI.QueryBuilder()
                        .WithAll<SpawnPointTag3x3, LocalTransform>()
                        .Build();
            var entities3 = q3.ToEntityArray(Allocator.Temp);
            var transforms3 = q3.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            UnityEngine.Debug.Log($"Spawn SpawnPointTag3x3: {entities3.Length}");
            for (int i = 0; i < entities3.Length; i++)
            {
                var e = em.Instantiate(singleton.prefab3x3);
                em.SetComponentData(e, transforms3[i]);
            }
            entities3.Dispose();
            transforms3.Dispose();
        }

        // ==== Spawn SpawnPointTag5x5 ====
        {
            if (singleton.prefab5x5 == Entity.Null) { return; }

            var q5 = SystemAPI.QueryBuilder()
                        .WithAll<SpawnPointTag5x5, LocalTransform>()
                        .Build();
            var entities5 = q5.ToEntityArray(Allocator.Temp);
            var transforms5 = q5.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            UnityEngine.Debug.Log($"Spawn SpawnPointTag5x5: {entities5.Length}");
            for (int i = 0; i < entities5.Length; i++)
            {
                var e = em.Instantiate(singleton.prefab5x5);
                em.SetComponentData(e, transforms5[i]);
            }
            entities5.Dispose();
            transforms5.Dispose();
        }

        // ==== Spawn SpawnPointTag7x7 ====
        {
            if (singleton.prefab7x7 == Entity.Null) { return; }

            var q7 = SystemAPI.QueryBuilder()
                        .WithAll<SpawnPointTag7x7, LocalTransform>()
                        .Build();
            var entities7 = q7.ToEntityArray(Allocator.Temp);
            var transforms7 = q7.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            UnityEngine.Debug.Log($"Spawn SpawnPointTag7x7: {entities7.Length}");
            for (int i = 0; i < entities7.Length; i++)
            {
                var e = em.Instantiate(singleton.prefab7x7);
                em.SetComponentData(e, transforms7[i]);
            }
            entities7.Dispose();
            transforms7.Dispose();
        }

        if (!SystemAPI.HasSingleton<SpawnComboPrefabByTagSystemDone>())
        {
            state.EntityManager.CreateEntity(typeof(SpawnComboPrefabByTagSystemDone));
        }
    }
}

// --- Singleton
public struct SpawnComboPrefabByTagSystemDone : IComponentData { }