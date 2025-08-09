using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public partial class SpawnerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (spawner, entity) in SystemAPI.Query<RefRO<Spawner>>().WithEntityAccess())
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Entity spawned = ecb.Instantiate(spawner.ValueRO.prefab);
                ecb.AddComponent(spawned, new ETFCube { });

                float3 randPos = new float3(
                    UnityEngine.Random.Range(0f, 5f),
                    UnityEngine.Random.Range(0f, 5f),
                    UnityEngine.Random.Range(0f, 5f)
                );
                ecb.SetComponent(spawned, LocalTransform.FromPosition(randPos));
            }
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}