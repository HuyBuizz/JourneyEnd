using Unity.Entities;
using UnityEngine;

public class SpawnPrefabAuthoring : MonoBehaviour
{
    public GameObject prefab0;
    public GameObject prefab3x3;
    public GameObject prefab5x5;
}

// Baker sẽ chạy trong editor để bake prefab
public class SpawnPrefabBaker : Baker<SpawnPrefabAuthoring>
{
    public override void Bake(SpawnPrefabAuthoring authoring)
    {
        var e = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(e, new SpawnPrefabSingleton
        {
            prefab0 = GetEntity(authoring.prefab0, TransformUsageFlags.None),
            prefab3x3 = GetEntity(authoring.prefab3x3, TransformUsageFlags.None),
            prefab5x5 = GetEntity(authoring.prefab5x5, TransformUsageFlags.None)
        });
    }
}

public struct SpawnPrefabSingleton : IComponentData
{
    public Entity prefab0;
    public Entity prefab3x3;
    public Entity prefab5x5;
}
