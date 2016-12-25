using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Seed : Projectile
	{
		protected override void Impact(Thing hitThing)
		{
			if (this.def.seed_PlantDefToMake.CanEverPlantAt(base.Position) && GenPlant.SnowAllowsPlanting(base.Position))
			{
				float b = Find.FertilityGrid.FertilityAt(base.Position);
				float num = Mathf.Lerp(1f, b, this.def.seed_PlantDefToMake.plant.fertilityFactorPlantChance);
				if (Rand.Value < num)
				{
					GenSpawn.Spawn(this.def.seed_PlantDefToMake, base.Position);
				}
			}
			this.Destroy(DestroyMode.Vanish);
		}
	}
}
