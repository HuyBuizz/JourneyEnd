using UnityEngine;
using Unity.Entities;

public struct FlamePointPlatform : IComponentData
{
    public enum PlatformType
    {
        Wood,
        Stone,
        Metal
    }

    public PlatformType platformType;
}

public class PlatformAuthoring : MonoBehaviour
{
    [SerializeField]
    private FlamePointPlatform.PlatformType platformType;

    class Baker : Baker<PlatformAuthoring>
    {
        public override void Bake(PlatformAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Kiểm tra có BoxCollider hay không
            var col = authoring.GetComponent<BoxCollider>();
            if (col != null)
            {
                AddComponent(entity, new FlamePointPlatform
                {
                    platformType = authoring.platformType
                });
            }
        }
    }
}
