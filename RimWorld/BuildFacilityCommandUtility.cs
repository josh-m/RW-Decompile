using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public static class BuildFacilityCommandUtility
	{
		[DebuggerHidden]
		public static IEnumerable<Command> BuildFacilityCommands(BuildableDef building)
		{
			ThingDef thingDef = building as ThingDef;
			if (thingDef != null)
			{
				CompProperties_AffectedByFacilities affectedByFacilities = thingDef.GetCompProperties<CompProperties_AffectedByFacilities>();
				if (affectedByFacilities != null)
				{
					for (int i = 0; i < affectedByFacilities.linkableFacilities.Count; i++)
					{
						ThingDef facility = affectedByFacilities.linkableFacilities[i];
						Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(facility, true);
						if (des != null)
						{
							yield return des;
						}
					}
				}
			}
		}
	}
}
