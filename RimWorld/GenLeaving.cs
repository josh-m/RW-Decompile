using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenLeaving
	{
		private const float LeaveFraction_Kill = 0.5f;

		private const float LeaveFraction_Cancel = 1f;

		public const float LeaveFraction_DeconstructDefault = 0.75f;

		private const float LeaveFraction_DeconstructFloor = 0.75f;

		private const float LeaveFraction_FailConstruction = 0.5f;

		private static List<IntVec3> tmpCellsCandidates = new List<IntVec3>();

		public static void DoLeavingsFor(Thing diedThing, Map map, DestroyMode mode)
		{
			GenLeaving.DoLeavingsFor(diedThing, map, mode, diedThing.OccupiedRect());
		}

		public static void DoLeavingsFor(Thing diedThing, Map map, DestroyMode mode, CellRect leavingsRect)
		{
			if (Current.ProgramState != ProgramState.Playing || mode == DestroyMode.Vanish)
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
						FilthMaker.MakeFilth(c, map, diedThing.def.filthLeaving, Rand.RangeInclusive(1, 3));
					}
				}
			}
			ThingContainer thingContainer = new ThingContainer();
			if (mode == DestroyMode.Kill && diedThing.def.killedLeavings != null)
			{
				for (int k = 0; k < diedThing.def.killedLeavings.Count; k++)
				{
					Thing thing = ThingMaker.MakeThing(diedThing.def.killedLeavings[k].thingDef, null);
					thing.stackCount = diedThing.def.killedLeavings[k].count;
					thingContainer.TryAdd(thing, true);
				}
			}
			float buildingResourcesLeaveFraction = GenLeaving.GetBuildingResourcesLeaveFraction(diedThing, mode);
			if (buildingResourcesLeaveFraction > 0f)
			{
				Frame frame = diedThing as Frame;
				if (frame != null)
				{
					for (int l = 0; l < frame.resourceContainer.Count; l++)
					{
						int num = GenMath.RoundRandom((float)frame.resourceContainer[l].stackCount * buildingResourcesLeaveFraction);
						if (num > 0)
						{
							thingContainer.TryAdd(frame.resourceContainer[l], num);
						}
					}
				}
				else
				{
					List<ThingCountClass> list = diedThing.CostListAdjusted();
					for (int m = 0; m < list.Count; m++)
					{
						ThingCountClass thingCountClass = list[m];
						int num2 = GenMath.RoundRandom((float)thingCountClass.count * buildingResourcesLeaveFraction);
						if (num2 > 0 && mode == DestroyMode.Kill && thingCountClass.thingDef.slagDef != null)
						{
							int count = thingCountClass.thingDef.slagDef.smeltProducts.First((ThingCountClass pro) => pro.thingDef == ThingDefOf.Steel).count;
							int num3 = num2 / 2 / 8;
							for (int n = 0; n < num3; n++)
							{
								thingContainer.TryAdd(ThingMaker.MakeThing(thingCountClass.thingDef.slagDef, null), true);
							}
							num2 -= num3 * count;
						}
						if (num2 > 0)
						{
							Thing thing2 = ThingMaker.MakeThing(thingCountClass.thingDef, null);
							thing2.stackCount = num2;
							thingContainer.TryAdd(thing2, true);
						}
					}
				}
			}
			List<IntVec3> list2 = leavingsRect.Cells.InRandomOrder(null).ToList<IntVec3>();
			int num4 = 0;
			while (thingContainer.Count > 0)
			{
				if (mode == DestroyMode.Kill && !map.areaManager.Home[list2[num4]])
				{
					thingContainer[0].SetForbidden(true, false);
				}
				Thing thing3;
				if (!thingContainer.TryDrop(thingContainer[0], list2[num4], map, ThingPlaceMode.Near, out thing3, null))
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
				num4++;
				if (num4 >= list2.Count)
				{
					num4 = 0;
				}
			}
		}

		public static void DoLeavingsFor(TerrainDef terrain, IntVec3 cell, Map map)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			ThingContainer thingContainer = new ThingContainer();
			List<ThingCountClass> list = terrain.CostListAdjusted(null, true);
			for (int i = 0; i < list.Count; i++)
			{
				ThingCountClass thingCountClass = list[i];
				int num = GenMath.RoundRandom((float)thingCountClass.count * 0.75f);
				if (num > 0)
				{
					Thing thing = ThingMaker.MakeThing(thingCountClass.thingDef, null);
					thing.stackCount = num;
					thingContainer.TryAdd(thing, true);
				}
			}
			while (thingContainer.Count > 0)
			{
				Thing thing2;
				if (!thingContainer.TryDrop(thingContainer[0], cell, map, ThingPlaceMode.Near, out thing2, null))
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

		public static float GetBuildingResourcesLeaveFraction(Thing diedThing, DestroyMode mode)
		{
			if (!(diedThing is Building))
			{
				return 0f;
			}
			if (mode == DestroyMode.Kill && !diedThing.def.leaveResourcesWhenKilled)
			{
				return 0f;
			}
			switch (mode)
			{
			case DestroyMode.Vanish:
				return 0f;
			case DestroyMode.Kill:
				return 0.5f;
			case DestroyMode.Deconstruct:
				return diedThing.def.resourcesFractionWhenDeconstructed;
			case DestroyMode.FailConstruction:
				return 0.5f;
			case DestroyMode.Cancel:
				return 1f;
			default:
				throw new ArgumentException("Unknown destroy mode " + mode);
			}
		}

		public static void DropFilthDueToDamage(Thing t, float damageDealt)
		{
			if (!t.def.useHitPoints || !t.Spawned || t.def.filthLeaving == null)
			{
				return;
			}
			CellRect cellRect = t.OccupiedRect().ExpandedBy(1);
			GenLeaving.tmpCellsCandidates.Clear();
			foreach (IntVec3 current in cellRect)
			{
				if (current.InBounds(t.Map) && current.Walkable(t.Map))
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
				FilthMaker.MakeFilth(GenLeaving.tmpCellsCandidates.RandomElement<IntVec3>(), t.Map, t.def.filthLeaving, 1);
			}
		}
	}
}
