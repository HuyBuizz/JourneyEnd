using Unity.Entities;
using UnityEngine;


public class SpawnerAuthoring : MonoBehaviour
{

    public GameObject prefab;


    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var prefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Spawner { prefab = prefabEntity });
        }
    }
}


public struct Spawner : IComponentData
{
    public Entity prefab;
}