using UnityEngine;
using Unity.Entities;

public class BuildingAuthoring : MonoBehaviour
{
    class BuildingAuthoringBaker : Baker<BuildingAuthoring>
    {
        public override void Bake(BuildingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Building());
        }
    }
}
public struct Building : IComponentData { }
public struct Storey : IComponentData { }