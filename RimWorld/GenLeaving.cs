using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenLeaving
	{
		private const int LeavingStackBaseSize = 50;

		private const int LeavingMinStackSize = 15;

		private const int LeavingStackVariance = 5;

		private const float TerrainResourcesLeaveFraction = 0.75f;

		private static List<IntVec3> tmpCellsCandidates = new List<IntVec3>();

		public static void DoLeavingsFor(Thing diedThing, DestroyMode mode)
		{
			GenLeaving.DoLeavingsFor(diedThing, mode, diedThing.OccupiedRect());
		}

		public static void DoLeavingsFor(Thing diedThing, DestroyMode mode, CellRect leavingsRect)
		{
			if (Current.ProgramState != ProgramState.MapPlaying || mode == DestroyMode.Vanish)
			{
				return;
			}
			if (mode == DestroyMode.Kill && diedThing.def.filthLeaving != null)
			{
				for (int i = leavingsRect.minZ; i <= leavingsRect.maxZ; i++)
				{
					for (int j = leavingsRect.minX; j <= leavingsRect.maxX; j++)
					{
						IntVec3 c = new IntVec3(j, 0, i);
						FilthMaker.MakeFilth(c, diedThing.def.filthLeaving, Rand.RangeInclusive(1, 3));
					}
				}
			}
			Frame frame = diedThing as Frame;
			ThingContainer thingContainer;
			if (frame != null)
			{
				thingContainer = frame.resourceContainer;
			}
			else
			{
				thingContainer = new ThingContainer();
				if (mode == DestroyMode.Kill && diedThing.def.killedLeavings != null)
				{
					for (int k = 0; k < diedThing.def.killedLeavings.Count; k++)
					{
						Thing thing = ThingMaker.MakeThing(diedThing.def.killedLeavings[k].thingDef, null);
						thing.stackCount = diedThing.def.killedLeavings[k].count;
						thingContainer.TryAdd(thing);
					}
				}
				float resourcesLeaveFraction = GenLeaving.GetResourcesLeaveFraction(diedThing, mode);
				if (resourcesLeaveFraction > 0f)
				{
					List<ThingCount> list = diedThing.CostListAdjusted();
					for (int l = 0; l < list.Count; l++)
					{
						ThingCount thingCount = list[l];
						int num = GenMath.RoundRandom((float)thingCount.count * resourcesLeaveFraction);
						if (num > 0 && mode == DestroyMode.Kill && thingCount.thingDef.slagDef != null)
						{
							int count = thingCount.thingDef.slagDef.smeltProducts.First((ThingCount pro) => pro.thingDef == ThingDefOf.Steel).count;
							int num2 = num / 2 / 8;
							for (int m = 0; m < num2; m++)
							{
								thingContainer.TryAdd(ThingMaker.MakeThing(thingCount.thingDef.slagDef, null));
							}
							num -= num2 * count;
						}
						if (num > 0)
						{
							Thing thing2 = ThingMaker.MakeThing(thingCount.thingDef, null);
							thing2.stackCount = num;
							thingContainer.TryAdd(thing2);
						}
					}
				}
			}
			List<IntVec3> list2 = leavingsRect.Cells.InRandomOrder(null).ToList<IntVec3>();
			int num3 = 0;
			while (thingContainer.Count > 0)
			{
				if (mode == DestroyMode.Kill && !Find.AreaHome[list2[num3]])
				{
					thingContainer[0].SetForbidden(true, false);
				}
				Thing thing3;
				if (!thingContainer.TryDrop(thingContainer[0], list2[num3], ThingPlaceMode.Near, out thing3, null))
				{
					Log.Warning(string.Concat(new object[]
					{
						"Failed to place all leavings for destroyed thing ",
						diedThing,
						" at ",
						leavingsRect.CenterCell
					}));
					return;
				}
				num3++;
				if (num3 >= list2.Count)
				{
					num3 = 0;
				}
			}
		}

		public static void DoLeavingsFor(TerrainDef terrain, IntVec3 cell)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			ThingContainer thingContainer = new ThingContainer();
			List<ThingCount> list = terrain.CostListAdjusted(null, true);
			for (int i = 0; i < list.Count; i++)
			{
				ThingCount thingCount = list[i];
				int num = GenMath.RoundRandom((float)thingCount.count * 0.75f);
				if (num > 0)
				{
					Thing thing = ThingMaker.MakeThing(thingCount.thingDef, null);
					thing.stackCount = num;
					thingContainer.TryAdd(thing);
				}
			}
			while (thingContainer.Count > 0)
			{
				Thing thing2;
				if (!thingContainer.TryDrop(thingContainer[0], cell, ThingPlaceMode.Near, out thing2, null))
				{
					Log.Warning(string.Concat(new object[]
					{
						"Failed to place all leavings for removed terrain ",
						terrain,
						" at ",
						cell
					}));
					return;
				}
			}
		}

		public static float GetResourcesLeaveFraction(Thing diedThing, DestroyMode mode)
		{
			if (diedThing is Building && (diedThing.def.leaveResourcesWhenKilled || mode == DestroyMode.Deconstruct))
			{
				float result = 0.5f;
				if (mode == DestroyMode.Deconstruct)
				{
					result = diedThing.def.resourcesFractionWhenDeconstructed;
				}
				return result;
			}
			return 0f;
		}

		public static void DropFilthDueToDamage(Thing t, float damageDealt)
		{
			if (!t.def.useHitPoints || t.Destroyed || t.def.filthLeaving == null)
			{
				return;
			}
			CellRect cellRect = t.OccupiedRect().ExpandedBy(1);
			GenLeaving.tmpCellsCandidates.Clear();
			foreach (IntVec3 current in cellRect)
			{
				if (current.InBounds() && current.Walkable())
				{
					GenLeaving.tmpCellsCandidates.Add(current);
				}
			}
			if (!GenLeaving.tmpCellsCandidates.Any<IntVec3>())
			{
				return;
			}
			int num = GenMath.RoundRandom(damageDealt * Mathf.Min(0.0166666675f, 1f / ((float)t.MaxHitPoints / 10f)));
			for (int i = 0; i < num; i++)
			{
				FilthMaker.MakeFilth(GenLeaving.tmpCellsCandidates.RandomElement<IntVec3>(), t.def.filthLeaving, 1);
			}
		}
	}
}
