using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class NetcodePlayerInputAuthoring : MonoBehaviour
{
    public class Baker : Baker<NetcodePlayerInputAuthoring>
    {
        public override void Bake(NetcodePlayerInputAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<NetcodePlayerInput>(entity);
        }
    }
}

public struct NetcodePlayerInput : IInputComponentData
{
    public float2 inputVector;
}
