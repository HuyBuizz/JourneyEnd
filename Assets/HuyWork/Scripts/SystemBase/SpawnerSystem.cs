using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public partial class SpawnerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var (spawner, entity) in SystemAPI.Query<RefRO<Spawner>>().WithEntityAccess())
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Spawning prefab...");
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                Entity spawned = entityManager.Instantiate(spawner.ValueRO.prefab);
                entityManager.SetComponentData(spawned, LocalTransform.FromPosition(new float3(0, 0, 0)));
            }
        }
    }
}