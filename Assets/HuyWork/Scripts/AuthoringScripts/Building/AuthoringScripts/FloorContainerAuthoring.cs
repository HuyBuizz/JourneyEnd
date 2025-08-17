using UnityEngine;
using Unity.Entities;

public struct FloorContainer : IComponentData { }

public class FloorContainerAuthoring : MonoBehaviour
{
    class Baker : Unity.Entities.Baker<FloorContainerAuthoring>
    {
        public override void Bake(FloorContainerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<FloorContainer>(e);
        }
    }
}
