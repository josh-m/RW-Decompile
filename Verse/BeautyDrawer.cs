using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	internal static class BeautyDrawer
	{
		private static List<Thing> beautyCountedThings = new List<Thing>();

		private static Color ColorUgly = Color.red;

		private static Color ColorBeautiful = Color.green;

		public static void DrawBeautyAroundMouse()
		{
			BeautyUtility.FillBeautyRelevantCells(UI.MouseCell(), Find.VisibleMap);
			for (int i = 0; i < BeautyUtility.beautyRelevantCells.Count; i++)
			{
				IntVec3 intVec = BeautyUtility.beautyRelevantCells[i];
				float num = BeautyUtility.CellBeauty(intVec, Find.VisibleMap, BeautyDrawer.beautyCountedThings);
				if (num != 0f)
				{
					Vector3 v = GenMapUI.LabelDrawPosFor(intVec);
					GenMapUI.DrawThingLabel(v, Mathf.RoundToInt(num).ToStringCached(), BeautyDrawer.BeautyColor(num, 8f));
				}
			}
			BeautyDrawer.beautyCountedThings.Clear();
		}

		public static Color BeautyColor(float beauty, float scale)
		{
			float num = Mathf.InverseLerp(-scale, scale, beauty);
			num = Mathf.Clamp01(num);
			Color a = Color.Lerp(BeautyDrawer.ColorUgly, BeautyDrawer.ColorBeautiful, num);
			return Color.Lerp(a, Color.white, 0.5f);
		}
	}
}
