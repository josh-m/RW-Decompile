using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ThingSelectionUtility
	{
		private static HashSet<Thing> yieldedThings = new HashSet<Thing>();

		private static HashSet<Zone> yieldedZones = new HashSet<Zone>();

		public static bool SelectableNow(this Thing t)
		{
			if (!t.def.selectable || !t.Spawned)
			{
				return false;
			}
			if (t.def.size.x == 1 && t.def.size.z == 1)
			{
				return !t.Position.Fogged();
			}
			CellRect.CellRectIterator iterator = t.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				if (!iterator.Current.Fogged())
				{
					return true;
				}
				iterator.MoveNext();
			}
			return false;
		}

		[DebuggerHidden]
		public static IEnumerable<Thing> MultiSelectableThingsInScreenRectDistinct(Rect rect)
		{
			CellRect worldRect = ThingSelectionUtility.GetWorldRect(rect);
			ThingSelectionUtility.yieldedThings.Clear();
			foreach (IntVec3 c in worldRect)
			{
				if (c.InBounds())
				{
					List<Thing> cellThings = Find.ThingGrid.ThingsListAt(c);
					if (cellThings != null)
					{
						for (int i = 0; i < cellThings.Count; i++)
						{
							Thing t = cellThings[i];
							if (t.SelectableNow() && !t.def.neverMultiSelect && !ThingSelectionUtility.yieldedThings.Contains(t))
							{
								yield return t;
							}
						}
					}
				}
			}
		}

		[DebuggerHidden]
		public static IEnumerable<Zone> MultiSelectableZonesInScreenRectDistinct(Rect rect)
		{
			CellRect worldRect = ThingSelectionUtility.GetWorldRect(rect);
			ThingSelectionUtility.yieldedZones.Clear();
			foreach (IntVec3 c in worldRect)
			{
				if (c.InBounds())
				{
					Zone zone = c.GetZone();
					if (zone != null)
					{
						if (zone.IsMultiselectable)
						{
							if (!ThingSelectionUtility.yieldedZones.Contains(zone))
							{
								yield return zone;
							}
						}
					}
				}
			}
		}

		private static CellRect GetWorldRect(Rect rect)
		{
			Vector2 screenLoc = new Vector2(rect.x, (float)Screen.height - rect.y);
			Vector2 screenLoc2 = new Vector2(rect.x + rect.width, (float)Screen.height - (rect.y + rect.height));
			Vector3 vector = Gen.ScreenToWorldPoint(screenLoc);
			Vector3 vector2 = Gen.ScreenToWorldPoint(screenLoc2);
			return new CellRect
			{
				minX = Mathf.FloorToInt(vector.x),
				minZ = Mathf.FloorToInt(vector2.z),
				maxX = Mathf.FloorToInt(vector2.x),
				maxZ = Mathf.FloorToInt(vector.z)
			};
		}
	}
}
