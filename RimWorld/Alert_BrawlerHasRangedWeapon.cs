using System;
using Verse;

namespace RimWorld
{
	public class Alert_BrawlerHasRangedWeapon : Alert
	{
		public Alert_BrawlerHasRangedWeapon()
		{
			this.defaultLabel = "BrawlerHasRangedWeapon".Translate();
			this.defaultExplanation = "BrawlerHasRangedWeaponDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			foreach (Pawn current in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				if (current.story.traits.HasTrait(TraitDefOf.Brawler) && current.equipment.Primary != null && current.equipment.Primary.def.IsRangedWeapon)
				{
					return current;
				}
			}
			return false;
		}
	}
}
