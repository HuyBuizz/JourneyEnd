using Unity.Entities.UniversalDelegates;
using UnityEngine;
using Unity.Entities;

public class FlameEffectAuthoring : MonoBehaviour
{
    class FlameEffectAuthoringBaker : Baker<FlameEffectAuthoring>
    {
        public override void Bake(FlameEffectAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlameEffect());
        }
    }
}

public struct FlameEffect : IComponentData { }



