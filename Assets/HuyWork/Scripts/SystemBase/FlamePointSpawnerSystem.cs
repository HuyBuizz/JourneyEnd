using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct FlamePointSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlamePointSpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;

        // Lấy cấu hình spawner một lần
        var spawner = SystemAPI.GetSingleton<FlamePointSpawner>();
        var prefab  = spawner.prefab;
        var margin  = math.max(0f, spawner.margin);
        var density = math.max(0.0001f, spawner.pointDensity);

        // Duyệt mọi platform (không cần LocalTransform nếu chỉ tính toán theo size/center)
        foreach (var platform in SystemAPI.Query<RefRO<FlamePointPlatform>>())
        {
            var size   = platform.ValueRO.size;
            var center = platform.ValueRO.center;

            // Vùng sử dụng sau khi trừ margin (clamp nhỏ nhất > 0 để tránh chia 0)
            float usableX = math.max(0.0001f, size.x - 2f * margin);
            float usableZ = math.max(0.0001f, size.z - 2f * margin);
            float area    = usableX * usableZ;

            int totalPoints  = math.max(1, (int)math.round(area * density));
            int pointsPerRow = math.max(2, (int)math.round(math.sqrt(totalPoints)));
            int spawnCount   = pointsPerRow * pointsPerRow;

            float stepX = usableX / (pointsPerRow - 1);
            float stepZ = usableZ / (pointsPerRow - 1);

            // Đặt lưới tại mặt trên của platform
            float y = center.y + size.y * 0.5f;

            // Tính sẵn gốc lưới
            float startX = center.x - size.x * 0.5f + margin;
            float startZ = center.z - size.z * 0.5f + margin;

            // Batch instantiate
            using (var created = new NativeArray<Entity>(spawnCount, Allocator.Temp))
            {
                em.Instantiate(prefab, created);

                int k = 0;
                for (int i = 0; i < pointsPerRow; i++)
                {
                    float x = startX + i * stepX;
                    for (int j = 0; j < pointsPerRow; j++)
                    {
                        float z = startZ + j * stepZ;
                        em.SetComponentData(created[k++], LocalTransform.FromPosition(new float3(x, y, z)));
                    }
                }
            }
        }

        // Hủy singleton để hệ này chỉ chạy một lần
        var singletonEntity = SystemAPI.GetSingletonEntity<FlamePointSpawner>();
        em.DestroyEntity(singletonEntity);
    }
}
