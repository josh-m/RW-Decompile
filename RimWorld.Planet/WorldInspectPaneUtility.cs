using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldInspectPaneUtility
	{
		public static string AdjustedLabelFor(List<WorldObject> worldObjects, Rect rect)
		{
			if (worldObjects.Count == 1)
			{
				return worldObjects[0].LabelCap;
			}
			if (WorldInspectPaneUtility.AllLabelsAreSame(worldObjects))
			{
				return worldObjects[0].LabelCap + " x" + worldObjects.Count;
			}
			return "VariousLabel".Translate();
		}

		private static bool AllLabelsAreSame(List<WorldObject> worldObjects)
		{
			for (int i = 0; i < worldObjects.Count; i++)
			{
				string labelCap = worldObjects[i].LabelCap;
				for (int j = i + 1; j < worldObjects.Count; j++)
				{
					if (labelCap != worldObjects[j].LabelCap)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
