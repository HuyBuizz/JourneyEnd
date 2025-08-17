using UnityEngine;
using Unity.Entities;

public class StoreyAuthoring : MonoBehaviour
{
    class Baker : Unity.Entities.Baker<StoreyAuthoring>
    {
        public override void Bake(StoreyAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Storey>(e);
        }
    }
}
