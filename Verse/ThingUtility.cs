using RimWorld.Planet;
using System;
using UnityEngine;

namespace Verse
{
	public static class ThingUtility
	{
		public static bool DestroyedOrNull(this Thing t)
		{
			return t == null || t.Destroyed;
		}

		public static void DestroyOrPassToWorld(this Thing t, DestroyMode mode = DestroyMode.Vanish)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				if (!Find.WorldPawns.Contains(pawn))
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
				}
			}
			else
			{
				t.Destroy(mode);
			}
		}

		public static int TryAsborbStackNumToTake(Thing thing, Thing other, bool respectStackLimit)
		{
			int result;
			if (respectStackLimit)
			{
				result = Mathf.Min(other.stackCount, thing.def.stackLimit - thing.stackCount);
			}
			else
			{
				result = other.stackCount;
			}
			return result;
		}

		public static void UpdateRegionListers(IntVec3 prevPos, IntVec3 newPos, Map map, Thing thing)
		{
			ThingDef def = thing.def;
			if (ListerThings.EverListable(def, ListerThingsUse.Region) && def.passability != Traversability.Impassable)
			{
				RegionGrid regionGrid = map.regionGrid;
				Region region = null;
				if (prevPos.InBounds(map))
				{
					region = regionGrid.GetValidRegionAt(prevPos);
				}
				Region region2 = null;
				if (newPos.InBounds(map))
				{
					region2 = regionGrid.GetValidRegionAt(newPos);
				}
				if (region2 != region)
				{
					if (region != null)
					{
						region.ListerThings.Remove(thing);
					}
					if (region2 != null)
					{
						region2.ListerThings.Add(thing);
					}
				}
			}
		}
	}
}
