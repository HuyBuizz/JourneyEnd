using UnityEngine;
using Unity.Entities;

public class FlameEffectSingletonAuthoring : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject effectPrefab;

    class FlameEffectSingletonAuthoringBaker : Baker<FlameEffectSingletonAuthoring>
    {
        public override void Bake(FlameEffectSingletonAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var prefabEntity = GetEntity(authoring.effectPrefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlameEffectSingleton
            {
                prefab = prefabEntity,
            });
        }
    }
}

public struct FlameEffectSingleton : IComponentData
{
    public Entity prefab;
}

