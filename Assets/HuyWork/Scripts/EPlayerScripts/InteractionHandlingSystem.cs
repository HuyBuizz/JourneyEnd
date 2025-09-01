// // 9/1/2025 AI-Tag
// // This was created with the help of Assistant, a Unity Artificial Intelligence product.

// using Unity.Burst;
// using Unity.Entities;
// using UnityEngine;

// [BurstCompile]
// public partial struct InteractionHandlingSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         foreach (var (interactionData, input) in SystemAPI.Query<RefRO<InteractionData>, RefRO<PlayerInputData>>())
//         {
//             if (input.ValueRO.InteractTriggered && interactionData.ValueRO.InteractableEntity != Entity.Null)
//             {
//                 // Lấy component của đối tượng tương tác
//                 var interactableEntity = interactionData.ValueRO.InteractableEntity;

//                 if (SystemAPI.HasComponent<Interactable>(interactableEntity))
//                 {
//                     var interactable = SystemAPI.GetComponent<Interactable>(interactableEntity);

//                     // Xử lý logic tương tác
//                     switch (interactable.Type)
//                     {
//                         case InteractableType.Takeable:
//                             Debug.Log("Take item!");
//                             break;
//                         case InteractableType.Storage:
//                             Debug.Log("Store item!");
//                             break;
//                         case InteractableType.Climb:
//                             Debug.Log("Climb!");
//                             break;
//                         default:
//                             Debug.Log("Unknown interaction type!");
//                             break;
//                     }
//                 }
//             }
//         }
//     }
// }