using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct SpawnComboSystem : ISystem
{
    public struct ComboSpawnData
    {
        public Entity prefab;
        public int amount;
        public ComboPrefabSize size;
    }

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

        // Dùng NativeArray thay cho ValueTuple[]
        var comboList = new NativeArray<ComboSpawnData>(4, Allocator.Temp);
        comboList[0] = new ComboSpawnData { prefab = singleton.prefab0, amount = Mathf.CeilToInt(singleton.amountPref0), size = ComboPrefabSize.Size0 };
        comboList[1] = new ComboSpawnData { prefab = singleton.prefab3x3, amount = Mathf.CeilToInt(singleton.amountPref3x3), size = ComboPrefabSize.Size3x3 };
        comboList[2] = new ComboSpawnData { prefab = singleton.prefab5x5, amount = Mathf.CeilToInt(singleton.amountPref5x5), size = ComboPrefabSize.Size5x5 };
        comboList[3] = new ComboSpawnData { prefab = singleton.prefab7x7, amount = Mathf.CeilToInt(singleton.amountPref7x7), size = ComboPrefabSize.Size7x7 };

        for (int i = 0; i < comboList.Length; i++)
        {
            var prefab = comboList[i].prefab;
            var amount = comboList[i].amount;
            var size = comboList[i].size;

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
        comboList.Dispose();
    }

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
                if (SystemAPI.IsComponentEnabled<SpawnPointOccupied>(other))
                    return false;

                availablePoints++;
            }
        }

        return availablePoints == requiredPoints;
    }

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
