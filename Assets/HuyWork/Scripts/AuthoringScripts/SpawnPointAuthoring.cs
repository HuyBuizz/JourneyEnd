using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnPointAuthoring : MonoBehaviour
{
    [Header("FlamePoint")]
    public float detectRadius = 2.5f;
    public bool isOcupied;
    [Header("Neighbors")]
    [Range(1, 32)] public int maxNeighbors = 8;

    class Baker : Baker<SpawnPointAuthoring>
    {
        public override void Bake(SpawnPointAuthoring a)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new SpawnPoint
            {
                detectRadius = math.max(0f, a.detectRadius),
                isOcupied = a.isOcupied
            });
            AddComponent(e, new SpawnPointSettings
            {
                maxNeighbors = math.clamp(a.maxNeighbors, 1, 32)
            });
            AddBuffer<Neighbor>(e);
        }
    }
}

public struct SpawnPoint : IComponentData
{
    public float detectRadius;
    public bool isOcupied;
}
public struct SpawnPointSettings : IComponentData
{
    public int maxNeighbors;
}