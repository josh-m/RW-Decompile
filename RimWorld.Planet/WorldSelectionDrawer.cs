using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldSelectionDrawer
	{
		private const float BaseSelectedTexJump = 25f;

		private const float BaseSelectedTexScale = 0.4f;

		private const float BaseSelectionRectSize = 35f;

		private static Dictionary<WorldObject, float> selectTimes = new Dictionary<WorldObject, float>();

		private static readonly Color HiddenSelectionBracketColor = new Color(1f, 1f, 1f, 0.35f);

		private static Vector2[] bracketLocs = new Vector2[4];

		public static Dictionary<WorldObject, float> SelectTimes
		{
			get
			{
				return WorldSelectionDrawer.selectTimes;
			}
		}

		public static void Notify_Selected(WorldObject t)
		{
			WorldSelectionDrawer.selectTimes[t] = Time.realtimeSinceStartup;
		}

		public static void Clear()
		{
			WorldSelectionDrawer.selectTimes.Clear();
		}

		public static void SelectionOverlaysOnGUI()
		{
			List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
			for (int i = 0; i < selectedObjects.Count; i++)
			{
				WorldObject worldObject = selectedObjects[i];
				WorldSelectionDrawer.DrawSelectionBracketOnGUIFor(worldObject);
				worldObject.ExtraSelectionOverlaysOnGUI();
			}
		}

		public static void DrawSelectionOverlays()
		{
			List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
			for (int i = 0; i < selectedObjects.Count; i++)
			{
				WorldObject worldObject = selectedObjects[i];
				worldObject.DrawExtraSelectionOverlays();
			}
		}

		private static void DrawSelectionBracketOnGUIFor(WorldObject obj)
		{
			Vector2 vector = obj.ScreenPos();
			Rect rect = new Rect(vector.x - 17.5f, vector.y - 17.5f, 35f, 35f);
			Vector2 textureSize = new Vector2((float)SelectionDrawerUtility.SelectedTexGUI.width * 0.4f, (float)SelectionDrawerUtility.SelectedTexGUI.height * 0.4f);
			SelectionDrawerUtility.CalculateSelectionBracketPositionsUI<WorldObject>(WorldSelectionDrawer.bracketLocs, obj, rect, WorldSelectionDrawer.selectTimes, textureSize, 25f);
			if (obj.HiddenBehindTerrainNow())
			{
				GUI.color = WorldSelectionDrawer.HiddenSelectionBracketColor;
			}
			else
			{
				GUI.color = Color.white;
			}
			int num = 90;
			for (int i = 0; i < 4; i++)
			{
				Widgets.DrawTextureRotated(WorldSelectionDrawer.bracketLocs[i], SelectionDrawerUtility.SelectedTexGUI, (float)num, 0.4f);
				num += 90;
			}
			GUI.color = Color.white;
		}
	}
}
