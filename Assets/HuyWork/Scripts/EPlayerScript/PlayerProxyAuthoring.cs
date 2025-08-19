using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

[DisallowMultipleComponent]
public class PlayerProxyAuthoring : MonoBehaviour
{
    [Header("Capsule (Unity.Physics)")]
    public float radius = 0.5f;
    public float height = 2f;
    public float mass   = 70f;
    public bool  kinematic = true;
    public Vector3 startOffset = new Vector3(0,1,0);

    [Header("Move Config")]
    public float moveSpeed = 6.5f;
    public float sprintMultiplier = 1.6f;
    public float jumpSpeed = 5.5f;
    public float gravity = 18f;
    [Range(0,89)] public float maxSlopeDeg = 55f;
    public float skin = 0.03f;
    public int   maxSlideIters = 2;

    void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated) { Debug.LogWarning("No default world"); return; }

        var em = world.EntityManager;
        var e  = em.CreateEntity();

        var pos = (float3)(transform.position + startOffset);
        var rot = (quaternion)transform.rotation;

        em.AddComponentData(e, LocalTransform.FromPositionRotationScale(pos, rot, 1f));
        em.AddComponentData(e, new PlayerProxyTag());
        em.AddComponentData(e, new LastPos { Value = pos });
        em.AddComponentData(e, new ProxyRuntimeState { Velocity = float3.zero, IsGrounded = false });

        // collider capsule
        float hh = math.max(0f, (height * 0.5f) - radius);
        var cap = new CapsuleGeometry { Radius = radius, Vertex0 = new float3(0,-hh,0), Vertex1 = new float3(0,hh,0) };
        var blob = Unity.Physics.CapsuleCollider.Create(cap, CollisionFilter.Default);
        em.AddComponentData(e, new PhysicsCollider { Value = blob });

        var mp = blob.Value.MassProperties;
        if (kinematic)
            em.AddComponentData(e, PhysicsMass.CreateKinematic(mp));
        else
        {
            em.AddComponentData(e, PhysicsMass.CreateDynamic(mp, mass));
            em.AddComponentData(e, new PhysicsGravityFactor { Value = 1f });
        }
        em.AddComponentData(e, new PhysicsVelocity());
        em.AddComponentData(e, new PhysicsDamping { Linear = 0.1f, Angular = 0.1f });

        em.AddComponentData(e, new ProxyMoveConfig {
            MoveSpeed = moveSpeed, SprintMultiplier = sprintMultiplier, JumpSpeed = jumpSpeed,
            Gravity = gravity, MaxSlopeDeg = maxSlopeDeg, Skin = skin, MaxSlideIters = math.max(1, maxSlideIters)
        });
    }
}
