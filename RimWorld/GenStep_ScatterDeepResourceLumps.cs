using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterDeepResourceLumps : GenStep_Scatterer
	{
		private const int CountPerCell = 150;

		private const float LumpSizeFactor = 1.6f;

		public override void Generate()
		{
			int num = base.CalculateFinalCount();
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec;
				if (!this.TryFindScatterCell(out intVec))
				{
					return;
				}
				this.ScatterAt(intVec, 1);
				this.usedSpots.Add(intVec);
			}
		}

		protected ThingDef ChooseThingDef()
		{
			return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight((ThingDef def) => def.deepCommonality);
		}

		protected override bool CanScatterAt(IntVec3 c)
		{
			foreach (IntVec3 current in this.usedSpots)
			{
				if ((current - c).LengthHorizontal < this.minSpacing)
				{
					return false;
				}
			}
			return true;
		}

		protected override void ScatterAt(IntVec3 c, int stackCount = 1)
		{
			ThingDef def = this.ChooseThingDef();
			int numCells = Mathf.CeilToInt((float)this.GetScatterLumpSizeRange(def).RandomInRange * 1.6f);
			foreach (IntVec3 current in GridShapeMaker.IrregularLump(c, numCells))
			{
				Find.DeepResourceGrid.SetAt(current, def, 150);
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
