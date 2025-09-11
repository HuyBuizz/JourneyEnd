using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

// Nếu muốn dùng trực tiếp Size0, Size3x3... thì dùng:
// using static ComboPrefabSize;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct SpawnComboSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Hệ thống chỉ chạy khi có singleton và PointSetup đã xong
        state.RequireForUpdate<ComboPrefabSingleton>();
        state.RequireForUpdate<PointSetupSystemDone>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<SpawnComboSystemDone>()) return;

        var em = state.EntityManager;
        var singleton = SystemAPI.GetSingleton<ComboPrefabSingleton>();

        var spawnQuery = SystemAPI.QueryBuilder()
                            .WithAll<SpawnPoint, LocalTransform>()
                            .Build();

        var spawnEntities = spawnQuery.ToEntityArray(Allocator.Temp);
        var spawnTransforms = spawnQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        if (spawnEntities.Length == 0)
        {
            spawnEntities.Dispose();
            spawnTransforms.Dispose();
            Debug.LogWarning("Không có spawn point nào.");
            return;
        }

        var rng = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

        var comboList = new (Entity prefab, int amount, ComboPrefabSize size)[]
        {
            (singleton.prefab0, Mathf.CeilToInt(singleton.amountPref0), ComboPrefabSize.Size0),
            (singleton.prefab3x3, Mathf.CeilToInt(singleton.amountPref3x3), ComboPrefabSize.Size3x3),
            (singleton.prefab5x5, Mathf.CeilToInt(singleton.amountPref5x5), ComboPrefabSize.Size5x5),
            (singleton.prefab7x7, Mathf.CeilToInt(singleton.amountPref7x7), ComboPrefabSize.Size7x7)
        };

        foreach (var (prefab, amount, size) in comboList)
        {
            if (prefab == Entity.Null || amount <= 0) continue;

            int placedCount = 0;
            int maxAttempts = 500;
            int attempts = 0;
            int halfSize = GetHalfSize(size);

            while (placedCount < amount && attempts < maxAttempts)
            {
                attempts++;

                int index = rng.NextInt(0, spawnEntities.Length);
                var candidate = spawnEntities[index];
                var candidateTransform = spawnTransforms[index];

                // Nếu spawn point trung tâm đã bị chiếm → bỏ
                if (SystemAPI.IsComponentEnabled<SpawnPointOccupied>(candidate))
                    continue;

                // Kiểm tra vùng đủ điểm và chưa bị occupied
                if (!CheckSpaceAvailable(ref state, candidateTransform, halfSize, spawnEntities, spawnTransforms))
                    continue;

                // Spawn prefab
                var e = em.Instantiate(prefab);
                em.SetComponentData(e, new LocalTransform
                {
                    Position = candidateTransform.Position,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                // Đánh dấu tất cả spawn points trong vùng chiếm là occupied
                MarkOccupied(ref state, candidateTransform, halfSize, spawnEntities, spawnTransforms);

                placedCount++;

                // Giảm số lượng trong singleton
                switch (size)
                {
                    case ComboPrefabSize.Size0: singleton.amountPref0--; break;
                    case ComboPrefabSize.Size3x3: singleton.amountPref3x3--; break;
                    case ComboPrefabSize.Size5x5: singleton.amountPref5x5--; break;
                    case ComboPrefabSize.Size7x7: singleton.amountPref7x7--; break;
                }

                SystemAPI.SetSingleton(singleton);
            }

            if (placedCount < amount)
            {
                Debug.LogWarning($"Không thể đặt đủ prefab {size}. Đã đặt {placedCount}/{amount}.");
            }
        }

        // Tạo flag đã spawn xong
        em.CreateEntity(typeof(SpawnComboSystemDone));

        spawnEntities.Dispose();
        spawnTransforms.Dispose();
    }

    // Kiểm tra cả vùng xung quanh candidate
    private bool CheckSpaceAvailable(ref SystemState state, LocalTransform centerTransform, int halfSize,
        NativeArray<Entity> spawnEntities, NativeArray<LocalTransform> spawnTransforms)
    {
        int requiredPoints = (halfSize * 2 + 1) * (halfSize * 2 + 1);
        int availablePoints = 0;

        for (int i = 0; i < spawnEntities.Length; i++)
        {
            var other = spawnEntities[i];
            var otherTransform = spawnTransforms[i];

            float dx = math.abs(centerTransform.Position.x - otherTransform.Position.x);
            float dz = math.abs(centerTransform.Position.z - otherTransform.Position.z);

            if (dx <= halfSize && dz <= halfSize)
            {
                // Nếu một điểm bị chiếm → vùng không hợp lệ
                if (SystemAPI.IsComponentEnabled<SpawnPointOccupied>(other))
                    return false;

                availablePoints++;
            }
        }

        // Kiểm tra vùng có đủ điểm chưa
        return availablePoints == requiredPoints;
    }

    // Đánh dấu toàn bộ vùng là occupied
    private void MarkOccupied(ref SystemState state, LocalTransform centerTransform, int halfSize,
        NativeArray<Entity> spawnEntities, NativeArray<LocalTransform> spawnTransforms)
    {
        for (int i = 0; i < spawnEntities.Length; i++)
        {
            var other = spawnEntities[i];
            var otherTransform = spawnTransforms[i];

            float dx = math.abs(centerTransform.Position.x - otherTransform.Position.x);
            float dz = math.abs(centerTransform.Position.z - otherTransform.Position.z);

            if (dx <= halfSize && dz <= halfSize)
            {
                SystemAPI.SetComponentEnabled<SpawnPointOccupied>(other, true);
            }
        }
    }

    private int GetHalfSize(ComboPrefabSize size)
    {
        return size switch
        {
            ComboPrefabSize.Size0 => 0,
            ComboPrefabSize.Size3x3 => 1,
            ComboPrefabSize.Size5x5 => 2,
            ComboPrefabSize.Size7x7 => 3,
            _ => 0
        };
    }
}

// Component flag hệ thống đã spawn xong
public struct SpawnComboSystemDone : IComponentData { }
