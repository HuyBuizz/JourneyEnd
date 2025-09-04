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
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnPoint>();
        state.RequireForUpdate<Neighbor>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<SpawnPointNeighborBuildSystemDone>()) return;

        var em = state.EntityManager;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var q = SystemAPI.QueryBuilder()
                    .WithAll<SpawnPoint, LocalTransform>()
                    .Build();

        var entities = q.ToEntityArray(Allocator.TempJob);
        var transforms = q.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        int countAll = entities.Length;

        const int K_MAX = 48;
        var tmpDist = new NativeArray<float>(K_MAX, Allocator.Temp);
        var tmpEnt = new NativeArray<Entity>(K_MAX, Allocator.Temp);

        // --- STEP 1: Detect radius
        const int STEP_COUNT = 3; // 3 steps: 7x7, 5x5, 3x3
        // const float detectRadii9x9 = 12.6f;
        const float detectRadii7x7 = 7f;
        const float detectRadii5x5 = 5f;
        const float detectRadii3x3 = 3f;
        const int neighborThreshold7x7 = 48;
        const int neighborThreshold5x5 = 24;
        const int neighborThreshold3x3 = 8;

        // Mảng giữ trạng thái tag tạm thời
        var tagStatus = new NativeArray<int>(countAll, Allocator.Temp); // 0=none, 1=3x3, 2=5x5, 3=7x7

        for (int step = 0; step < STEP_COUNT; step++)
        {
            float r;
            int threshold;

            switch (step)
            {
                case 0: r = detectRadii7x7; threshold = neighborThreshold7x7; break; // 7x7
                case 1: r = detectRadii5x5; threshold = neighborThreshold5x5; break; // 5x5
                default: r = detectRadii3x3; threshold = neighborThreshold3x3; break; // 3x3
            }

            float r2 = r * r;

            for (int i = 0; i < countAll; i++)
            {
                if (step == 1 && tagStatus[i] == 3) continue; 
                if (step == 2 && (tagStatus[i] == 3 || tagStatus[i] == 2)) continue;

                var pos = transforms[i].Position;
                int kCount = 0;

                for (int j = 0; j < countAll; j++)
                {
                    if (i == j) continue;
                    float dx = math.abs(transforms[j].Position.x - pos.x);
                    float dz = math.abs(transforms[j].Position.z - pos.z);
                    if (dx > r || dz > r) continue;

                    float distSq = dx * dx + dz * dz;

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
                if (neighborCount == neighborThreshold7x7) tagStatus[i] = 3; // 7x7
                else if (neighborCount == neighborThreshold5x5) tagStatus[i] = 2; // 5x5
                else if (neighborCount == neighborThreshold3x3 && tagStatus[i] == 0) tagStatus[i] = 1; // 3x3
            }
        }

        // --- STEP 2: Gán tag chính thức
        for (int i = 0; i < countAll; i++)
        {
            ecb.RemoveComponent<SpawnPointTag0>(entities[i]);
            ecb.RemoveComponent<SpawnPointTag3x3>(entities[i]);
            ecb.RemoveComponent<SpawnPointTag5x5>(entities[i]);
            ecb.RemoveComponent<SpawnPointTag7x7>(entities[i]);

            switch (tagStatus[i])
            {
                case 1: ecb.AddComponent(entities[i], new SpawnPointTag3x3()); break;
                case 2: ecb.AddComponent(entities[i], new SpawnPointTag5x5()); break;
                case 3: ecb.AddComponent(entities[i], new SpawnPointTag7x7()); break;
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

        if (!SystemAPI.HasSingleton<SpawnPointNeighborBuildSystemDone>())
        {
            state.EntityManager.CreateEntity(typeof(SpawnPointNeighborBuildSystemDone));
        }
    }
}

// --- Components
public struct SpawnPointTag0 : IComponentData { }
public struct SpawnPointTag3x3 : IComponentData { }
public struct SpawnPointTag5x5 : IComponentData { }
public struct SpawnPointTag7x7 : IComponentData { }

// --- Singleton
public struct SpawnPointNeighborBuildSystemDone : IComponentData { }


