using Unity.Entities;
using UnityEngine;

public class ComboPrefabSingletonAuthoring : MonoBehaviour
{
    [Header("Amount Prefab To Spawn")]
    public float amountPref0 = 0;
    public float amountPref3x3 = 0;
    public float amountPref5x5 = 0;
    public float amountPref7x7 = 0;
    [Header("Prefab Combo")]
    public GameObject prefab0;
    public GameObject prefab3x3;
    public GameObject prefab5x5;
    public GameObject prefab7x7;
}

// Baker sẽ chạy trong editor để bake prefab
public class ComboPrefabSingletonBaker : Baker<ComboPrefabSingletonAuthoring>
{
    public override void Bake(ComboPrefabSingletonAuthoring authoring)
    {
        var e = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(e, new ComboPrefabSingleton
        {
            amountPref0 = Mathf.Max(0, authoring.amountPref0),
            amountPref3x3 = Mathf.Max(0, authoring.amountPref3x3),
            amountPref5x5 = Mathf.Max(0, authoring.amountPref5x5),
            amountPref7x7 = Mathf.Max(0, authoring.amountPref7x7),
            prefab0 = GetEntity(authoring.prefab0, TransformUsageFlags.None),
            prefab3x3 = GetEntity(authoring.prefab3x3, TransformUsageFlags.None),
            prefab5x5 = GetEntity(authoring.prefab5x5, TransformUsageFlags.None),
            prefab7x7 = GetEntity(authoring.prefab7x7, TransformUsageFlags.None)
        });
    }
}

public struct ComboPrefabSingleton : IComponentData
{
    public float amountPref0;
    public float amountPref3x3;
    public float amountPref5x5;
    public float amountPref7x7;
    public Entity prefab0;
    public Entity prefab3x3;
    public Entity prefab5x5;
    public Entity prefab7x7;
}
