using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DefaultExecutionOrder(-10)]
public class ECSPlayerInputBridge : MonoBehaviour
{
#if ENABLE_INPUT_SYSTEM
    public PlayerInput playerInput;
#endif
    public Transform cameraYaw; // pivot chỉ yaw

    EntityManager _em; Entity _singleton;

    void Awake()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        _em = world.EntityManager;
        var q = _em.CreateEntityQuery(typeof(PlayerInputState));
        _singleton = q.CalculateEntityCount() == 0 ? _em.CreateEntity(typeof(PlayerInputState))
                                                   : q.GetSingletonEntity();
        _em.SetComponentData(_singleton, new PlayerInputState());
    }

    void Update()
    {
        if (!_em.Exists(_singleton)) return;

        Vector2 move = Vector2.zero; bool sprint = false; bool jump = false;
#if ENABLE_INPUT_SYSTEM
        move = playerInput.actions["Move"]?.ReadValue<Vector2>() ?? Vector2.zero;
        sprint = playerInput.actions["Sprint"]?.IsPressed() ?? false;
        jump = playerInput.actions["Jump"]?.WasPressedThisFrame() ?? false;
#endif
        Vector3 fwd = cameraYaw ? Vector3.ProjectOnPlane(cameraYaw.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 right = cameraYaw ? Vector3.ProjectOnPlane(cameraYaw.right, Vector3.up).normalized : Vector3.right;
        Vector3 worldMove = (right * move.x + fwd * move.y);
        if (worldMove.sqrMagnitude > 1e-6f) worldMove.Normalize();

        var s = _em.GetComponentData<PlayerInputState>(_singleton);
        s.Move = new float2(move.x, move.y);
        s.MoveDirWorld = (float3)worldMove;
        s.Sprint = sprint;
        s.Jump = jump;
        _em.SetComponentData(_singleton, s);
    }
}
