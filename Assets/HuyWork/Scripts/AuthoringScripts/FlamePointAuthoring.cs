using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// ComponentData lưu dữ liệu FlamePoint.
/// </summary>
public struct FlamePoint : IComponentData
{
    public float detectRadius;
    public float maxHealth;
    public float currentHealth;
    public float dps;
    public Entity model;
    public Entity effect;
}

/// <summary>
/// Authoring chuyển FlamePoint MonoBehaviour thành entity DOTS.
/// </summary>
public class FlamePointAuthoring : MonoBehaviour
{
    public float detectRadius = 2.5f;
    public float maxHealth = 100f;
    public float currentHealth = 0f;
    public float dps = 10f;
    public GameObject model;
    public GameObject effect;

    class Baker : Baker<FlamePointAuthoring>
    {
        public override void Bake(FlamePointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlamePoint
            {
                detectRadius = authoring.detectRadius,
                maxHealth = authoring.maxHealth,
                currentHealth = authoring.currentHealth,
                dps = authoring.dps,
                model = authoring.model != null ? GetEntity(authoring.model, TransformUsageFlags.Dynamic) : Entity.Null,
                effect = authoring.effect != null ? GetEntity(authoring.effect, TransformUsageFlags.Dynamic) : Entity.Null
            });
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}

