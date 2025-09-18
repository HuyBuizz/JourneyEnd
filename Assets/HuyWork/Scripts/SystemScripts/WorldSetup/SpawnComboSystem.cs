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

    // [BurstCompile]
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

        // Dùng NativeArray<int> và NativeArray<Entity> riêng thay vì struct managed
        NativeArray<Entity> prefabs = new NativeArray<Entity>(4, Allocator.Temp);
        NativeArray<int> amounts = new NativeArray<int>(4, Allocator.Temp);
        NativeArray<ComboPrefabSize> sizes = new NativeArray<ComboPrefabSize>(4, Allocator.Temp);

        prefabs[0] = singleton.prefab0;
        prefabs[1] = singleton.prefab3x3;
        prefabs[2] = singleton.prefab5x5;
        prefabs[3] = singleton.prefab7x7;

        amounts[0] = (int)math.ceil(singleton.amountPref0);
        amounts[1] = (int)math.ceil(singleton.amountPref3x3);
        amounts[2] = (int)math.ceil(singleton.amountPref5x5);
        amounts[3] = (int)math.ceil(singleton.amountPref7x7);

        sizes[0] = ComboPrefabSize.Size0;
        sizes[1] = ComboPrefabSize.Size3x3;
        sizes[2] = ComboPrefabSize.Size5x5;
        sizes[3] = ComboPrefabSize.Size7x7;

        for (int i = 0; i < prefabs.Length; i++)
        {
            var prefab = prefabs[i];
            var amount = amounts[i];
            var size = sizes[i];

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

                if (SystemAPI.IsComponentEnabled<SpawnPointOccupied>(candidate))
                    continue;

                if (!CheckSpaceAvailable(candidateTransform, halfSize, spawnEntities, spawnTransforms))
                    continue;

                var e = em.Instantiate(prefab);
                em.SetComponentData(e, new LocalTransform
                {
                    Position = candidateTransform.Position,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

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

        em.CreateEntity(typeof(SpawnComboSystemDone));

        spawnEntities.Dispose();
        spawnTransforms.Dispose();
        prefabs.Dispose();
        amounts.Dispose();
        sizes.Dispose();
    }

    // Không dùng ref SystemState, chỉ xử lý dữ liệu
    private bool CheckSpaceAvailable(LocalTransform centerTransform, int halfSize,
        NativeArray<Entity> spawnEntities, NativeArray<LocalTransform> spawnTransforms)
    {
        int requiredPoints = (halfSize * 2 + 1) * (halfSize * 2 + 1);
        int availablePoints = 0;

        for (int i = 0; i < spawnEntities.Length; i++)
        {
            var otherTransform = spawnTransforms[i];

            float dx = math.abs(centerTransform.Position.x - otherTransform.Position.x);
            float dz = math.abs(centerTransform.Position.z - otherTransform.Position.z);

            if (dx <= halfSize && dz <= halfSize)
            {
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
