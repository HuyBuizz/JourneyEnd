using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public struct InteractPressLatch : IComponentData
{
    public byte  WaitingForRelease;
    public float Cooldown; // giây
}

// (tuỳ chọn) cho phép đặt cooldown riêng trên từng interactable
public struct InteractionCooldown : IComponentData
{
    public float Seconds;
}

// [BurstCompile]
public partial struct InteractionHandlingSystem : ISystem
{
    private EntityQuery _missingLatchQ;
    private ComponentLookup<EInteractable>     _interactableLookupRO;
    private ComponentLookup<RenderBounds>      _rbLookupRO;
    private ComponentLookup<LocalToWorld>      _l2wLookupRO;
    private ComponentLookup<LocalTransform>    _ltLookupRO;
    private ComponentLookup<InteractionCooldown> _cooldownLookupRO; // NEW

    // [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(
            SystemAPI.QueryBuilder()
                .WithAll<InteractionData, FirstPersonCharacterControl>()
                .Build()
        );

        _missingLatchQ = SystemAPI.QueryBuilder()
            .WithAll<InteractionData, FirstPersonCharacterControl>()
            .WithNone<InteractPressLatch>()
            .Build();

        _interactableLookupRO  = state.GetComponentLookup<EInteractable>(true);
        _rbLookupRO            = state.GetComponentLookup<RenderBounds>(true);
        _l2wLookupRO           = state.GetComponentLookup<LocalToWorld>(true);
        _ltLookupRO            = state.GetComponentLookup<LocalTransform>(true);
        _cooldownLookupRO      = state.GetComponentLookup<InteractionCooldown>(true); // NEW
    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        if (!math.isfinite(dt) || dt <= 0f || dt > 0.2f) return;

        _interactableLookupRO.Update(ref state);
        _rbLookupRO.Update(ref state);
        _l2wLookupRO.Update(ref state);
        _ltLookupRO.Update(ref state);
        _cooldownLookupRO.Update(ref state); // NEW

        var interactableLkp = _interactableLookupRO;
        var rbLkp           = _rbLookupRO;
        var l2wLkp          = _l2wLookupRO;
        var ltLkp           = _ltLookupRO;
        var cdLkp           = _cooldownLookupRO; // NEW

        var ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        if (!_missingLatchQ.IsEmptyIgnoreFilter)
        {
            ecb.AddComponent(_missingLatchQ, new InteractPressLatch { WaitingForRelease = 0, Cooldown = 0f });
        }

        foreach (var (idata, input, latch, chrState, lt, entity) in SystemAPI
                     .Query<RefRO<InteractionData>, RefRO<FirstPersonCharacterControl>,
                            RefRW<InteractPressLatch>, RefRW<FirstPersonCharacterState>, RefRW<LocalTransform>>()
                     .WithEntityAccess())
        {
            // giảm cooldown
            if (latch.ValueRO.Cooldown > 0f)
                latch.ValueRW.Cooldown = math.max(0f, latch.ValueRO.Cooldown - dt);

            bool pressed = input.ValueRO.Interact;

            // Nhả phím -> hạ latch
            if (!pressed)
            {
                latch.ValueRW.WaitingForRelease = 0;
                continue;
            }

            // Đang giữ phím hoặc còn cooldown -> bỏ qua
            if (latch.ValueRO.WaitingForRelease != 0 || latch.ValueRO.Cooldown > 0f)
                continue;

            float nextCd = 0f; // mặc định không khóa (Takeable có thể spam)

            Entity target = idata.ValueRO.InteractableEntity;
            if (target != Entity.Null && interactableLkp.HasComponent(target))
            {
                var interactable = interactableLkp[target];

                switch (interactable.eInteractableType)
                {
                    case EInteractableType.Takeable:
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.Log("Take item!");
                    #endif
                        // nextCd = 0f; // cho phép nhặt nhiều lần nhanh
                        break;

                    case EInteractableType.Storage:
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.Log("Store item!");
                    #endif
                        nextCd = 0.20f; // tránh mở/đóng UI 2 lần
                        break;

                    case EInteractableType.Climb:
                        if (!chrState.ValueRO.IsClimbing)
                        {
                            lt.ValueRW.Position += 1f * math.up(); // nudge nhỏ
                            chrState.ValueRW.IsClimbing = true;
                            chrState.ValueRW.ClimableObjectHeight =
                                ClimbHelper.GetTopY(target, rbLkp, l2wLkp, ltLkp);
                            nextCd = 0.25f; // cho physics/KCC ổn định
                        }
                        break;

                    default:
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.Log("Unknown interaction type!");
                    #endif
                        nextCd = 0.10f;
                        break;
                }

                // Nếu target đặt cooldown riêng → ưu tiên
                if (cdLkp.HasComponent(target))
                {
                    var custom = math.max(0f, cdLkp[target].Seconds);
                    nextCd = custom;
                }
            }

            // Bật latch + áp cooldown (nếu có)
            latch.ValueRW.WaitingForRelease = 1;
            latch.ValueRW.Cooldown = nextCd;
        }
    }
}

public static class ClimbHelper
{
    public static float GetTopY(
        Entity e,
        ComponentLookup<RenderBounds> rbLkp,
        ComponentLookup<LocalToWorld> l2wLkp,
        ComponentLookup<LocalTransform> ltLkp)
    {
        if (rbLkp.HasComponent(e) && l2wLkp.HasComponent(e))
        {
            var rb  = rbLkp[e].Value;
            var l2w = l2wLkp[e].Value;

            float centerWy = l2w.c0.y * rb.Center.x + l2w.c1.y * rb.Center.y + l2w.c2.y * rb.Center.z + l2w.c3.y;
            float exY = math.abs(l2w.c0.y) * rb.Extents.x
                      + math.abs(l2w.c1.y) * rb.Extents.y
                      + math.abs(l2w.c2.y) * rb.Extents.z;

            return centerWy + exY;
        }
        else if (ltLkp.HasComponent(e))
        {
            return ltLkp[e].Position.y;
        }
        return 0f;
    }
}
