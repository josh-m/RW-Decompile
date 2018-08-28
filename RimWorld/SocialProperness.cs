using System;
using Verse;

namespace RimWorld
{
	public static class SocialProperness
	{
		public static bool IsSociallyProper(this Thing t, Pawn p)
		{
			return t.IsSociallyProper(p, p.IsPrisonerOfColony, false);
		}

		public static bool IsSociallyProper(this Thing t, Pawn p, bool forPrisoner, bool animalsCare = false)
		{
			if (!animalsCare && p != null && !p.RaceProps.Humanlike)
			{
				return true;
			}
			if (!t.def.socialPropernessMatters)
			{
				return true;
			}
			if (!t.Spawned)
			{
				return true;
			}
			IntVec3 intVec = (!t.def.hasInteractionCell) ? t.Position : t.InteractionCell;
			if (forPrisoner)
			{
				return p == null || intVec.GetRoom(t.Map, RegionType.Set_Passable) == p.GetRoom(RegionType.Set_Passable);
			}
			return !intVec.IsInPrisonCell(t.Map);
		}
	}
}
