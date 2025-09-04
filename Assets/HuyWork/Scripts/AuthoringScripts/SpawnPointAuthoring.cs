using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SpawnPoint : IComponentData { }
public struct SpawnPointOccupied : IComponentData, IEnableableComponent { }
public struct SpawnPointSettings : IComponentData
{
    public int maxNeighbors;
}

public class SpawnPointAuthoring : MonoBehaviour
{
    public bool isOcupied = false;
    [Header("Neighbors")]
    [Range(1, 32)] public int maxNeighbors = 8;

    class Baker : Baker<SpawnPointAuthoring>
    {
        public override void Bake(SpawnPointAuthoring a)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new SpawnPoint { });
            AddComponent(e, new SpawnPointSettings
            {
                maxNeighbors = math.clamp(a.maxNeighbors, 1, 32)
            });
            AddBuffer<Neighbor>(e);
            AddComponent<SpawnPointOccupied>(e);
            SetComponentEnabled<SpawnPointOccupied>(e, a.isOcupied);
        }
    }
}

