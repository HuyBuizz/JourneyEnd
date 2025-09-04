using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public partial struct FlameSpreadViaNeighborsSystem : ISystem
{
    // Tra cứu dữ liệu theo Entity
    private ComponentLookup<FlamePoint> flamePointLookup;
    private BufferLookup<Neighbor> neighborBufferLookup;
    private ComponentLookup<Burning> burningTagLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlamePoint>();
        flamePointLookup = state.GetComponentLookup<FlamePoint>(isReadOnly: false);   // đọc/ghi FlamePoint
        neighborBufferLookup = state.GetBufferLookup<Neighbor>(isReadOnly: true);  // đọc danh sách Neighbor
        burningTagLookup = state.GetComponentLookup<Burning>(isReadOnly: true); // kiểm tra bật/tắt Burning
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        flamePointLookup.Update(ref state);
        neighborBufferLookup.Update(ref state);
        burningTagLookup.Update(ref state);

        float deltaTime = SystemAPI.Time.DeltaTime;
        const float igniteThresholdFraction = 0.5f;

        // DUYỆT THEO "MỤC TIÊU": mỗi FlamePoint tự kiểm tra hàng xóm của mình
        foreach (var (targetFlamePointRefReadWrite, targetEntity) in SystemAPI
                     .Query<RefRW<FlamePoint>>()
                     .WithEntityAccess())
        {
            var targetFlamePoint = targetFlamePointRefReadWrite.ValueRO;

            // Tính sát thương nhận trong frame này = MAX dps của các neighbor đang Burning
            float maxNeighborDamagePerSecond = 0f;

            if (neighborBufferLookup.HasBuffer(targetEntity))
            {
                var neighborBuffer = neighborBufferLookup[targetEntity];

                for (int i = 0; i < neighborBuffer.Length; i++)
                {
                    Entity neighborEntity = neighborBuffer[i].Entity;

                    // Neighbor phải có tag Burning và đang enabled
                    bool neighborIsBurning =
                        burningTagLookup.HasComponent(neighborEntity) &&
                        burningTagLookup.IsComponentEnabled(neighborEntity);

                    if (!neighborIsBurning) continue;

                    // Đọc dps của neighbor (đổi "damagePerSecond" nếu struct của bạn đang dùng "dps")
                    if (!flamePointLookup.HasComponent(neighborEntity)) continue;
                    var neighborFlamePointRefReadOnly = flamePointLookup.GetRefRO(neighborEntity);
                    float neighborDamagePerSecond = neighborFlamePointRefReadOnly.ValueRO.dps;

                    if (neighborDamagePerSecond > maxNeighborDamagePerSecond)
                        maxNeighborDamagePerSecond = neighborDamagePerSecond;
                }
            }

            // Áp sát thương 1 lần theo MAX (không cộng dồn)
            if (maxNeighborDamagePerSecond > 0f)
            {
                float newCurrentHealth = math.min(
                    targetFlamePoint.currentHealth + maxNeighborDamagePerSecond * deltaTime,
                    targetFlamePoint.maxHealth
                );
                targetFlamePointRefReadWrite.ValueRW.currentHealth = newCurrentHealth;
            }

            // Cập nhật trạng thái cháy của chính target
            bool isTargetHot = targetFlamePointRefReadWrite.ValueRO.currentHealth
                               >= targetFlamePointRefReadWrite.ValueRO.maxHealth * igniteThresholdFraction;

            targetFlamePointRefReadWrite.ValueRW.onFire = isTargetHot;

            // Nếu entity có component Burning, đồng bộ enable/disable theo ngưỡng
            // (Nếu bạn muốn “đã cháy thì không tắt”, hãy bỏ dòng dưới và tự áp logic latch riêng)
            if (state.EntityManager.HasComponent<Burning>(targetEntity))
            {
                state.EntityManager.SetComponentEnabled<Burning>(targetEntity, isTargetHot);
            }
        }
    }
}
