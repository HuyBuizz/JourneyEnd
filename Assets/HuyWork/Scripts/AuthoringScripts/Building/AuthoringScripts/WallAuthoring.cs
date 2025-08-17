using UnityEngine;
using Unity.Entities;

public struct Wall: IComponentData { }

public class WallAuthoring : MonoBehaviour
{

    class Baker : Unity.Entities.Baker<WallAuthoring>
    {
        public override void Bake(WallAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Wall());
        }
    }
}

