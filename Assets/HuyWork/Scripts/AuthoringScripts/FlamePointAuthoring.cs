using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct FlamePoint : IComponentData
{
    public float detectRadius;
    public float maxHealth;
    public float currentHealth;
    public float dps;
    public bool onFire;
}

// Enableable tag để bật/tắt trạng thái cháy mà không cần Add/Remove
public struct Burning : IComponentData, IEnableableComponent {}

// Buffer lưu nhiều láng giềng
[InternalBufferCapacity(8)]
public struct Neighbor : IBufferElementData
{
    public Entity Entity;
    public float  DistanceSq; // bình phương khoảng cách để so sánh nhanh
}

// Cấu hình số láng giềng tối đa (K)
public struct FlameNeighborSettings : IComponentData
{
    public int maxNeighbors; // 1..32
}

public class FlamePointAuthoring : MonoBehaviour
{
    [Header("FlamePoint")]
    public float detectRadius = 2.5f;
    public float maxHealth    = 100f;
    public float currentHealth= 0f;
    public float dps          = 10f;
    public bool  onFire       = false;

    [Header("Neighbors")]
    [Range(1, 32)] public int maxNeighbors = 8;

    class Baker : Baker<FlamePointAuthoring>
    {
        public override void Bake(FlamePointAuthoring a)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(e, new FlamePoint
            {
                detectRadius  = math.max(0f, a.detectRadius),
                maxHealth     = math.max(0.0001f, a.maxHealth),
                currentHealth = math.clamp(a.currentHealth, 0f, math.max(0.0001f, a.maxHealth)),
                dps           = math.max(0f, a.dps),
                onFire        = a.onFire
            });

            AddComponent(e, new FlameNeighborSettings { maxNeighbors = math.clamp(a.maxNeighbors, 1, 32) });

            // Tạo buffer Neighbor rỗng để runtime lấp đầy
            AddBuffer<Neighbor>(e);

            // Thêm Burning dưới dạng enableable và set trạng thái ban đầu theo onFire
            AddComponent<Burning>(e);
            SetComponentEnabled<Burning>(e, a.onFire); // ❗ không dùng state.EntityManager trong Baker
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
