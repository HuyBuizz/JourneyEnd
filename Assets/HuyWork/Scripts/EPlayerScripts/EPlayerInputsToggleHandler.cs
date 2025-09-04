using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct CrawlPressLatch : IComponentData
{
    public byte  WaitingForRelease; 
    public float Cooldown;          
}

public struct CrawlingActive : IComponentData, IEnableableComponent {}

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct CrawlToggleSystem : ISystem
{
    private EntityQuery _needLatchQ;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(
            SystemAPI.QueryBuilder()
                .WithAll<FirstPersonCharacterControl, FirstPersonCharacterState>()
                .Build()
        );

        // Batch add latch qua query (khỏi foreach)
        _needLatchQ = SystemAPI.QueryBuilder()
            .WithAll<FirstPersonCharacterControl, FirstPersonCharacterState>()
            .WithNone<CrawlPressLatch>()
            .Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        // Guard dt (bật nếu hay hitch)
        // if (!math.isfinite(dt) || dt <= 0f || dt > 0.2f) return;

        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // Thêm latch cho tất cả entity thiếu trong 1 lệnh
        if (!_needLatchQ.IsEmptyIgnoreFilter)
        {
            ecb.AddComponent(_needLatchQ, new CrawlPressLatch { WaitingForRelease = 0, Cooldown = 0f });
        }

        // Toggle crawl (edge-detect)
        foreach (var (control, chrState, latch, entity) in
                 SystemAPI.Query<RefRO<FirstPersonCharacterControl>,
                                 RefRW<FirstPersonCharacterState>,
                                 RefRW<CrawlPressLatch>>()
                          .WithEntityAccess())
        {
            // Cooldown
            if (latch.ValueRO.Cooldown > 0f)
                latch.ValueRW.Cooldown = math.max(0f, latch.ValueRO.Cooldown - dt);

            bool pressed = control.ValueRO.Crawl;

            // Nhả phím -> hạ latch
            if (!pressed)
            {
                latch.ValueRW.WaitingForRelease = 0;
                continue;
            }

            // Đang giữ phím hoặc còn cooldown -> bỏ
            if (latch.ValueRO.WaitingForRelease != 0 || latch.ValueRO.Cooldown > 0f)
                continue;

            // ==== Toggle ====
            bool newIsCrawling = !chrState.ValueRO.IsCrawling;
            chrState.ValueRW.IsCrawling = newIsCrawling;

            if (newIsCrawling)
            {
                // Mutual exclusive
                chrState.ValueRW.IsClimbing = false;
            }

            // Đồng bộ enableable tag (ít đụng EntityManager)
            if (SystemAPI.HasComponent<CrawlingActive>(entity))
            {
                state.EntityManager.SetComponentEnabled<CrawlingActive>(entity, newIsCrawling);
            }
            else if (newIsCrawling)
            {
                // Chỉ add khi bật lần đầu
                ecb.AddComponent<CrawlingActive>(entity);
            }

            // Arm latch + debounce
            latch.ValueRW.WaitingForRelease = 1;
            latch.ValueRW.Cooldown = 0.15f;
        }
    }
}
