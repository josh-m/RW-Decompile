using System;
using Verse;

namespace RimWorld
{
	public class Alert_BrawlerHasRangedWeapon : Alert_Medium
	{
		public override AlertReport Report
		{
			get
			{
				foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
				{
					if (current.story.traits.HasTrait(TraitDefOf.Brawler) && current.equipment.Primary != null && current.equipment.Primary.def.IsRangedWeapon)
					{
						return current;
					}
				}
				return false;
			}
		}

		public Alert_BrawlerHasRangedWeapon()
		{
			this.baseLabel = "BrawlerHasRangedWeapon".Translate();
			this.baseExplanation = "BrawlerHasRangedWeaponDesc".Translate();
		}
	}
}
