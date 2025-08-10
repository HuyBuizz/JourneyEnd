// NetcodeBootstrap.cs
// Merged bootstrap and tick rate systems: GameBootstrap, ConfigureTickRateSystem, RunInBackground

using UnityEngine;
using Unity.NetCode;
using Unity.Entities;

namespace Multiplayer.NetcodeBootstrap
{
	// GameBootstrap
	[UnityEngine.Scripting.Preserve]
	public class GameBootstrap : ClientServerBootstrap
	{
		public override bool Initialize(string defaultWorldName)
		{
			AutoConnectPort = 7979;
			return base.Initialize(defaultWorldName);
		}
	}

	// ConfigureTickRateSystem
	[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public partial struct ConfigureTickRateSystem : ISystem
	{
		public void OnCreate(ref SystemState state)
		{
			var e = state.EntityManager.CreateEntity();
			state.EntityManager.AddComponentData(e, new ClientServerTickRate
			{
				SimulationTickRate = 30,
				NetworkTickRate     = 30,
				MaxSimulationStepsPerFrame = 3,
				// MaxSimulationStepBatchSize = 1, // nếu field có trong bản bạn dùng
			});
			state.Enabled = false;
		}
	}

	// RunInBackground
	public class RunInBackground
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void ForceRunInBackground()
		{
			Application.runInBackground = true;
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 120;
		}
	}
}
