using System;
using Verse;

namespace RimWorld
{
	public class Alert_HunterHasShieldAndRangedWeapon : Alert_Medium
	{
		public override AlertReport Report
		{
			get
			{
				Pawn pawn = this.BadHunter();
				if (pawn == null)
				{
					return false;
				}
				return AlertReport.CulpritIs(pawn);
			}
		}

		public Alert_HunterHasShieldAndRangedWeapon()
		{
			this.baseLabel = "HunterHasShieldAndRangedWeapon".Translate();
			this.baseExplanation = "HunterHasShieldAndRangedWeaponDesc".Translate();
		}

		private Pawn BadHunter()
		{
			foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
			{
				if (current.workSettings.WorkIsActive(WorkTypeDefOf.Hunting) && WorkGiver_HunterHunt.HasShieldAndRangedWeapon(current))
				{
					return current;
				}
			}
			return null;
		}
	}
}
