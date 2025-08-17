using UnityEngine;
using Unity.Entities;

public struct BuildingObjectContainer : IComponentData { }

public class BuildingObjectContainerAuthoring : MonoBehaviour
{
    class Baker : Unity.Entities.Baker<BuildingObjectContainerAuthoring>
    {
        public override void Bake(BuildingObjectContainerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<BuildingObjectContainer>(e);
        }
    }
}
