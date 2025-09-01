using Unity.Entities;
using UnityEngine;

public class BallAuthoring : MonoBehaviour
{
    class Baker : Baker<BallAuthoring>
    {
        public override void Bake(BallAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Ball());
        }
    }
}
public struct Ball : IComponentData { }