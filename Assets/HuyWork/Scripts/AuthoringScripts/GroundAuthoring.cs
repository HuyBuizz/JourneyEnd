using Unity.Entities;
using UnityEngine;

public class GroundAuthoring : MonoBehaviour
{
        class Baker : Baker<GroundAuthoring>
    {
        public override void Bake(GroundAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Ground());
        }
    }
}
public struct Ground : IComponentData { }
