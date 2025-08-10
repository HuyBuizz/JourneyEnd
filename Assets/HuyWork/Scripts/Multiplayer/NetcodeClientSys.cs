using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct NetcodeClientSys : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Entity entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new SimpleRPC { Value = 42 });
            state.EntityManager.AddComponentData(entity, new SendRpcCommandRequest());
            Debug.Log("SimpleRPC sent with value: 42");
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
