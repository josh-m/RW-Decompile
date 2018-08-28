using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class GiveToPackAnimalUtility
	{
		public static IEnumerable<Pawn> CarrierCandidatesFor(Pawn pawn)
		{
			IEnumerable<Pawn> enumerable = (!pawn.IsFormingCaravan()) ? pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction) : pawn.GetLord().ownedPawns;
			enumerable = from x in enumerable
			where x.RaceProps.packAnimal && !x.inventory.UnloadEverything
			select x;
			if (pawn.Map.IsPlayerHome)
			{
				enumerable = enumerable.Where(new Func<Pawn, bool>(CaravanFormingUtility.IsFormingCaravan));
			}
			return enumerable;
		}

		public static Pawn UsablePackAnimalWithTheMostFreeSpace(Pawn pawn)
		{
			IEnumerable<Pawn> enumerable = GiveToPackAnimalUtility.CarrierCandidatesFor(pawn);
			Pawn pawn2 = null;
			float num = 0f;
			foreach (Pawn current in enumerable)
			{
				if (current.RaceProps.packAnimal && current != pawn && pawn.CanReach(current, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					float num2 = MassUtility.FreeSpace(current);
					if (pawn2 == null || num2 > num)
					{
						pawn2 = current;
						num = num2;
					}
				}
			}
			return pawn2;
		}
	}
}
