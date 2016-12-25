using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class InstallBlueprintUtility
	{
		public static void CancelBlueprintsFor(Thing th)
		{
			Blueprint_Install blueprint_Install = InstallBlueprintUtility.ExistingBlueprintFor(th);
			if (blueprint_Install != null)
			{
				blueprint_Install.Destroy(DestroyMode.Cancel);
			}
		}

		public static Blueprint_Install ExistingBlueprintFor(Thing th)
		{
			ThingDef installBlueprintDef = th.GetInnerIfMinified().def.installBlueprintDef;
			if (installBlueprintDef == null)
			{
				return null;
			}
			List<Thing> list = Find.ListerThings.ThingsMatching(ThingRequest.ForDef(installBlueprintDef));
			for (int i = 0; i < list.Count; i++)
			{
				Blueprint_Install blueprint_Install = list[i] as Blueprint_Install;
				if (blueprint_Install != null && blueprint_Install.MiniToInstallOrBuildingToReinstall == th)
				{
					return blueprint_Install;
				}
			}
			return null;
		}
	}
}
