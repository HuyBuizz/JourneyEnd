using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct FlamePointNeighborBuildSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PointSetupSystemDone>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<FlamePointNeighborBuildSystemDone>()) return;

        var em = state.EntityManager;

        // Gom dữ liệu vào mảng liên tục để duyệt nhanh
        var q = SystemAPI.QueryBuilder().WithAll<FlamePoint, LocalTransform, FlameNeighborSettings, Neighbor>().Build();
        var entities = q.ToEntityArray(Allocator.TempJob);
        var transforms = q.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var points = q.ToComponentDataArray<FlamePoint>(Allocator.TempJob);
        var settings = q.ToComponentDataArray<FlameNeighborSettings>(Allocator.TempJob);

        // Tạo bộ đệm tạm cho top-K (k tối đa 32)
        const int K_MAX = 32;
        var tmpDist = new NativeArray<float>(K_MAX, Allocator.Temp);
        var tmpEnt = new NativeArray<Entity>(K_MAX, Allocator.Temp);

        int countAll = entities.Length;

        for (int i = 0; i < countAll; i++)
        {
            var e = entities[i];
            var pos = transforms[i].Position;
            float r2 = points[i].detectRadius * points[i].detectRadius;
            int kMax = math.clamp(settings[i].maxNeighbors, 1, K_MAX);

            // Làm trống top-K tạm
            int kCount = 0;

            // Dò các điểm khác
            for (int j = 0; j < countAll; j++)
            {
                if (i == j) continue;

                float3 d = transforms[j].Position - pos;
                float distSq = math.lengthsq(d);
                if (distSq > r2) continue;

                // --- Chèn vào danh sách top-K đã sắp xếp tăng dần theo distSq ---
                int ins = kCount;

                if (kCount < kMax)
                {
                    while (ins > 0 && distSq < tmpDist[ins - 1]) { ins--; }
                    for (int s = kCount; s > ins; s--) { tmpDist[s] = tmpDist[s - 1]; tmpEnt[s] = tmpEnt[s - 1]; }
                    tmpDist[ins] = distSq; tmpEnt[ins] = entities[j];
                    kCount++;
                }
                else
                {
                    if (distSq >= tmpDist[kCount - 1]) continue;

                    while (ins > 0 && distSq < tmpDist[ins - 1]) { ins--; }
                    for (int s = kCount - 1; s > ins; s--) { tmpDist[s] = tmpDist[s - 1]; tmpEnt[s] = tmpEnt[s - 1]; }
                    tmpDist[ins] = distSq; tmpEnt[ins] = entities[j];
                }
            }

            // Ghi vào buffer Neighbor (giữ thứ tự gần -> xa)
            var buf = em.GetBuffer<Neighbor>(e);
            buf.Clear();

            for (int t = 0; t < kCount; t++)
            {
                buf.Add(new Neighbor { Entity = tmpEnt[t], DistanceSq = tmpDist[t] });
            }
        }

        tmpEnt.Dispose();
        tmpDist.Dispose();
        settings.Dispose();
        points.Dispose();
        transforms.Dispose();
        entities.Dispose();

        if (!SystemAPI.HasSingleton<FlamePointNeighborBuildSystemDone>())
        {
            em.CreateEntity(typeof(FlamePointNeighborBuildSystemDone));
        }

    }
}
public struct FlamePointNeighborBuildSystemDone : IComponentData { }
