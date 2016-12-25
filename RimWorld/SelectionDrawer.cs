using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class SelectionDrawer
	{
		private static Dictionary<object, float> selectTimes = new Dictionary<object, float>();

		private static readonly Material SelectionBracketMat = MaterialPool.MatFrom("UI/Overlays/SelectionBracket", ShaderDatabase.MetaOverlay);

		private static Vector3[] bracketLocs = new Vector3[4];

		public static Dictionary<object, float> SelectTimes
		{
			get
			{
				return SelectionDrawer.selectTimes;
			}
		}

		public static void Notify_Selected(object t)
		{
			SelectionDrawer.selectTimes[t] = Time.realtimeSinceStartup;
		}

		public static void Clear()
		{
			SelectionDrawer.selectTimes.Clear();
		}

		public static void DrawSelectionOverlays()
		{
			foreach (object current in Find.Selector.SelectedObjects)
			{
				SelectionDrawer.DrawSelectionBracketFor(current);
				Thing thing = current as Thing;
				if (thing != null)
				{
					thing.DrawExtraSelectionOverlays();
				}
			}
		}

		private static void DrawSelectionBracketFor(object obj)
		{
			Zone zone = obj as Zone;
			if (zone != null)
			{
				GenDraw.DrawFieldEdges(zone.Cells);
			}
			Thing thing = obj as Thing;
			if (thing != null)
			{
				SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld<object>(SelectionDrawer.bracketLocs, thing, thing.DrawPos, thing.RotatedSize.ToVector2(), SelectionDrawer.selectTimes, Vector2.one, 1f);
				int num = 0;
				for (int i = 0; i < 4; i++)
				{
					Quaternion rotation = Quaternion.AngleAxis((float)num, Vector3.up);
					Graphics.DrawMesh(MeshPool.plane10, SelectionDrawer.bracketLocs[i], rotation, SelectionDrawer.SelectionBracketMat, 0);
					num -= 90;
				}
			}
		}
	}
}
