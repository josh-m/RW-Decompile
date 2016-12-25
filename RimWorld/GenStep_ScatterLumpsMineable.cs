using System;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterLumpsMineable : GenStep_Scatterer
	{
		public ThingDef forcedDefToScatter;

		public override void Generate(Map map)
		{
			this.minSpacing = 5f;
			this.warnOnFail = false;
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

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (base.NearUsedSpot(c, this.minSpacing))
			{
				return false;
			}
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice.def.building.isNaturalRock;
		}

		protected override void ScatterAt(IntVec3 c, Map map, int stackCount = 1)
		{
			ThingDef thingDef = this.ChooseThingDef();
			int randomInRange = thingDef.building.mineableScatterLumpSizeRange.RandomInRange;
			foreach (IntVec3 current in GridShapeMaker.IrregularLump(c, map, randomInRange))
			{
				GenSpawn.Spawn(thingDef, current, map);
			}
		}
	}
}
