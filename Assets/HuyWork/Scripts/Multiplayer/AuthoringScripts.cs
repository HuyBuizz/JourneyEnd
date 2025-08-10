// AuthoringScripts.cs
// Merged authoring scripts: EntitiesReferencesAuthoring, MyValueAuthoring, NetcodePlayerInputAuthoring, PlayerAuthoring

using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;

namespace Multiplayer.Authoring
{
	// EntitiesReferencesAuthoring
	public class EntitiesReferencesAuthoring : MonoBehaviour
	{
		public GameObject playerPrefabGameObject;

		public class Baker : Baker<EntitiesReferencesAuthoring>
		{
			public override void Bake(EntitiesReferencesAuthoring authoring)
			{
				Entity entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new EntitiesReferences
				{
					playerPrefabEntity = GetEntity(authoring.playerPrefabGameObject, TransformUsageFlags.Dynamic),
				});
			}
		}
	}

	public struct EntitiesReferences : IComponentData
	{
		public Entity playerPrefabEntity;
	}

	// MyValueAuthoring
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

	// NetcodePlayerInputAuthoring
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

	// PlayerAuthoring
	public class PlayerAuthoring : MonoBehaviour
	{
		public class Baker : Baker<PlayerAuthoring>
		{
			public override void Bake(PlayerAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new EPlayer());
			}
		}
	}

	public struct EPlayer : IComponentData
	{
	}
}
