using Unity.Entities;
using UnityEngine;

public class ComboPrefabAuthoring : MonoBehaviour
{
    public ComboPrefabSize prefabSize;
    public float totalHuman;
    public float totalVehicle;
    public float totalSupply;
    class Baker : Baker<ComboPrefabAuthoring>
    {
        public override void Bake(ComboPrefabAuthoring a)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new ComboPrefab
            {
                prefabSize = a.prefabSize,
                totalHuman = a.totalHuman,
                totalVehicle = a.totalVehicle,
                totalSupply = a.totalSupply
            });
        }
    }
}

public struct ComboPrefab : IComponentData
{
    public ComboPrefabSize prefabSize;
    public float totalHuman;
    public float totalVehicle;
    public float totalSupply;
}

public enum ComboPrefabSize : byte
{
    Size0,
    Size3x3,
    Size5x5,
    Size7x7
}

