using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Unity.CharacterController;
using Unity.Physics;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Unity.VisualScripting;

[DisallowMultipleComponent]
public class FirstPersonCharacterAuthoring : MonoBehaviour
{
    public GameObject ViewEntity;
    public AuthoringKinematicCharacterProperties CharacterProperties = AuthoringKinematicCharacterProperties.GetDefault();

    public float GroundMaxSpeed = 10f;
    public float GroundedMovementSharpness = 15f;
    public float AirAcceleration = 50f;
    public float AirMaxSpeed = 10f;
    public float AirDrag = 0f;
    public float JumpSpeed = 10f;
    public float3 Gravity = math.up() * -30f;
    public bool PreventAirAccelerationAgainstUngroundedHits = true;
    public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling = BasicStepAndSlopeHandlingParameters.GetDefault();
    public float MinViewAngle = -90f;
    public float MaxViewAngle = 90f;
    
    [Header("InteractionConfigs")]
    public float InteractionReach = 3f;
    [Header("ClimbConfigs")]
    public float ClimbSpeed = 3f;
    [Header("CrawlConfigs")]
    public float CrawlSpeed = 3f;    

    public class Baker : Baker<FirstPersonCharacterAuthoring>
    {
        public override void Bake(FirstPersonCharacterAuthoring authoring)
        {
            KinematicCharacterUtilities.BakeCharacter(this, authoring.gameObject, authoring.CharacterProperties);

            Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

            AddComponent(entity, new FirstPersonCharacterComponent
            {
                GroundMaxSpeed = authoring.GroundMaxSpeed,
                GroundedMovementSharpness = authoring.GroundedMovementSharpness,
                AirAcceleration = authoring.AirAcceleration,
                AirMaxSpeed = authoring.AirMaxSpeed,
                AirDrag = authoring.AirDrag,
                JumpSpeed = authoring.JumpSpeed,
                Gravity = authoring.Gravity,
                PreventAirAccelerationAgainstUngroundedHits = authoring.PreventAirAccelerationAgainstUngroundedHits,
                StepAndSlopeHandling = authoring.StepAndSlopeHandling,
                MinViewAngle = authoring.MinViewAngle,
                MaxViewAngle = authoring.MaxViewAngle,

                ViewEntity = GetEntity(authoring.ViewEntity, TransformUsageFlags.Dynamic),
                ViewPitchDegrees = 0f,
                ViewLocalRotation = quaternion.identity,
                ClimbSpeed = authoring.ClimbSpeed,
                CrawlSpeed = authoring.CrawlSpeed
            });
            AddComponent(entity, new FirstPersonCharacterControl
            {
                CursorLocked = true,
            });
            AddComponent(entity, new InteractionConfig
            {
                ReachRange = authoring.InteractionReach,
            });
            AddComponent(entity, new InteractionData
            {
                InteractableEntity = Entity.Null,
                InteractionPoint = float3.zero,
            });
            AddComponent(entity, new FirstPersonCharacterState
            {
                IsClimbing = false,
                ClimableObjectHeight = 0f,
                IsCrawling = false
            });
        }
    }
}
