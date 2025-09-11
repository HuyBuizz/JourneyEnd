using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public struct EffectTag : IComponentData { } // Tag component để đánh dấu entity là effect

[BurstCompile]
public partial struct FlameEffectSpawnerSystem : ISystem
{
    private BufferLookup<Child> _childLookup;          // đọc danh sách child của FlamePoint
    private ComponentLookup<EffectTag> _effectLookup;  // kiểm tra child có phải effect không

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlameEffectSingleton>();
        _childLookup  = state.GetBufferLookup<Child>(isReadOnly: true);
        _effectLookup = state.GetComponentLookup<EffectTag>(isReadOnly: true);
    }

    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;
        var spawner = SystemAPI.GetSingleton<FlameEffectSingleton>();
        if (spawner.prefab == Entity.Null) return;

        // Refresh lookups mỗi frame
        _childLookup.Update(ref state);
        _effectLookup.Update(ref state);

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // 1) BURNING = ENABLED  -> SPAWN nếu chưa có effect child
        foreach (var (flamePoint, entity) in SystemAPI
                     .Query<RefRO<FlamePoint>>()
                     .WithAll<Burning>()
                     .WithEntityAccess())
        {
            bool hasEffectChild = false;

            if (_childLookup.HasBuffer(entity))
            {
                var children = _childLookup[entity];
                for (int i = 0; i < children.Length; i++)
                {
                    var child = children[i].Value;
                    if (_effectLookup.HasComponent(child))
                    {
                        hasEffectChild = true;
                        break;
                    }
                }
            }

            if (!hasEffectChild)
            {
                var effect = ecb.Instantiate(spawner.prefab);

                // Gắn tag để lần sau nhận diện là "effect"
                ecb.AddComponent<EffectTag>(effect);

                // Cho effect làm child của FlamePoint
                ecb.AddComponent(effect, new Parent { Value = entity });

                // Đặt local transform = identity để trùng vị trí/orientation với parent
                // (nếu prefab đã có LocalTransform thì ghi đè; nếu chưa có thì thêm)
                ecb.SetComponent(effect, LocalTransform.Identity);
            }
        }

        // 2) BURNING = DISABLED -> XOÁ mọi effect child
        foreach (var (flamePoint, entity) in SystemAPI
                     .Query<RefRO<FlamePoint>>()
                     .WithDisabled<Burning>()
                     .WithEntityAccess())
        {
            if (!_childLookup.HasBuffer(entity)) continue;

            var children = _childLookup[entity];
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (_effectLookup.HasComponent(child))
                {
                    ecb.DestroyEntity(child);
                }
            }
        }

        ecb.Playback(em);
        ecb.Dispose();
    }
}
