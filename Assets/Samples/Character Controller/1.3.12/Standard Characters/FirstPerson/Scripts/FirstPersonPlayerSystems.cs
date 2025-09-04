using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Unity.CharacterController;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
public partial class FirstPersonPlayerInputsSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<FixedTickSystem.Singleton>();
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerInputs>().Build());

        // Lock the cursor initially if cursorLocked is true
        foreach (var player in SystemAPI.Query<FirstPersonCharacterControl>())
        {
            if (player.CursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    protected override void OnUpdate()
    {
        uint tick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

#if ENABLE_INPUT_SYSTEM


        foreach (var (playerInputs, player) in SystemAPI.Query<RefRW<FirstPersonPlayerInputs>, FirstPersonPlayer>())
        {
            if (!SystemAPI.HasComponent<FirstPersonCharacterControl>(player.ControlledCharacter))
                continue;
            var characterControl = SystemAPI.GetComponent<FirstPersonCharacterControl>(player.ControlledCharacter);

            float2 moveInput = float2.zero;

            var mappingBuffer = SystemAPI.GetSingletonBuffer<KeyActionMappingData>();

            foreach (var keyAction in mappingBuffer)
            {
                if (Keyboard.current[keyAction.KeyCode].isPressed)
                {
                    switch (keyAction.Action.ToString().ToLower())
                    {
                        case "move_forward":
                            moveInput.y += 1f;
                            break;
                        case "move_backward":
                            moveInput.y -= 1f;
                            break;
                        case "move_right":
                            moveInput.x += 1f;
                            break;
                        case "move_left":
                            moveInput.x -= 1f;
                            break;
                        case "jump":
                            playerInputs.ValueRW.JumpPressed.Set(tick);
                            break;
                        case "interact":
                            playerInputs.ValueRW.InteractPressed.Set(tick);
                            break;
                        case "crawl":
                            playerInputs.ValueRW.CrawlPressed.Set(tick);
                            break;
                    }
                }
            }

            playerInputs.ValueRW.MoveInput = moveInput;

            // Only process camera look input if cursorLocked is true
            if (characterControl.CursorLocked)
            {
                playerInputs.ValueRW.LookInput = Mouse.current.delta.ReadValue() * player.LookInputSensitivity;
            }

            // ESC: hiện/ẩn cursor
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                characterControl.CursorLocked = false; // unlock để hiện
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                playerInputs.ValueRW.LookInput = float2.zero; // ngừng xoay camera
            }

            // Click chuột trái: lock lại
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                characterControl.CursorLocked = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
        }
#endif
    }
}

/// <summary>
/// Apply inputs that need to be read at a variable rate
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct FirstPersonPlayerVariableStepControlSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerInputs>().Build());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInputs, player) in SystemAPI.Query<FirstPersonPlayerInputs, FirstPersonPlayer>().WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<FirstPersonCharacterControl>(player.ControlledCharacter))
            {
                FirstPersonCharacterControl characterControl = SystemAPI.GetComponent<FirstPersonCharacterControl>(player.ControlledCharacter);

                characterControl.LookDegreesDelta = playerInputs.LookInput;

                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
            }
        }
    }
}

/// <summary>
/// Apply inputs that need to be read at a fixed rate.
/// It is necessary to handle this as part of the fixed step group, in case your framerate is lower than the fixed step rate.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
[BurstCompile]
public partial struct FirstPersonPlayerFixedStepControlSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FixedTickSystem.Singleton>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerInputs>().Build());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        uint tick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

        foreach (var (playerInputs, player) in SystemAPI.Query<FirstPersonPlayerInputs, FirstPersonPlayer>().WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<FirstPersonCharacterControl>(player.ControlledCharacter))
            {
                FirstPersonCharacterControl characterControl = SystemAPI.GetComponent<FirstPersonCharacterControl>(player.ControlledCharacter);

                quaternion characterRotation = SystemAPI.GetComponent<LocalTransform>(player.ControlledCharacter).Rotation;

                // Synce MoveInput
                characterControl.MoveInput = playerInputs.MoveInput;

                // Move
                float3 characterForward = MathUtilities.GetForwardFromRotation(characterRotation);
                float3 characterRight = MathUtilities.GetRightFromRotation(characterRotation);
                characterControl.MoveVector = (playerInputs.MoveInput.y * characterForward) + (playerInputs.MoveInput.x * characterRight);
                characterControl.MoveVector = MathUtilities.ClampToMaxLength(characterControl.MoveVector, 1f);

                // Jump
                characterControl.Jump = playerInputs.JumpPressed.IsSet(tick);

                // Interact
                characterControl.Interact = playerInputs.InteractPressed.IsSet(tick);

                // Crawl
                characterControl.Crawl = playerInputs.CrawlPressed.IsSet(tick);

                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
            }
        }
    }
}