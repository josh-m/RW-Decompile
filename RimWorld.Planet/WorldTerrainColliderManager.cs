using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class WorldTerrainColliderManager
	{
		private static GameObject gameObjectInt;

		public static GameObject GameObject
		{
			get
			{
				return WorldTerrainColliderManager.gameObjectInt;
			}
		}

		static WorldTerrainColliderManager()
		{
			WorldTerrainColliderManager.gameObjectInt = WorldTerrainColliderManager.CreateGameObject();
		}

		private static GameObject CreateGameObject()
		{
			GameObject gameObject = new GameObject("WorldTerrainCollider");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.layer = WorldCameraManager.WorldLayer;
			return gameObject;
		}
	}
}
