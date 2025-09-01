using UnityEngine;
using Unity.Entities;

public struct SpawnZone : IComponentData
{
    public enum SpawnZoneType
    {
        FlamePoint,
        SpawnPoint,
        Other
    }

    public SpawnZoneType spawnZoneType;
}

public class SpawnZoneAuthoring : MonoBehaviour
{
    [SerializeField]
    private SpawnZone.SpawnZoneType spawnZoneType;

    class Baker : Baker<SpawnZoneAuthoring>
    {
        public override void Bake(SpawnZoneAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Kiểm tra có BoxCollider hay không
            var col = authoring.GetComponent<BoxCollider>();
            if (col != null)
            {
                AddComponent(entity, new SpawnZone
                {
                    spawnZoneType = authoring.spawnZoneType
                });
            }
        }
    }
}
