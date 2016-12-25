using System;
using Verse;

namespace RimWorld
{
	public class Alert_HunterLacksRangedWeapon : Alert
	{
		public Alert_HunterLacksRangedWeapon()
		{
			this.defaultLabel = "HunterLacksWeapon".Translate();
			this.defaultExplanation = "HunterLacksWeaponDesc".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			foreach (Pawn current in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				if (current.workSettings.WorkIsActive(WorkTypeDefOf.Hunting) && !WorkGiver_HunterHunt.HasHuntingWeapon(current) && !current.Downed)
				{
					return current;
				}
			}
			return false;
		}
	}
}
