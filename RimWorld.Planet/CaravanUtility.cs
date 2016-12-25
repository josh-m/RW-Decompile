using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanUtility
	{
		public static bool IsOwner(Pawn pawn, Faction caravanFaction)
		{
			if (caravanFaction == null)
			{
				Log.Warning("Called IsOwner with null faction.");
				return false;
			}
			return pawn.RaceProps.Humanlike && pawn.Faction == caravanFaction && pawn.HostFaction == null;
		}

		public static Caravan GetCaravan(this Pawn pawn)
		{
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				if (caravans[i].ContainsPawn(pawn))
				{
					return caravans[i];
				}
			}
			return null;
		}

		public static bool IsCaravanMember(this Pawn pawn)
		{
			return pawn.GetCaravan() != null;
		}

		public static bool IsPlayerControlledCaravanMember(this Pawn pawn)
		{
			Caravan caravan = pawn.GetCaravan();
			return caravan != null && caravan.IsPlayerControlled;
		}

		public static int BestGotoDestNear(int tile, Caravan c)
		{
			Predicate<int> predicate = (int t) => !Find.World.Impassable(t) && c.CanReach(t);
			if (predicate(tile))
			{
				return tile;
			}
			int result;
			GenWorldClosest.TryFindClosestTile(tile, predicate, out result, 50, true);
			return result;
		}

		public static bool PlayerHasAnyCaravan()
		{
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				if (caravans[i].IsPlayerControlled)
				{
					return true;
				}
			}
			return false;
		}
	}
}
