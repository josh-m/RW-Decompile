using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class StealAIUtility
	{
		private const float MinMarketValueToTake = 320f;

		private const float MinCombatPowerPerPawn = 100f;

		private static readonly FloatRange StealThresholdValuePerCombatPowerRange = new FloatRange(2f, 10f);

		private static List<Thing> tmpToSteal = new List<Thing>();

		public static bool TryFindBestItemToSteal(IntVec3 root, Map map, float maxDist, out Thing item, Pawn thief, List<Thing> disallowed = null)
		{
			if (thief != null && !thief.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				item = null;
				return false;
			}
			if ((thief != null && !map.reachability.CanReachMapEdge(thief.Position, TraverseParms.For(thief, Danger.Some, TraverseMode.ByPawn, false))) || (thief == null && !map.reachability.CanReachMapEdge(root, TraverseParms.For(TraverseMode.PassDoors, Danger.Some, false))))
			{
				item = null;
				return false;
			}
			Predicate<Thing> validator = (Thing t) => (thief == null || thief.CanReserve(t, 1)) && (disallowed == null || !disallowed.Contains(t)) && t.def.stealable && !t.IsBurning();
			item = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(root, map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEverOrMinifiable), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some, false), maxDist, validator, (Thing x) => StealAIUtility.GetValue(x), 15, 15);
			if (item != null && StealAIUtility.GetValue(item) < 320f)
			{
				item = null;
			}
			return item != null;
		}

		public static float TotalMarketValueAround(List<Pawn> pawns)
		{
			float num = 0f;
			StealAIUtility.tmpToSteal.Clear();
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].Spawned)
				{
					Thing thing;
					if (StealAIUtility.TryFindBestItemToSteal(pawns[i].Position, pawns[i].Map, 7f, out thing, pawns[i], StealAIUtility.tmpToSteal))
					{
						num += StealAIUtility.GetValue(thing);
						StealAIUtility.tmpToSteal.Add(thing);
					}
				}
			}
			StealAIUtility.tmpToSteal.Clear();
			return num;
		}

		public static float StartStealingMarketValueThreshold(Lord lord)
		{
			Rand.PushSeed();
			Rand.Seed = lord.loadID;
			float randomInRange = StealAIUtility.StealThresholdValuePerCombatPowerRange.RandomInRange;
			Rand.PopSeed();
			float num = 0f;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				num += Mathf.Max(lord.ownedPawns[i].kindDef.combatPower, 100f);
			}
			return num * randomInRange;
		}

		public static float GetValue(Thing thing)
		{
			return thing.GetInnerIfMinified().MarketValue * (float)thing.stackCount;
		}
	}
}
