using System;
using Verse;

namespace RimWorld
{
	public class WorkGiver_Deconstruct : WorkGiver_RemoveBuilding
	{
		protected override DesignationDef Designation
		{
			get
			{
				return DesignationDefOf.Deconstruct;
			}
		}

		protected override JobDef RemoveBuildingJob
		{
			get
			{
				return JobDefOf.Deconstruct;
			}
		}
	}
}
