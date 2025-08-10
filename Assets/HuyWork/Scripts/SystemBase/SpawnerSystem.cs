using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public partial class SpawnerSystem : SystemBase
{
    private float spawnTimer = 0f;

    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        spawnTimer += deltaTime;

        if (spawnTimer < 1f)
            return;

        spawnTimer = 0f;

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (spawner, entity) in SystemAPI.Query<RefRO<Spawner>>().WithEntityAccess())
        {
            Entity spawned = ecb.Instantiate(spawner.ValueRO.prefab);
            ecb.AddComponent(spawned, new ETFCube { });
            ecb.AddComponent(spawned, new ECube
            {
                isSmall = false
            });

            float3 randPos = new float3(
                UnityEngine.Random.Range(0f, 5f),
                UnityEngine.Random.Range(0f, 5f),
                UnityEngine.Random.Range(0f, 5f)
            );
            ecb.SetComponent(spawned, LocalTransform.FromPosition(randPos));
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}