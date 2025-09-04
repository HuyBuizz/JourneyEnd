using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class ClimbableAuthoring : MonoBehaviour
{
    public Vector3 TargetPosition;

    class Baker : Baker<ClimbableAuthoring>
    {
        public override void Bake(ClimbableAuthoring a)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new Climbable {});
        }
    }
}

public struct Climbable : IComponentData{}

