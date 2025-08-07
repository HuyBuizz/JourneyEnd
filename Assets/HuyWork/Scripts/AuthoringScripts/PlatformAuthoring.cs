using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// ComponentData cho platform để spawn FlamePoint.
/// </summary>
public struct FlamePointPlatform : IComponentData
{
    public float3 size;
    public float3 center;
}

/// <summary>
/// Authoring cho từng platform để chuyển thành entity DOTS có FlamePointPlatform.
/// </summary>
public class PlatformAuthoring : MonoBehaviour
{
    class Baker : Baker<PlatformAuthoring>
    {
        public override void Bake(PlatformAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var col = authoring.GetComponent<BoxCollider>();
            if (col != null)
            {
                AddComponent(entity, new FlamePointPlatform
                {
                    size = Vector3.Scale(col.size, authoring.transform.localScale),
                    center = authoring.transform.position
                });
            }
        }
    }
}