using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct NetcodeServerSys : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    // KHÔNG nên BurstCompile nếu dùng Debug.Log
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((
            RefRO<SimpleRPC> simpleRpc,
            RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<SimpleRPC>,
                RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
        {
            Debug.Log("Received Rpc: " + simpleRpc.ValueRO.Value + " :: " + receiveRpcCommandRequest.ValueRO.SourceConnection);
            ecb.DestroyEntity(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}
