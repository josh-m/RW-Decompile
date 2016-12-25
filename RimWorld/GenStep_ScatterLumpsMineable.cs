using System;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterLumpsMineable : GenStep_Scatterer
	{
		public ThingDef forcedDefToScatter;

		public override void Generate()
		{
			this.minSpacing = 5f;
			this.warnOnFail = false;
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
			if (this.forcedDefToScatter != null)
			{
				return this.forcedDefToScatter;
			}
			return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight(delegate(ThingDef d)
			{
				if (d.building == null)
				{
					return 0f;
				}
				return d.building.mineableScatterCommonality;
			});
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
			Building edifice = c.GetEdifice();
			return edifice != null && edifice.def.building.isNaturalRock;
		}

		protected override void ScatterAt(IntVec3 c, int stackCount = 1)
		{
			ThingDef thingDef = this.ChooseThingDef();
			int randomInRange = thingDef.building.mineableScatterLumpSizeRange.RandomInRange;
			foreach (IntVec3 current in GridShapeMaker.IrregularLump(c, randomInRange))
			{
				GenSpawn.Spawn(thingDef, current);
			}
		}
	}
}
