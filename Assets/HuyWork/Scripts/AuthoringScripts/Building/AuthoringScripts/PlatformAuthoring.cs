using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct FlamePointPlatform : IComponentData { }

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
                AddComponent(entity, new FlamePointPlatform());
            }
        }
    }
}