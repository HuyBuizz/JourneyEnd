using UnityEngine;
using Unity.Entities;
public class SpawnerAuthoring : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject SpawnPointPrefab;
    public enum SpawnerType
    {
        FlamePoint,
        Car,
        Human,
        Other
    }

    public SpawnerType spawnerType;
    [Header("Spawn Settings")]
    public float margin = 1f;
    public float pointDensity = 0.2f;
    public float chisophuctap1 = 0f;
    public float chisophuctap2 = 0f;
    public float chisophuctap3 = 0f;
    

    class FlamePointFloorSpawnAuthoringBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var prefabEntity = GetEntity(authoring.SpawnPointPrefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpawnPointSpawner
            {
                prefab = prefabEntity,
                spawnerType = authoring.spawnerType,
                margin = authoring.margin,
                pointDensity = authoring.pointDensity,
            });
        }
    }
}
public struct SpawnPointSpawner : IComponentData
{
    public Entity prefab;
    public SpawnerAuthoring.SpawnerType spawnerType;
    public float margin;
    public float pointDensity;
}

