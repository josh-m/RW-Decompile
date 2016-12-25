using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class InstallationDesignatorDatabase
	{
		private static Dictionary<ThingDef, Designator_Install> designators = new Dictionary<ThingDef, Designator_Install>();

		public static Designator_Install DesignatorFor(ThingDef artDef)
		{
			Designator_Install designator_Install;
			if (InstallationDesignatorDatabase.designators.TryGetValue(artDef, out designator_Install))
			{
				return designator_Install;
			}
			designator_Install = InstallationDesignatorDatabase.NewDesignatorFor(artDef);
			InstallationDesignatorDatabase.designators.Add(artDef, designator_Install);
			return designator_Install;
		}

		private static Designator_Install NewDesignatorFor(ThingDef artDef)
		{
			return new Designator_Install
			{
				hotKey = KeyBindingDefOf.Misc1
			};
		}
	}
}
