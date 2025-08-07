using UnityEngine;
using Unity.Entities;

/// <summary>
/// Authoring cho hệ thống spawn FlamePoint, cho phép cấu hình prefab và các tham số spawn.
/// </summary>
public class FlamePointSpawnAuthoring : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject flamePointPrefab;
    [Header("Spawn Settings")]
    public float margin = 1f;
    public float pointDensity = 0.2f;
    public float childPointWidth = 2.5f;
    public float childPointHeightMin = 0.0f;
    public float childPointHeightMax = 0.0f;

    class FlamePointSpawnAuthoringBaker : Baker<FlamePointSpawnAuthoring>
    {
        public override void Bake(FlamePointSpawnAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var prefabEntity = GetEntity(authoring.flamePointPrefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlamePointSpawner
            {
                prefab = prefabEntity,
                margin = authoring.margin,
                pointDensity = authoring.pointDensity,
                childPointWidth = authoring.childPointWidth,
                childPointHeightMin = authoring.childPointHeightMin,
                childPointHeightMax = authoring.childPointHeightMax
            });
        }
    }
}

/// <summary>
/// ComponentData lưu thông tin cấu hình spawn FlamePoint.
/// </summary>
public struct FlamePointSpawner : IComponentData
{
    public Entity prefab;
    public float margin;
    public float pointDensity;
    public float childPointWidth;
    public float childPointHeightMin;
    public float childPointHeightMax;
}

