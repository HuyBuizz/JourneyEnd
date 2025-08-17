using UnityEngine;
using Unity.Entities;

public struct OtherContainer : IComponentData { }

public class OtherContainerAuthoring : MonoBehaviour
{
    class Baker : Unity.Entities.Baker<OtherContainerAuthoring>
    {
        public override void Bake(OtherContainerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<OtherContainer>(e);
        }
    }
}
