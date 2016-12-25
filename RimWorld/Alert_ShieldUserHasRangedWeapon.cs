using System;
using Verse;

namespace RimWorld
{
	public class Alert_ShieldUserHasRangedWeapon : Alert_Medium
	{
		public override AlertReport Report
		{
			get
			{
				foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
				{
					if (current.equipment.Primary != null && current.equipment.Primary.def.IsRangedWeapon)
					{
						if (current.apparel.WornApparel.Any((Apparel ap) => ap.def == ThingDefOf.Apparel_PersonalShield))
						{
							return current;
						}
					}
				}
				return false;
			}
		}

		public Alert_ShieldUserHasRangedWeapon()
		{
			this.baseLabel = "ShieldUserHasRangedWeapon".Translate();
			this.baseExplanation = "ShieldUserHasRangedWeaponDesc".Translate();
		}
	}
}
