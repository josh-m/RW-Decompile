using System;
using Verse;

namespace RimWorld
{
	public class Alert_HunterHasShieldAndRangedWeapon : Alert
	{
		public Alert_HunterHasShieldAndRangedWeapon()
		{
			this.defaultLabel = "HunterHasShieldAndRangedWeapon".Translate();
			this.defaultExplanation = "HunterHasShieldAndRangedWeaponDesc".Translate();
		}

		private Pawn BadHunter()
		{
			foreach (Pawn current in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				if (current.workSettings.WorkIsActive(WorkTypeDefOf.Hunting) && WorkGiver_HunterHunt.HasShieldAndRangedWeapon(current))
				{
					return current;
				}
			}
			return null;
		}

		public override AlertReport GetReport()
		{
			Pawn pawn = this.BadHunter();
			if (pawn == null)
			{
				return false;
			}
			return AlertReport.CulpritIs(pawn);
		}
	}
}
