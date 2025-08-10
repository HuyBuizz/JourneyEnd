using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct TestMyValueSys : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRO<MyValue> myValue,
            Entity entity)
            in SystemAPI.Query<
                RefRO<MyValue>>().WithEntityAccess())
        {
            Debug.Log(myValue.ValueRO.value + " :: " + entity + " :: " + state.World);
        }
    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct TextMyValueServerSys : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<MyValue> myValue in SystemAPI.Query<RefRW<MyValue>>())
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                myValue.ValueRW.value = UnityEngine.Random.Range(100, 999);
                Debug.Log("MyValue changed to: " + myValue.ValueRW.value);
            }
        }

    }
}