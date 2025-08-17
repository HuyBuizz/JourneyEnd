using UnityEngine;
using Unity.Entities;

public struct WallContainer : IComponentData { }

public class WallContainerAuthoring : MonoBehaviour
{
    class Baker : Unity.Entities.Baker<WallContainerAuthoring>
    {
        public override void Bake(WallContainerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<WallContainer>(e);
        }
    }
}
