using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.CharacterController;
using UnityEngine;

public struct FirstPersonCharacterUpdateContext
{
    public void OnSystemCreate(ref SystemState state)
    {
    }

    public void OnSystemUpdate(ref SystemState state)
    {
    }
}

public readonly partial struct FirstPersonCharacterAspect : IAspect, IKinematicCharacterProcessor<FirstPersonCharacterUpdateContext>
{
    public readonly KinematicCharacterAspect CharacterAspect;
    public readonly RefRW<FirstPersonCharacterComponent> CharacterComponent;
    public readonly RefRW<FirstPersonCharacterControl> CharacterControl;
    public readonly RefRW<FirstPersonCharacterState> CharacterState;
    public readonly RefRW<PhysicsCollider> physicsCollider;
    public readonly RefRW<CharacterColliderVariants> characterColliderVariants;

    public void PhysicsUpdate(ref FirstPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref FirstPersonCharacterComponent characterComponent = ref CharacterComponent.ValueRW;
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;

        CharacterAspect.Update_Initialize(in this, ref context, ref baseContext, ref characterBody, baseContext.Time.DeltaTime);
        CharacterAspect.Update_ParentMovement(in this, ref context, ref baseContext, ref characterBody, ref characterPosition, characterBody.WasGroundedBeforeCharacterUpdate);
        CharacterAspect.Update_Grounding(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);

        HandleVelocityControl(ref context, ref baseContext);

        CharacterAspect.Update_PreventGroundingFromFutureSlopeChange(in this, ref context, ref baseContext, ref characterBody, in characterComponent.StepAndSlopeHandling);
        CharacterAspect.Update_GroundPushing(in this, ref context, ref baseContext, characterComponent.Gravity);
        CharacterAspect.Update_MovementAndDecollisions(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);
        CharacterAspect.Update_MovingPlatformDetection(ref baseContext, ref characterBody);
        CharacterAspect.Update_ParentMomentum(ref baseContext, ref characterBody);
        CharacterAspect.Update_ProcessStatefulCharacterHits();
    }

    private void HandleVelocityControl(ref FirstPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        float deltaTime = baseContext.Time.DeltaTime;
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref FirstPersonCharacterComponent characterComponent = ref CharacterComponent.ValueRW;
        ref FirstPersonCharacterControl characterControl = ref CharacterControl.ValueRW;
        ref FirstPersonCharacterState characterState = ref CharacterState.ValueRW;
        ref PhysicsCollider physicsCollider = ref this.physicsCollider.ValueRW;
        ref CharacterColliderVariants characterColliderVariants = ref this.characterColliderVariants.ValueRW;

        // Change collider when switching character physicsstates
        if (characterState.IsCrawling && !characterState.CrawlColliderShrunk)
        {
            physicsCollider.Value = characterColliderVariants.Crawling;
            characterState.CrawlColliderShrunk = true;
        }
        else if (!characterState.IsCrawling && characterState.CrawlColliderShrunk)
        {
            physicsCollider.Value = characterColliderVariants.Standing;
            characterState.CrawlColliderShrunk = false;
        }

        if (characterBody.ParentEntity != Entity.Null)
        {
            characterControl.MoveVector = math.rotate(characterBody.RotationFromParent, characterControl.MoveVector);
            characterBody.RelativeVelocity = math.rotate(characterBody.RotationFromParent, characterBody.RelativeVelocity);
        }

        if (characterState.IsClimbing)
        {
            // Exit climbing if grounded
            if (characterBody.IsGrounded)
            {
                characterState.IsClimbing = false;
                characterState.ClimableObjectHeight = 0f;
                return;
            }

            // Exit climbing if reaching the top
            float currentY = CharacterAspect.LocalTransform.ValueRO.Position.y;
            if (characterState.ClimableObjectHeight > 0f &&
                currentY >= characterState.ClimableObjectHeight + 1f)
            {
                characterState.IsClimbing = false;
                characterState.ClimableObjectHeight = 0f;
                characterBody.RelativeVelocity = float3.zero; // Reset velocity
                return;
            }

            // Get vertical input (W/S)
            float climbInput = characterControl.MoveInput.y;

            // Climbing velocity: only along Up axis
            characterBody.RelativeVelocity = math.up() * (climbInput * characterComponent.ClimbSpeed);

            // Exit climbing when jumping
            if (characterControl.Jump)
            {
                characterState.IsClimbing = false;
                characterState.ClimableObjectHeight = 0f;

                CharacterControlUtilities.StandardJump(
                    ref characterBody,
                    math.up() * characterComponent.JumpSpeed,
                    true,
                    math.up()
                );
                return;
            }

            return;
        }

        if (characterBody.IsGrounded)
        {
            if (characterState.IsCrawling)
            {
                // Use crawl speed instead of normal ground speed
                float3 targetVelocity = characterControl.MoveVector * characterComponent.CrawlSpeed;
                CharacterControlUtilities.StandardGroundMove_Interpolated(
                    ref characterBody.RelativeVelocity,
                    targetVelocity,
                    characterComponent.GroundedMovementSharpness,
                    deltaTime,
                    characterBody.GroundingUp,
                    characterBody.GroundHit.Normal);

                // Exit crawling if jump is triggered
                if (characterControl.Jump)
                {
                    characterState.IsCrawling = false;
                    if (characterState.CrawlColliderShrunk)
                    {
                        physicsCollider.Value = characterColliderVariants.Standing;
                        characterState.CrawlColliderShrunk = false;
                    }
                }
            }
            else
            {
                float3 targetVelocity = characterControl.MoveVector * characterComponent.GroundMaxSpeed;
                CharacterControlUtilities.StandardGroundMove_Interpolated(
                    ref characterBody.RelativeVelocity,
                    targetVelocity,
                    characterComponent.GroundedMovementSharpness,
                    deltaTime,
                    characterBody.GroundingUp,
                    characterBody.GroundHit.Normal);

                if (characterControl.Jump)
                {
                    CharacterControlUtilities.StandardJump(
                        ref characterBody,
                        characterBody.GroundingUp * characterComponent.JumpSpeed,
                        true,
                        characterBody.GroundingUp);
                }
            }
        }
        else
        {
            float3 airAcceleration = characterControl.MoveVector * characterComponent.AirAcceleration;
            if (math.lengthsq(airAcceleration) > 0f)
            {
                float3 tmpVelocity = characterBody.RelativeVelocity;
                CharacterControlUtilities.StandardAirMove(
                    ref characterBody.RelativeVelocity,
                    airAcceleration,
                    characterComponent.AirMaxSpeed,
                    characterBody.GroundingUp,
                    deltaTime,
                    false);

                if (characterComponent.PreventAirAccelerationAgainstUngroundedHits &&
                    CharacterAspect.MovementWouldHitNonGroundedObstruction(
                        in this,
                        ref context,
                        ref baseContext,
                        characterBody.RelativeVelocity * deltaTime,
                        out ColliderCastHit hit))
                {
                    characterBody.RelativeVelocity = tmpVelocity;
                }
            }

            CharacterControlUtilities.AccelerateVelocity(
                ref characterBody.RelativeVelocity,
                characterComponent.Gravity,
                deltaTime);
            CharacterControlUtilities.ApplyDragToVelocity(
                ref characterBody.RelativeVelocity,
                deltaTime,
                characterComponent.AirDrag);
        }
    }

    public void VariableUpdate(ref FirstPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref FirstPersonCharacterComponent characterComponent = ref CharacterComponent.ValueRW;
        ref FirstPersonCharacterControl characterControl = ref CharacterControl.ValueRW;
        ref quaternion characterRotation = ref CharacterAspect.LocalTransform.ValueRW.Rotation;

        KinematicCharacterUtilities.AddVariableRateRotationFromFixedRateRotation(
            ref characterRotation,
            characterBody.RotationFromParent,
            baseContext.Time.DeltaTime,
            characterBody.LastPhysicsUpdateDeltaTime);

        FirstPersonCharacterUtilities.ComputeFinalRotationsFromRotationDelta(
            ref characterRotation,
            ref characterComponent.ViewPitchDegrees,
            characterControl.LookDegreesDelta,
            0f,
            characterComponent.MinViewAngle,
            characterComponent.MaxViewAngle,
            out float canceledPitchDegrees,
            out characterComponent.ViewLocalRotation);
    }

    public void UpdateGroundingUp(ref FirstPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        CharacterAspect.Default_UpdateGroundingUp(ref characterBody);
    }

    public bool CanCollideWithHit(ref FirstPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in BasicHit hit)
    {
        return PhysicsUtilities.IsCollidable(hit.Material);
    }

    public bool IsGroundedOnHit(ref FirstPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in BasicHit hit, int groundingEvaluationType)
    {
        FirstPersonCharacterComponent characterComponent = CharacterComponent.ValueRO;
        return CharacterAspect.Default_IsGroundedOnHit(
            in this,
            ref context,
            ref baseContext,
            in hit,
            in characterComponent.StepAndSlopeHandling,
            groundingEvaluationType);
    }

    public void OnMovementHit(
        ref FirstPersonCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref KinematicCharacterHit hit,
        ref float3 remainingMovementDirection,
        ref float remainingMovementLength,
        float3 originalVelocityDirection,
        float hitDistance)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
        FirstPersonCharacterComponent characterComponent = CharacterComponent.ValueRO;

        CharacterAspect.Default_OnMovementHit(
            in this,
            ref context,
            ref baseContext,
            ref characterBody,
            ref characterPosition,
            ref hit,
            ref remainingMovementDirection,
            ref remainingMovementLength,
            originalVelocityDirection,
            hitDistance,
            characterComponent.StepAndSlopeHandling.StepHandling,
            characterComponent.StepAndSlopeHandling.MaxStepHeight,
            characterComponent.StepAndSlopeHandling.CharacterWidthForStepGroundingCheck);
    }

    public void OverrideDynamicHitMasses(
        ref FirstPersonCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref PhysicsMass characterMass,
        ref PhysicsMass otherMass,
        BasicHit hit)
    {
    }

    public void ProjectVelocityOnHits(
        ref FirstPersonCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref float3 velocity,
        ref bool characterIsGrounded,
        ref BasicHit characterGroundHit,
        in DynamicBuffer<KinematicVelocityProjectionHit> velocityProjectionHits,
        float3 originalVelocityDirection)
    {
        FirstPersonCharacterComponent characterComponent = CharacterComponent.ValueRO;
        CharacterAspect.Default_ProjectVelocityOnHits(
            ref velocity,
            ref characterIsGrounded,
            ref characterGroundHit,
            in velocityProjectionHits,
            originalVelocityDirection,
            characterComponent.StepAndSlopeHandling.ConstrainVelocityToGroundPlane);
    }
}