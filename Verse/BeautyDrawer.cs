using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	internal static class BeautyDrawer
	{
		private static Color ColorUgly = Color.red;

		private static Color ColorBeautiful = Color.green;

		private static List<Thing> tempCountedThings = new List<Thing>();

		public static void BeautyOnGUI()
		{
			if (!Find.PlaySettings.showBeauty)
			{
				return;
			}
			if (!Gen.MouseCell().InBounds() || Gen.MouseCell().Fogged())
			{
				return;
			}
			BeautyDrawer.tempCountedThings.Clear();
			BeautyUtility.FillBeautyRelevantCells(Gen.MouseCell());
			for (int i = 0; i < BeautyUtility.beautyRelevantCells.Count; i++)
			{
				IntVec3 intVec = BeautyUtility.beautyRelevantCells[i];
				float num = BeautyUtility.CellBeauty(intVec, BeautyDrawer.tempCountedThings);
				if (num != 0f)
				{
					Vector3 v = GenWorldUI.LabelDrawPosFor(intVec);
					GenWorldUI.DrawThingLabel(v, Mathf.RoundToInt(num).ToStringCached(), BeautyDrawer.BeautyColor(num));
				}
			}
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(Event.current.mousePosition.x + 19f, Event.current.mousePosition.y + 19f, 100f, 100f);
			float beauty = BeautyUtility.AverageBeautyPerceptible(Gen.MouseCell());
			GUI.color = BeautyDrawer.BeautyColor(beauty);
			Widgets.Label(rect, beauty.ToString("F1"));
			GUI.color = Color.white;
		}

		private static Color BeautyColor(float beauty)
		{
			float num = Mathf.InverseLerp(-40f, 40f, beauty);
			num = Mathf.Clamp01(num);
			return Color.Lerp(BeautyDrawer.ColorUgly, BeautyDrawer.ColorBeautiful, num);
		}
	}
}
