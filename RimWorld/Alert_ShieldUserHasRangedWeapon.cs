using System;
using Verse;

namespace RimWorld
{
	public class Alert_ShieldUserHasRangedWeapon : Alert
	{
		public Alert_ShieldUserHasRangedWeapon()
		{
			this.defaultLabel = "ShieldUserHasRangedWeapon".Translate();
			this.defaultExplanation = "ShieldUserHasRangedWeaponDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			foreach (Pawn current in PawnsFinder.AllMaps_FreeColonistsSpawned)
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
}
