using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenWorld
	{
		public const float MaxRayLength = 1500f;

		private static int cachedTile = -1;

		private static int cachedFrame = -1;

		private static List<WorldObject> tmpWorldObjectsUnderMouse = new List<WorldObject>();

		public static int MouseTile(bool snapToExpandableWorldObjects = false)
		{
			if (GenWorld.cachedFrame == Time.frameCount)
			{
				return GenWorld.cachedTile;
			}
			GenWorld.cachedTile = GenWorld.TileAt(UI.MousePositionOnUI, snapToExpandableWorldObjects);
			GenWorld.cachedFrame = Time.frameCount;
			return GenWorld.cachedTile;
		}

		public static int TileAt(Vector2 clickPos, bool snapToExpandableWorldObjects = false)
		{
			Camera worldCamera = Find.WorldCamera;
			if (!worldCamera.gameObject.activeInHierarchy)
			{
				return -1;
			}
			if (snapToExpandableWorldObjects)
			{
				ExpandableWorldObjectsUtility.GetExpandedWorldObjectUnderMouse(UI.MousePositionOnUI, GenWorld.tmpWorldObjectsUnderMouse);
				if (GenWorld.tmpWorldObjectsUnderMouse.Any<WorldObject>())
				{
					int tile = GenWorld.tmpWorldObjectsUnderMouse[0].Tile;
					GenWorld.tmpWorldObjectsUnderMouse.Clear();
					return tile;
				}
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
