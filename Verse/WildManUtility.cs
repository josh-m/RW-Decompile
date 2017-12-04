using RimWorld;
using System;

namespace Verse
{
	public static class WildManUtility
	{
		public const float ManhunterOnDamageChance = 0.5f;

		public static bool IsWildMan(this Pawn p)
		{
			return p.kindDef == PawnKindDefOf.WildMan;
		}

		public static bool AnimalOrWildMan(this Pawn p)
		{
			return p.RaceProps.Animal || p.IsWildMan();
		}

		public static bool NonHumanlikeOrWildMan(this Pawn p)
		{
			return !p.RaceProps.Humanlike || p.IsWildMan();
		}
	}
}
