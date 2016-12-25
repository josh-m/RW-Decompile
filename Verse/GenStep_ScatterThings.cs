using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class GenStep_ScatterThings : GenStep_Scatterer
	{
		private const int ClusterRadius = 4;

		public ThingDef thingDef;

		public ThingDef stuff;

		public int clearSpaceSize;

		public TerrainAffordance neededTerrainAffordance;

		public int clusterSize = 1;

		[Unsaved]
		private IntVec3 clusterCenter;

		[Unsaved]
		private int leftInCluster;

		public override void Generate(Map map)
		{
			if (!this.allowOnWater && map.TileInfo.WaterCovered)
			{
				return;
			}
			int count = base.CalculateFinalCount(map);
			IntRange one;
			if (this.thingDef.ingestible != null && this.thingDef.ingestible.IsMeal && this.thingDef.stackLimit <= 10)
			{
				one = IntRange.one;
			}
			else if (this.thingDef.stackLimit > 5)
			{
				one = new IntRange(Mathf.RoundToInt((float)this.thingDef.stackLimit * 0.5f), this.thingDef.stackLimit);
			}
			else
			{
				one = new IntRange(this.thingDef.stackLimit, this.thingDef.stackLimit);
			}
			List<int> list = GenStep_ScatterThings.CountDividedIntoStacks(count, one);
			for (int i = 0; i < list.Count; i++)
			{
				IntVec3 intVec;
				if (!this.TryFindScatterCell(map, out intVec))
				{
					return;
				}
				this.ScatterAt(intVec, map, list[i]);
				this.usedSpots.Add(intVec);
			}
			this.usedSpots.Clear();
			this.clusterCenter = IntVec3.Invalid;
			this.leftInCluster = 0;
		}

		protected override bool TryFindScatterCell(Map map, out IntVec3 result)
		{
			if (this.clusterSize > 1)
			{
				if (this.leftInCluster <= 0)
				{
					if (!base.TryFindScatterCell(map, out this.clusterCenter))
					{
						Log.Error("Could not find cluster center to scatter " + this.thingDef);
					}
					this.leftInCluster = this.clusterSize;
				}
				this.leftInCluster--;
				result = CellFinder.RandomClosewalkCellNear(this.clusterCenter, map, 4);
				return result.IsValid;
			}
			return base.TryFindScatterCell(map, out result);
		}

		protected override void ScatterAt(IntVec3 loc, Map map, int stackCount = 1)
		{
			Rot4 rot = (!this.thingDef.rotatable) ? Rot4.North : Rot4.Random;
			CellRect cellRect = GenAdj.OccupiedRect(loc, rot, this.thingDef.size);
			if (!cellRect.InBounds(map))
			{
				Log.Warning(string.Concat(new object[]
				{
					"Failed to scatter ",
					this.thingDef.defName,
					" at ",
					loc,
					" due to being out of bounds."
				}));
				return;
			}
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (this.neededTerrainAffordance != TerrainAffordance.Undefined && !current.SupportsStructureType(map, this.neededTerrainAffordance))
				{
					Log.Warning(string.Concat(new object[]
					{
						"Failed to scatter ",
						this.thingDef.defName,
						" at ",
						loc,
						" due to missing terrain affordance."
					}));
					return;
				}
				iterator.MoveNext();
			}
			if (this.clearSpaceSize > 0)
			{
				foreach (IntVec3 current2 in GridShapeMaker.IrregularLump(loc, map, this.clearSpaceSize))
				{
					Thing edifice = current2.GetEdifice(map);
					if (edifice != null)
					{
						edifice.Destroy(DestroyMode.Vanish);
					}
				}
			}
			Thing thing = ThingMaker.MakeThing(this.thingDef, this.stuff);
			if (this.thingDef.Minifiable)
			{
				thing = thing.MakeMinified();
			}
			if (thing.def.category == ThingCategory.Item)
			{
				thing.stackCount = stackCount;
				thing.SetForbidden(true, false);
				Thing thing2;
				GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Near, out thing2, null);
				if (this.nearPlayerStart && thing2 != null && thing2.def.category == ThingCategory.Item && TutorSystem.TutorialMode)
				{
					Find.TutorialState.AddStartingItem(thing2);
				}
			}
			else
			{
				GenSpawn.Spawn(thing, loc, map, rot);
			}
		}

		public static List<int> CountDividedIntoStacks(int count, IntRange stackSizeRange)
		{
			List<int> list = new List<int>();
			while (count > 0)
			{
				int num = Mathf.Min(count, stackSizeRange.RandomInRange);
				count -= num;
				list.Add(num);
			}
			if (stackSizeRange.max > 2)
			{
				for (int i = 0; i < list.Count * 4; i++)
				{
					int num2 = Rand.RangeInclusive(0, list.Count - 1);
					int num3 = Rand.RangeInclusive(0, list.Count - 1);
					if (num2 != num3)
					{
						if (list[num2] > list[num3])
						{
							int num4 = (int)((float)(list[num2] - list[num3]) * Rand.Value);
							List<int> list2;
							List<int> expr_9B = list2 = list;
							int num5;
							int expr_9F = num5 = num2;
							num5 = list2[num5];
							expr_9B[expr_9F] = num5 - num4;
							List<int> list3;
							List<int> expr_B8 = list3 = list;
							int expr_BD = num5 = num3;
							num5 = list3[num5];
							expr_B8[expr_BD] = num5 + num4;
						}
					}
				}
			}
			return list;
		}
	}
}
