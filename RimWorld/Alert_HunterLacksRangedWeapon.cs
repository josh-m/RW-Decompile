using System;
using Verse;

namespace RimWorld
{
	public class Alert_HunterLacksRangedWeapon : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
				foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
				{
					if (current.workSettings.WorkIsActive(WorkTypeDefOf.Hunting) && !WorkGiver_HunterHunt.HasHuntingWeapon(current) && !current.Downed)
					{
						return current;
					}
				}
				return false;
			}
		}

		public Alert_HunterLacksRangedWeapon()
		{
			this.baseLabel = "HunterLacksWeapon".Translate();
			this.baseExplanation = "HunterLacksWeaponDesc".Translate();
		}
	}
}
