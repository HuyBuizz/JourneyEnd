using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// ComponentData lưu dữ liệu FlamePoint.
/// </summary>
public struct FlamePoints : IComponentData
{
    public float detectRadius;
    public float maxHealth;
    public float currentHealth;
    public float dps;
    public Entity pointContainer;
    public Entity model;
    public Entity effect;
}

/// <summary>
/// Authoring chuyển FlamePoint MonoBehaviour thành entity DOTS.
/// </summary>
// public class FlamePointAuthoring : MonoBehaviour
// {
//     public float detectRadius = 1f;
//     public float maxHealth = 100f;
//     public float currentHealth = 100f;
//     public float dps = 5f;
//     public GameObject pointContainer;
//     public GameObject model;
//     public GameObject effect;

//     class Baker : Baker<FlamePointAuthoring>
//     {
//         public override void Bake(FlamePointAuthoring authoring)
//         {
//             var entity = GetEntity(TransformUsageFlags.Dynamic);
//             AddComponent(entity, new FlamePoints
//             {
//                 detectRadius = authoring.detectRadius,
//                 maxHealth = authoring.maxHealth,
//                 currentHealth = authoring.currentHealth,
//                 dps = authoring.dps,
//                 pointContainer = authoring.pointContainer != null ? GetEntity(authoring.pointContainer, TransformUsageFlags.Dynamic) : Entity.Null,
//                 model = authoring.model != null ? GetEntity(authoring.model, TransformUsageFlags.Dynamic) : Entity.Null,
//                 effect = authoring.effect != null ? GetEntity(authoring.effect, TransformUsageFlags.Dynamic) : Entity.Null
//             });
//         }
//     }
// }

