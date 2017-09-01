using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_CropBlight : IncidentWorker
	{
		private const float MaxDaysToGrown = 15f;

		private const float KillChance = 0.8f;

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant);
			bool flag = false;
			IntVec3 cell = IntVec3.Invalid;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				Plant plant = (Plant)list[i];
				if (map.Biome.CommonalityOfPlant(plant.def) == 0f)
				{
					if (plant.def.plant.growDays <= 15f)
					{
						if (plant.sown)
						{
							if ((plant.LifeStage == PlantLifeStage.Growing || plant.LifeStage == PlantLifeStage.Mature) && Rand.Value < 0.8f)
							{
								flag = true;
								cell = plant.Position;
								plant.CropBlighted();
							}
						}
					}
				}
			}
			if (!flag)
			{
				return false;
			}
			Find.LetterStack.ReceiveLetter("LetterLabelCropBlight".Translate(), "CropBlight".Translate(), LetterDefOf.BadNonUrgent, new GlobalTargetInfo(cell, map, false), null);
			return true;
		}
	}
}
