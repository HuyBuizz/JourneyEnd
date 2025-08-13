using UnityEngine;
using Unity.Entities;

public class HouseAuthoring : MonoBehaviour
{
    class HouseAuthoringBaker : Baker<HouseAuthoring>
    {
        public override void Bake(HouseAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new House());
        }
    }
}
public struct House : IComponentData { }
public struct Floor : IComponentData { }
public struct Part : IComponentData { }