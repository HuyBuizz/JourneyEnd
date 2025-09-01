using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

public struct InteractionData : IComponentData
{
    public Entity InteractableEntity;
    public float3 InteractionPoint;
}

public struct InteractionConfig : IComponentData
{
    public float ReachRange;
}

public struct EInteractable : IComponentData { }

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(Unity.Physics.Systems.PhysicsSystemGroup))]
public partial struct InteractionDetectionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var collisionWorld = physicsWorld.CollisionWorld;

        foreach (var (interactionData, interactionConfig, playertransform)
                 in SystemAPI.Query<RefRW<InteractionData>, RefRO<InteractionConfig>, RefRO<LocalTransform>>())
        {
            foreach (var (playerView, playerViewTansform)
                     in SystemAPI.Query<RefRO<FirstPersonCharacterView>, RefRO<LocalTransform>>())
            {
                var origin = playertransform.ValueRO.Position;
                var direction = math.forward(playerViewTansform.ValueRO.Rotation);

                var rayInput = new RaycastInput
                {
                    Start = origin,
                    End = origin + direction * interactionConfig.ValueRO.ReachRange,
                    Filter = new CollisionFilter
                    {
                        BelongsTo = (uint)PhysicsCategory.Character,
                        CollidesWith = (uint)(PhysicsCategory.Interactable | PhysicsCategory.Ground),
                        GroupIndex = 0
                    }
                };

                // Debug thông số Raycast
                UnityEngine.Debug.Log($"Raycast: Origin={origin}, Direction={direction}, ReachRange={interactionConfig.ValueRO.ReachRange}, Filter: BelongsTo={(uint)PhysicsCategory.Character}, CollidesWith={(uint)PhysicsCategory.Ground}");
                UnityEngine.Debug.DrawRay(origin, direction * interactionConfig.ValueRO.ReachRange, UnityEngine.Color.red, 1f);

                if (collisionWorld.CastRay(rayInput, out RaycastHit hit))
                {
                    var hitEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                    interactionData.ValueRW.InteractableEntity = hitEntity;
                    interactionData.ValueRW.InteractionPoint = hit.Position;

                    // Lấy CollisionFilter của entity bị va chạm
                    string categoryName = "Unknown";
                    uint belongsTo = 0;
                    if (SystemAPI.HasComponent<Unity.Physics.PhysicsCollider>(hitEntity))
                    {
                        var pc = SystemAPI.GetComponent<Unity.Physics.PhysicsCollider>(hitEntity);
                        if (pc.Value.IsCreated)
                        {
                            // Lấy filter theo API mới (ổn định hơn giữa các version)
                            Unity.Physics.CollisionFilter filter = pc.Value.Value.GetCollisionFilter(Unity.Physics.ColliderKey.Empty);
                            belongsTo = filter.BelongsTo;
                        }
                    }
                    UnityEngine.Debug.Log($"Hit Entity: {hitEntity}, Position: {hit.Position}, Category: {categoryName} (BelongsTo: {belongsTo})");
                }
                else
                {
                    interactionData.ValueRW.InteractableEntity = Entity.Null;
                    interactionData.ValueRW.InteractionPoint = float3.zero;
                    UnityEngine.Debug.Log("No Raycast hit detected.");
                }
            }
        }
    }

    // Hàm chuyển đổi bitmask thành tên danh mục
    private static string GetCategoryName(uint belongsTo)
    {
        if (belongsTo == 0) return "None";
        if ((belongsTo & (uint)PhysicsCategory.Character) != 0) return "Character";
        if ((belongsTo & (uint)PhysicsCategory.Interactable) != 0) return "Interactable";
        if ((belongsTo & (uint)PhysicsCategory.Ground) != 0) return "Ground";
        return "Unknown";
    }
}