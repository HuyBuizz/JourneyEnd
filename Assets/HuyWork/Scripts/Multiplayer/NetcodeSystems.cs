// NetcodeSystems.cs
// Merged systems: NetcodeClientSys, NetcodeServerSys, NetcodePlayerInputSystem, NetcodePlayerMovementSys, TestMyValueSys, SimpleRPC

using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Multiplayer.Authoring;

namespace Multiplayer.NetcodeSystems
{
	// NetcodeClientSys
	[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
	partial struct NetcodeClientSys : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state) { }

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
		public void OnDestroy(ref SystemState state) { }
	}

	// NetcodeServerSys
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

	// NetcodePlayerInputSystem
	[UpdateInGroup(typeof(GhostInputSystemGroup))]
	partial struct NetcodePlayerInputSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<NetcodePlayerInput>();
			state.RequireForUpdate<NetworkStreamInGame>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			foreach ((RefRW<NetcodePlayerInput> netcodePlayerInput, RefRW<MyValue> myValue) in SystemAPI.Query<RefRW<NetcodePlayerInput>, RefRW<MyValue>>().WithAll<GhostOwnerIsLocal>())
			{
				float2 inputVector = new float2();

				if (Input.GetKey(KeyCode.W)) inputVector.y = +1f;
				if (Input.GetKey(KeyCode.A)) inputVector.x = -1f;
				if (Input.GetKey(KeyCode.S)) inputVector.y = -1f;
				if (Input.GetKey(KeyCode.D)) inputVector.x = +1f;
				netcodePlayerInput.ValueRW.inputVector = inputVector;
			}
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state) { }
	}

	// NetcodePlayerMovementSys
	[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
	partial struct NetcodePlayerMovementSys : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state) { }

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			foreach ((
					RefRO<NetcodePlayerInput> netcodePlayerInput,
					RefRW<LocalTransform> localTransform)
					in SystemAPI.Query<
						RefRO<NetcodePlayerInput>,
						RefRW<LocalTransform>>()
						.WithAll<Simulate>())
			{
				float moveSpeed = 10f;
				float3 moveVector = new float3(
					netcodePlayerInput.ValueRO.inputVector.x,
					0f,
					netcodePlayerInput.ValueRO.inputVector.y);

				localTransform.ValueRW.Position += moveVector * moveSpeed * SystemAPI.Time.DeltaTime;
			}
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state) { }
	}

	// TestMyValueSys
	partial struct TestMyValueSys : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state) { }

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
		public void OnDestroy(ref SystemState state) { }
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

	// SimpleRPC
	public struct SimpleRPC : IRpcCommand
	{
		public int Value;
	}
}
