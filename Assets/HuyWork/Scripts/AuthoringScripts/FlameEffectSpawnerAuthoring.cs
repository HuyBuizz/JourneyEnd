using UnityEngine;
using Unity.Entities;

public class FlameEffectSpawnerAuthoring : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject effectPrefab;



    class FlameEffectSpawnerAuthoringBaker : Baker<FlameEffectSpawnerAuthoring>
    {
        public override void Bake(FlameEffectSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var prefabEntity = GetEntity(authoring.effectPrefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlameEffectSpawner
            {
                prefab = prefabEntity,
            });
        }
    }
}


public struct FlameEffectSpawner : IComponentData
{
    public Entity prefab;
}

