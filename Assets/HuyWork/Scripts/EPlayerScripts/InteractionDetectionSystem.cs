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

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(Unity.Physics.Systems.PhysicsSystemGroup))]
public partial struct InteractionDetectionSystem : ISystem
{
    // [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var collisionWorld = physicsWorld.CollisionWorld;

        foreach (var (interactionData, interactionConfig, playertransform)
                 in SystemAPI.Query<RefRW<InteractionData>, RefRO<InteractionConfig>, RefRO<LocalTransform>>())
        {
            Entity mainEntityCameraEntity = SystemAPI.GetSingletonEntity<MainEntityCamera>();
            LocalToWorld targetLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(mainEntityCameraEntity);

            var origin = playertransform.ValueRO.Position;
            var direction = math.forward(targetLocalToWorld.Rotation);

            var rayInput = new RaycastInput
            {
                Start = origin + 1.4f * math.up(),
                End = origin + direction * interactionConfig.ValueRO.ReachRange,
                Filter = new CollisionFilter
                {
                    BelongsTo = (uint)PhysicsCategory.Character,
                    CollidesWith = (uint)(PhysicsCategory.Interactable),
                    GroupIndex = 0
                }
            };

            // Debug thông số Raycast
            // UnityEngine.Debug.Log($"Raycast: Origin={origin}, Direction={direction}, ReachRange={interactionConfig.ValueRO.ReachRange}, Filter: BelongsTo={(uint)PhysicsCategory.Character}, CollidesWith={(uint)PhysicsCategory.Ground}");
            UnityEngine.Debug.DrawRay(origin + 1.4f * math.up(), direction * interactionConfig.ValueRO.ReachRange, UnityEngine.Color.red, 1f);

            if (collisionWorld.CastRay(rayInput, out RaycastHit hit))
            {
                var hitEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                interactionData.ValueRW.InteractableEntity = hitEntity;
                interactionData.ValueRW.InteractionPoint = hit.Position;

                KeyHintContext.Instance?.SetFlag("hasObjToInteract", true);
            }
            else
            {
                interactionData.ValueRW.InteractableEntity = Entity.Null;
                interactionData.ValueRW.InteractionPoint = float3.zero;
                KeyHintContext.Instance?.SetFlag("hasObjToInteract", false);
            }
        }
    }
}