using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public partial class FlamePointSpawnerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (!SystemAPI.HasSingleton<FlamePointSpawner>())
            return;
        var spawner = SystemAPI.GetSingleton<FlamePointSpawner>();

        var prefab = spawner.prefab;
        var margin = spawner.margin;
        var pointDensity = spawner.pointDensity;

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (platform, transform, entity) in SystemAPI.Query<RefRO<FlamePointPlatform>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            var size = platform.ValueRO.size;
            var center = platform.ValueRO.center;

            float usableX = size.x - 2 * margin;
            float usableZ = size.z - 2 * margin;
            float area = usableX * usableZ;

            int totalPoints = math.max(1, (int)math.round(area * pointDensity));
            int pointsPerRow = math.max(2, (int)math.round(math.sqrt(totalPoints)));

            float stepX = usableX / (pointsPerRow - 1);
            float stepZ = usableZ / (pointsPerRow - 1);
            float y = center.y + size.y / 2;

            for (int i = 0; i < pointsPerRow; i++)
            {
                for (int j = 0; j < pointsPerRow; j++)
                {
                    float x = center.x - size.x / 2 + margin + i * stepX;
                    float z = center.z - size.z / 2 + margin + j * stepZ;
                    float3 spawnPos = new float3(x, y, z);

                    Entity flame = ecb.Instantiate(prefab);
                    ecb.SetComponent(flame, LocalTransform.FromPosition(spawnPos));
                    ecb.AddComponent(flame, new FlamePoints
                    {
                        detectRadius = 1f,
                        maxHealth = 100f,
                        currentHealth = 100f,
                        dps = 5f,
                        pointContainer = Entity.Null,
                        model = Entity.Null,
                        effect = Entity.Null
                    });
                }
            }
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();

        var singletonEntity = SystemAPI.GetSingletonEntity<FlamePointSpawner>();
        EntityManager.DestroyEntity(singletonEntity);
    }
}