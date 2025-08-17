using UnityEngine;
using Unity.Entities;
public class FlamePointFloorSpawnerAuthoring : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject flamePointPrefab;
    [Header("Spawn Settings")]
    public float margin = 1f;
    public float pointDensity = 0.2f;


    class FlamePointFloorSpawnAuthoringBaker : Baker<FlamePointFloorSpawnerAuthoring>
    {
        public override void Bake(FlamePointFloorSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var prefabEntity = GetEntity(authoring.flamePointPrefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlamePointFloorSpawner
            {
                prefab = prefabEntity,
                margin = authoring.margin,
                pointDensity = authoring.pointDensity,
            });
        }
    }
}
public struct FlamePointFloorSpawner : IComponentData
{
    public Entity prefab;
    public float margin;
    public float pointDensity;
}

