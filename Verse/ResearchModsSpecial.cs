using RimWorld;
using System;
using System.Linq;

namespace Verse
{
	public static class ResearchModsSpecial
	{
		public static void GunTurretCooling()
		{
			(from x in ThingDefOf.Gun_TurretImprovised.Verbs
			where x.isPrimary
			select x).First<VerbProperties>().burstShotCount = 4;
		}
	}
}
