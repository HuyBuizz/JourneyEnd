using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using PhysicsMat = Unity.Physics.Material;


[DisallowMultipleComponent]
public class HybridPlayerPhysicsProxy : MonoBehaviour
{
    private Unity.Entities.World _world;
    [Header("DOTS Capsule (Unity.Physics)")]
    public float radius = 0.5f;
    public float height = 2f;
    public float mass = 70f;
    public Vector3 centerOffset = new Vector3(0f, 1f, 0f);
    public bool kinematicProxy = true;   // true: kinematic, false: dynamic

    [Header("Move Config")]
    public float moveSpeed = 6.5f;
    public float sprintMultiplier = 1.6f;
    public float jumpSpeed = 5.5f;
    public float gravity = 18f;          // dương; hệ di chuyển sẽ trừ dt
    [Range(0, 89)] public float maxSlopeDeg = 55f;
    public float skin = 0.03f;
    public int maxSlideIters = 2;

    [Header("Collision Filter (Unity.Physics)")]
    [Tooltip("Bitmask 32-bit. Ví dụ: 1u<<0 = layer 0")]
    public uint belongsTo = ~0u;   // mặc định: thuộc mọi category
    public uint collidesWith = ~0u;   // mặc định: va với mọi category
    public byte groupIndex = 0;

    [Header("Physics Material")]
    [Range(0, 1)] public float friction = 0.5f;
    [Range(0, 1)] public float restitution = 0.0f;
    public PhysicsMat.CombinePolicy frictionCombine = PhysicsMat.CombinePolicy.ArithmeticMean;
    public PhysicsMat.CombinePolicy restitutionCombine = PhysicsMat.CombinePolicy.Maximum;

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public Color gizmoColor = new Color(0.15f, 0.8f, 1f, 0.85f);
    public bool onlyWhenSelected = true;

    private EntityManager _em;
    public Entity Proxy { get; private set; } = Entity.Null;

    // Giữ blob để chủ động Dispose (tránh rò rỉ)
    private BlobAssetReference<Unity.Physics.Collider> _colliderBlob;

    void Start()
    {
        _world = World.DefaultGameObjectInjectionWorld;
        if (_world == null || !_world.IsCreated)
        {
            Debug.LogWarning("[Proxy] No Default World");
            return;
        }
        _em = _world.EntityManager;

        // Tạo entity
        Proxy = _em.CreateEntity();

        // Đặt tên để dễ debug trong Entities Hierarchy
        _em.SetName(Proxy, "PlayerProxy");

        // Khởi tạo pose
        var pos = (float3)(transform.position + centerOffset);
        var rot = (quaternion)transform.rotation;
        _em.AddComponentData(Proxy, LocalTransform.FromPositionRotationScale(pos, rot, 1f));

        // Tag/State/Config
        _em.AddComponentData(Proxy, new PlayerProxyTag());
        _em.AddComponentData(Proxy, new LastPos { Value = pos });
        _em.AddComponentData(Proxy, new ProxyRuntimeState { Velocity = float3.zero, IsGrounded = false });
        _em.AddComponentData(Proxy, new ProxyMoveConfig
        {
            MoveSpeed = moveSpeed,
            SprintMultiplier = sprintMultiplier,
            JumpSpeed = jumpSpeed,
            Gravity = gravity,
            MaxSlopeDeg = maxSlopeDeg,
            Skin = skin,
            MaxSlideIters = math.max(1, maxSlideIters)
        });

        // Collider + mass
        BuildAndApplyCollider();
        ApplyMassAndDamping();
    }

    void OnDestroy()
    {
        if (_world != null && _world.IsCreated)
        {
            if (Proxy != Entity.Null && _em.Exists(Proxy))
            {
                if (_em.HasComponent<PhysicsCollider>(Proxy))
                    _em.RemoveComponent<PhysicsCollider>(Proxy);
                _em.DestroyEntity(Proxy);
            }
        }
        Proxy = Entity.Null;

        if (_colliderBlob.IsCreated) _colliderBlob.Dispose();
        _colliderBlob = default;
    }

    // Cho phép rebuild khi đổi tham số trong Play Mode
    void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (_world == null || !_world.IsCreated) return;
        if (Proxy == Entity.Null || !_em.Exists(Proxy)) return;

        BuildAndApplyCollider();
        ApplyMassAndDamping();
    }

    // ----------------- Helpers -----------------

    void BuildAndApplyCollider()
    {
        if (_colliderBlob.IsCreated) _colliderBlob.Dispose();

        float halfHeight = math.max(0f, (height * 0.5f) - radius);
        var capsule = new CapsuleGeometry
        {
            Radius = radius,
            Vertex0 = new float3(0, -halfHeight, 0),
            Vertex1 = new float3(0, halfHeight, 0)
        };

        var filter = new CollisionFilter
        {
            BelongsTo = belongsTo,
            CollidesWith = collidesWith,
            GroupIndex = groupIndex
        };

        var material = new PhysicsMat
        {
            Friction = friction,
            Restitution = restitution,
            FrictionCombinePolicy = frictionCombine,
            RestitutionCombinePolicy = restitutionCombine
        };

        _colliderBlob = Unity.Physics.CapsuleCollider.Create(capsule, filter, material);

        var pc = new PhysicsCollider { Value = _colliderBlob };
        if (_em.HasComponent<PhysicsCollider>(Proxy)) _em.SetComponentData(Proxy, pc);
        else _em.AddComponentData(Proxy, pc);
    }

    void ApplyMassAndDamping()
    {
        if (!_em.Exists(Proxy)) return;

        var massProps = _colliderBlob.IsCreated ? _colliderBlob.Value.MassProperties : MassProperties.UnitSphere;

        if (_em.HasComponent<PhysicsMass>(Proxy))
            _em.RemoveComponent<PhysicsMass>(Proxy);
        if (_em.HasComponent<PhysicsGravityFactor>(Proxy))
            _em.RemoveComponent<PhysicsGravityFactor>(Proxy);

        if (kinematicProxy)
        {
            _em.AddComponentData(Proxy, PhysicsMass.CreateKinematic(massProps));
        }
        else
        {
            _em.AddComponentData(Proxy, PhysicsMass.CreateDynamic(massProps, mass));
            _em.AddComponentData(Proxy, new PhysicsGravityFactor { Value = 1f });
        }

        if (!_em.HasComponent<PhysicsVelocity>(Proxy))
            _em.AddComponentData(Proxy, new PhysicsVelocity());
        if (!_em.HasComponent<PhysicsDamping>(Proxy))
            _em.AddComponentData(Proxy, new PhysicsDamping { Linear = 0.1f, Angular = 0.1f });
    }

    // --- Gizmo ---
    void OnDrawGizmos()
    {
        if (!drawGizmos || onlyWhenSelected) return;
        DrawCapsuleGizmo();
    }
    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || !onlyWhenSelected) return;
        DrawCapsuleGizmo();
    }
    void DrawCapsuleGizmo()
    {
        Gizmos.color = gizmoColor;
        float hh = Mathf.Max(0f, (height * 0.5f) - radius);
        var center = transform.position + centerOffset;
        var up = transform.up; var right = transform.right; var fwd = transform.forward;

        var p0 = center - up * hh;
        var p1 = center + up * hh;
        Gizmos.DrawWireSphere(p0, radius);
        Gizmos.DrawWireSphere(p1, radius);
        Gizmos.DrawLine(p0 + right * radius, p1 + right * radius);
        Gizmos.DrawLine(p0 - right * radius, p1 - right * radius);
        Gizmos.DrawLine(p0 + fwd * radius, p1 + fwd * radius);
        Gizmos.DrawLine(p0 - fwd * radius, p1 - fwd * radius);
        Gizmos.DrawRay(center, fwd * (radius * 2f));
    }
}
