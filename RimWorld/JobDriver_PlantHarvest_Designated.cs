using System;
using Verse;

namespace RimWorld
{
	public class JobDriver_PlantHarvest_Designated : JobDriver_PlantHarvest
	{
		protected override DesignationDef RequiredDesignation
		{
			get
			{
				return DesignationDefOf.HarvestPlant;
			}
		}
	}
}
