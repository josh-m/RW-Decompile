using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenWorld
	{
		public const float MaxRayLength = 1500f;

		private static int cachedTile = -1;

		private static int cachedFrame = -1;

		public static int MouseTile()
		{
			if (GenWorld.cachedFrame == Time.frameCount)
			{
				return GenWorld.cachedTile;
			}
			GenWorld.cachedTile = GenWorld.TileAt(UI.MousePositionOnUI);
			GenWorld.cachedFrame = Time.frameCount;
			return GenWorld.cachedTile;
		}

		public static int TileAt(Vector2 clickPos)
		{
			Camera worldCamera = Find.WorldCamera;
			if (!worldCamera.gameObject.activeInHierarchy)
			{
				return -1;
			}
			Ray ray = worldCamera.ScreenPointToRay(clickPos * Prefs.UIScale);
			int worldLayerMask = WorldCameraManager.WorldLayerMask;
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1500f, worldLayerMask))
			{
				return Find.World.renderer.GetTileIDFromRayHit(hit);
			}
			return -1;
		}
	}
}
