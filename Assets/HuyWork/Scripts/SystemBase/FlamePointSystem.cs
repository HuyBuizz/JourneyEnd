// using Unity.Burst;
// using Unity.Entities;
// using Unity.Transforms;
// using Unity.Mathematics;
// using Unity.Collections;

// [BurstCompile]
// public partial struct FlamePointSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         var deltaTime = SystemAPI.Time.DeltaTime;

//         // Query tất cả FlamePoint
//         foreach (var (flamePoint, transform, entity) in SystemAPI.Query<RefRW<FlamePoint>, RefRO<LocalTransform>>().WithEntityAccess())
//         {
//             // Detect các FlamePoint khác trong bán kính detectRadius
//             var pos = transform.ValueRO.Position;
//             var detectRadius = flamePoint.ValueRO.detectRadius;

//             // Đơn giản: duyệt tất cả FlamePoint khác (có thể tối ưu spatial query sau)
//             foreach (var (otherFlame, otherTransform, otherEntity) in SystemAPI.Query<RefRW<FlamePoint>, RefRO<LocalTransform>>().WithEntityAccess())
//             {
//                 if (entity == otherEntity) continue;
//                 float dist = math.distance(pos, otherTransform.ValueRO.Position);
//                 if (dist <= detectRadius)
//                 {
//                     // InflictFireDamage logic
//                     if (flamePoint.ValueRO.currentHealth >= flamePoint.ValueRO.maxHealth / 2)
//                     {
//                         if (otherFlame.ValueRO.currentHealth < otherFlame.ValueRO.maxHealth)
//                         {
//                             otherFlame.ValueRW.currentHealth = math.min(
//                                 otherFlame.ValueRO.currentHealth + flamePoint.ValueRO.dps * deltaTime,
//                                 otherFlame.ValueRO.maxHealth
//                             );
//                         }
//                         else
//                         {
//                             otherFlame.ValueRW.currentHealth = otherFlame.ValueRO.maxHealth;
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }