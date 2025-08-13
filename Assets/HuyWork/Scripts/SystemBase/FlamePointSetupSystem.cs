using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public struct DoneProcess : IComponentData { }

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct FloorTagSetup : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (children, houseEntity) in
                 SystemAPI.Query<DynamicBuffer<Child>>()
                          .WithAll<House>()
                          .WithNone<DoneProcess>()
                          .WithEntityAccess())
        {
            foreach (var child in children)
            {
                // Thêm Tag Floor cho lớp con đầu con của House
                ecb.AddComponent<Floor>(child.Value);
            }

            ecb.AddComponent<DoneProcess>(houseEntity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public partial struct FloorPartTagSetup : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (children, floorEntity) in
                 SystemAPI.Query<DynamicBuffer<Child>>()
                          .WithAll<Floor>()
                          .WithNone<DoneProcess>()
                          .WithEntityAccess())
        {
            foreach (var child in children)
            {
                if (!state.EntityManager.HasBuffer<Child>(child.Value))
                {
                    ecb.AddComponent<Part>(child.Value);
                }
            }

            ecb.AddComponent<DoneProcess>(floorEntity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
