using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterDeepResourceLumps : GenStep_Scatterer
	{
		private const float LumpSizeFactor = 1.6f;

		public override void Generate(Map map)
		{
			if (map.TileInfo.WaterCovered)
			{
				return;
			}
			int num = base.CalculateFinalCount(map);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec;
				if (!this.TryFindScatterCell(map, out intVec))
				{
					return;
				}
				this.ScatterAt(intVec, map, 1);
				this.usedSpots.Add(intVec);
			}
			this.usedSpots.Clear();
		}

		protected ThingDef ChooseThingDef()
		{
			return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight((ThingDef def) => def.deepCommonality);
		}

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			return !base.NearUsedSpot(c, this.minSpacing);
		}

		protected override void ScatterAt(IntVec3 c, Map map, int stackCount = 1)
		{
			ThingDef thingDef = this.ChooseThingDef();
			int numCells = Mathf.CeilToInt((float)this.GetScatterLumpSizeRange(thingDef).RandomInRange * 1.6f);
			foreach (IntVec3 current in GridShapeMaker.IrregularLump(c, map, numCells))
			{
				map.deepResourceGrid.SetAt(current, thingDef, thingDef.deepCountPerCell);
			}
		}

		private IntRange GetScatterLumpSizeRange(ThingDef def)
		{
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].building != null && allDefsListForReading[i].building.mineableThing == def)
				{
					return allDefsListForReading[i].building.mineableScatterLumpSizeRange;
				}
			}
			return new IntRange(2, 30);
		}
	}
}
