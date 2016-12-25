using System;
using Verse;

namespace RimWorld
{
	public class WorkGiver_Uninstall : WorkGiver_RemoveBuilding
	{
		protected override DesignationDef Designation
		{
			get
			{
				return DesignationDefOf.Uninstall;
			}
		}

		protected override JobDef RemoveBuildingJob
		{
			get
			{
				return JobDefOf.Uninstall;
			}
		}
	}
}
