using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.CharacterController;

[Serializable]
public struct FirstPersonCharacterComponent : IComponentData
{
    public float GroundMaxSpeed;
    public float GroundedMovementSharpness;
    public float AirAcceleration;
    public float AirMaxSpeed;
    public float AirDrag;
    public float JumpSpeed;
    public float3 Gravity;
    public bool PreventAirAccelerationAgainstUngroundedHits;
    public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;

    public float MinViewAngle;
    public float MaxViewAngle;

    public Entity ViewEntity;
    public float ViewPitchDegrees;
    public quaternion ViewLocalRotation;

    public float ClimbSpeed;
    public float CrawlSpeed;
}

[Serializable]
public struct FirstPersonCharacterControl : IComponentData
{
    public float3 MoveVector;
    public float2 MoveInput;
    public float2 LookDegreesDelta;
    public bool Jump;
    public bool CursorLocked;
    public bool Interact;
    public bool Crawl;
}

[Serializable]
public struct FirstPersonCharacterView : IComponentData
{
    public Entity CharacterEntity;
}

[Serializable]
public struct FirstPersonCharacterState : IComponentData
{
    public bool IsClimbing;
    public bool IsCrawling;
    public float ClimableObjectHeight;
    public bool CrawlColliderShrunk;
}

public struct CharacterColliderVariants : ICleanupComponentData
{
    public BlobAssetReference<Unity.Physics.Collider> Standing; // asset gốc (KHÔNG dispose)
    public BlobAssetReference<Unity.Physics.Collider> Crawling; // do ta tạo (PHẢI dispose)
    public byte HasCrawling;
}

