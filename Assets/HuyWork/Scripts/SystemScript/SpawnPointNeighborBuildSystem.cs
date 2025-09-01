using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct SpawnPointNeighborBuildSystem : ISystem
{
    public bool hasRun;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnPoint>();
        state.RequireForUpdate<Neighbor>();
        hasRun = false;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
           if (hasRun) return;

    var em = state.EntityManager;
    var ecb = new EntityCommandBuffer(Allocator.Temp);

    var q = SystemAPI.QueryBuilder()
                .WithAll<SpawnPoint, LocalTransform>()
                .Build();

    var entities = q.ToEntityArray(Allocator.TempJob);
    var transforms = q.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

    int countAll = entities.Length;

    const int K_MAX = 32;
    var tmpDist = new NativeArray<float>(K_MAX, Allocator.Temp);
    var tmpEnt = new NativeArray<Entity>(K_MAX, Allocator.Temp);

    // --- STEP 1: Detect radius 6.5 (5x5)
    const int STEP_COUNT = 2;
    const float detectRadii0 = 6.5f;
    const float detectRadii1 = 3.2f;
    const int neighborThreshold0 = 24;
    const int neighborThreshold1 = 8;

    // Mảng giữ trạng thái tag tạm thời
    var tagStatus = new NativeArray<int>(countAll, Allocator.Temp); // 0=none, 1=3x3, 2=5x5

    for (int step = 0; step < STEP_COUNT; step++)
    {
        float r = (step == 0) ? detectRadii0 : detectRadii1;
        float r2 = r * r;
        int threshold = (step == 0) ? neighborThreshold0 : neighborThreshold1;

        for (int i = 0; i < countAll; i++)
        {
            // Nếu đã được gán tag lớn hơn (5x5), bỏ qua bước 3x3
            if (step == 1 && tagStatus[i] == 2) continue;

            var pos = transforms[i].Position;
            int kCount = 0;

            for (int j = 0; j < countAll; j++)
            {
                if (i == j) continue;
                float distSq = math.lengthsq(transforms[j].Position - pos);
                if (distSq > r2) continue;

                // Insert sorted
                int ins = kCount;
                if (kCount < K_MAX)
                {
                    while (ins > 0 && distSq < tmpDist[ins - 1]) ins--;
                    for (int s = kCount; s > ins; s--)
                    {
                        tmpDist[s] = tmpDist[s - 1];
                        tmpEnt[s] = tmpEnt[s - 1];
                    }
                    tmpDist[ins] = distSq;
                    tmpEnt[ins] = entities[j];
                    kCount++;
                }
                else
                {
                    if (distSq >= tmpDist[kCount - 1]) continue;
                    while (ins > 0 && distSq < tmpDist[ins - 1]) ins--;
                    for (int s = kCount - 1; s > ins; s--)
                    {
                        tmpDist[s] = tmpDist[s - 1];
                        tmpEnt[s] = tmpEnt[s - 1];
                    }
                    tmpDist[ins] = distSq;
                    tmpEnt[ins] = entities[j];
                }
            }

            int neighborCount = (kCount >= threshold) ? threshold : 0;

            var buf = em.GetBuffer<Neighbor>(entities[i]);
            if (step == 0) buf.Clear(); // radius lớn xóa neighbor cũ

            for (int t = 0; t < neighborCount; t++)
            {
                buf.Add(new Neighbor { Entity = tmpEnt[t], DistanceSq = tmpDist[t] });
            }

            // Gán tag tạm
            if (neighborCount == neighborThreshold0) tagStatus[i] = 2; // 5x5
            else if (neighborCount == neighborThreshold1 && tagStatus[i] == 0) tagStatus[i] = 1; // 3x3
        }
    }

    // --- STEP 2: Gán tag chính thức
    for (int i = 0; i < countAll; i++)
    {
        ecb.RemoveComponent<SpawnPointTag0>(entities[i]);
        ecb.RemoveComponent<SpawnPointTag3x3>(entities[i]);
        ecb.RemoveComponent<SpawnPointTag5x5>(entities[i]);

        switch (tagStatus[i])
        {
            case 1: ecb.AddComponent(entities[i], new SpawnPointTag3x3()); break;
            case 2: ecb.AddComponent(entities[i], new SpawnPointTag5x5()); break;
            default: ecb.AddComponent(entities[i], new SpawnPointTag0()); break;
        }
    }

    tmpDist.Dispose();
    tmpEnt.Dispose();
    transforms.Dispose();
    entities.Dispose();
    tagStatus.Dispose();

    ecb.Playback(em);
    ecb.Dispose();

    hasRun = true;
    }
}

public struct SpawnPointTag0 : IComponentData {}
public struct SpawnPointTag3x3 : IComponentData {}
public struct SpawnPointTag5x5 : IComponentData {}
