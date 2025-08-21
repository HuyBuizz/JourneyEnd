using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerChecker : MonoBehaviour
{
    [Header("Detection Area (GO)")]
    [SerializeField] private Transform rangeMesh;          
    [SerializeField] private MeshCollider meshCollider;    
    [Header("Damage")]
    public float dps = 0f;

    private PlayerInput _playerInput;
    private InputAction _leftHoldAction;

    private EntityManager _em;
    private EntityQuery _flameQuery;

    private static readonly float INSIDE_EPS = 1e-8f; 

    private void Awake()
    {
        var playerGO = GameObject.Find("PlayerCapsule");
        if (playerGO != null)
        {
            _playerInput = playerGO.GetComponent<PlayerInput>();
            if (_playerInput != null)
                _leftHoldAction = _playerInput.actions["LeftMouseHold"];
        }

        if (rangeMesh == null) rangeMesh = transform.Find("Range");
        if (meshCollider == null && rangeMesh != null)
            meshCollider = rangeMesh.GetComponent<MeshCollider>();

        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
        {
            _em = world.EntityManager;
            _flameQuery = _em.CreateEntityQuery(
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadWrite<FlamePoint>() 
            );
        }
    }

    private void Update()
    {
        if (meshCollider == null || _em == default)
            return;
        if (_leftHoldAction == null || !_leftHoldAction.IsPressed())
            return;

        Bounds b = meshCollider.bounds;

        using var entities = _flameQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        for (int i = 0; i < entities.Length; i++)
        {
            var e = entities[i];
            var lt = _em.GetComponentData<LocalTransform>(e);
            float3 pos = lt.Position;

            if (!b.Contains((Vector3)pos))
                continue;

            Vector3 closest = meshCollider.ClosestPoint(pos);
            if (((Vector3)pos - closest).sqrMagnitude <= INSIDE_EPS)
            {
                var fp = _em.GetComponentData<FlamePoint>(e);
                if (fp.currentHealth <= 0f)
                    continue;
                fp.currentHealth = math.max(0f, fp.currentHealth - dps * Time.deltaTime);
                _em.SetComponentData(e, fp);
            }
        }
    }
}
