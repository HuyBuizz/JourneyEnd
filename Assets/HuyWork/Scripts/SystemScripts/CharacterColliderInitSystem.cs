using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))] 
public partial struct CharacterColliderInitSystem : ISystem
{
    private EntityQuery _needVariantsQ;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _needVariantsQ = SystemAPI.QueryBuilder()
            .WithAll<PhysicsCollider>()
            .WithNone<CharacterColliderVariants>()
            .Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (_needVariantsQ.IsEmptyIgnoreFilter) return;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                           .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (pc, e) in SystemAPI.Query<RefRO<PhysicsCollider>>()
                                          .WithNone<CharacterColliderVariants>()
                                          .WithEntityAccess())
        {
            var standing = pc.ValueRO.Value; // blob gốc (shared) — KHÔNG dispose
            var crawling = CreateCrawlingFromStanding(pc.ValueRO, 0.5f); // cao còn 50%

            ecb.AddComponent(e, new CharacterColliderVariants
            {
                Standing    = standing,
                Crawling    = crawling,
                HasCrawling = (byte)(crawling.IsCreated ? 1 : 0)
            });
        }
    }

    private static unsafe BlobAssetReference<Collider> CreateCrawlingFromStanding(
        PhysicsCollider src, float k)
    {
        if (!src.Value.IsCreated) return default;
        var cap = (CapsuleCollider*)src.ColliderPtr;
        if (cap == null || cap->Type != ColliderType.Capsule) return default;

        var filter   = cap->GetCollisionFilter();
        var material = cap->Material;
        var old      = cap->Geometry;

        float3 c  = 0.5f * (old.Vertex0 + old.Vertex1);
        float3 ax = old.Vertex1 - old.Vertex0;
        float len = math.length(ax);
        if (len <= 1e-5f) return default;

        float R  = old.Radius;
        float H  = len + 2f * R;
        float Hn = math.max(k * H, 2f * R + 1e-4f);
        float ln = Hn - 2f * R;

        float3 u   = ax / len;
        float half = 0.5f * ln;

        var geom = new CapsuleGeometry
        {
            Vertex0 = c - u * half,
            Vertex1 = c + u * half,
            Radius  = R
        };
        return CapsuleCollider.Create(geom, filter, material);
    }
}

[BurstCompile]
public partial struct CharacterColliderCleanupSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (v, e) in SystemAPI.Query<RefRO<CharacterColliderVariants>>()
                                        .WithNone<PhysicsCollider>()
                                        .WithEntityAccess())
        {
            if (v.ValueRO.Crawling.IsCreated)
                v.ValueRO.Crawling.Dispose(); 
        }
    }
}