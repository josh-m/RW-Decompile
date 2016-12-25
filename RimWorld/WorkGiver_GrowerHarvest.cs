using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_GrowerHarvest : WorkGiver_Grower
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c)
		{
			Plant plant = c.GetPlant();
			if (plant == null)
			{
				return null;
			}
			if (plant.IsForbidden(pawn))
			{
				return null;
			}
			if (!plant.def.plant.Harvestable || plant.LifeStage != PlantLifeStage.Mature)
			{
				return null;
			}
			if (!pawn.CanReserve(plant, 1))
			{
				return null;
			}
			return new Job(JobDefOf.Harvest, plant);
		}
	}
}
