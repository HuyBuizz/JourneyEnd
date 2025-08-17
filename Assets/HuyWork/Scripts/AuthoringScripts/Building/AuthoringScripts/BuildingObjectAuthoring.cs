using UnityEngine;
using Unity.Entities;

public struct BuildingObject : IComponentData { }
public class BuildingObjectAuthoring : MonoBehaviour
{
    class Baker : Baker<BuildingObjectAuthoring>
    {
        public override void Bake(BuildingObjectAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingObject());
        }
    }
}