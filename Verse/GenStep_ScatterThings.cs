using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class GenStep_ScatterThings : GenStep_Scatterer
	{
		public ThingDef thingDef;

		public ThingDef stuff;

		public int clearSpaceSize;

		public TerrainAffordance neededTerrainAffordance;

		public int clusterSize = 1;

		private int leftInCluster;

		private IntVec3 clusterCenter;

		private int ClusterRadius = 4;

		public override void Generate()
		{
			int count = base.CalculateFinalCount();
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
				if (!this.TryFindScatterCell(out intVec))
				{
					return;
				}
				this.ScatterAt(intVec, list[i]);
				this.usedSpots.Add(intVec);
			}
		}

		protected override bool TryFindScatterCell(out IntVec3 result)
		{
			if (this.clusterSize > 1)
			{
				if (this.leftInCluster <= 0)
				{
					this.leftInCluster = this.clusterSize;
					this.FindNewClusterCenter();
				}
				this.leftInCluster--;
				result = CellFinder.RandomClosewalkCellNear(this.clusterCenter, this.ClusterRadius);
				return result.IsValid;
			}
			return base.TryFindScatterCell(out result);
		}

		private void FindNewClusterCenter()
		{
			if (!base.TryFindScatterCell(out this.clusterCenter))
			{
				Log.Error("Could not find cluster center to scatter " + this.thingDef);
			}
		}

		protected override void ScatterAt(IntVec3 loc, int stackCount = 1)
		{
			Rot4 rotation = (!this.thingDef.rotatable) ? Rot4.North : Rot4.Random;
			if (this.neededTerrainAffordance != TerrainAffordance.Undefined)
			{
				foreach (IntVec3 current in GenAdj.CellsOccupiedBy(loc, rotation, this.thingDef.size))
				{
					if (!current.SupportsStructureType(this.neededTerrainAffordance))
					{
						return;
					}
				}
			}
			if (this.clearSpaceSize > 0)
			{
				foreach (IntVec3 current2 in GridShapeMaker.IrregularLump(loc, this.clearSpaceSize))
				{
					Thing edifice = current2.GetEdifice();
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
			thing.Rotation = rotation;
			thing.stackCount = stackCount;
			thing.SetForbidden(true, false);
			Thing thing2;
			GenPlace.TryPlaceThing(thing, loc, ThingPlaceMode.Near, out thing2, null);
			if (this.nearPlayerStart && thing2 != null && thing2.def.category == ThingCategory.Item && Find.TutorialState != null)
			{
				Find.TutorialState.AddStartingItem(thing2);
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
