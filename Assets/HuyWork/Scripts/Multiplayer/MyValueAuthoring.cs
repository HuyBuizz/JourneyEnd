using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

public class MyValueAuthoring : MonoBehaviour
{
    public class Baker : Baker<MyValueAuthoring>
    {
        public override void Bake(MyValueAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MyValue());

        }
    }


}
public struct MyValue : IComponentData
{
    [GhostField] public int value;
}
