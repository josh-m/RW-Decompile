using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld.Planet
{
	public static class CaravanFormingUtility
	{
		private static List<Thing> tmpReachableItems = new List<Thing>();

		private static List<Pawn> tmpSendablePawns = new List<Pawn>();

		public static void FormAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, int startingTile)
		{
			Caravan o = CaravanExitMapUtility.ExitMapAndCreateCaravan(pawns, faction, startingTile);
			Find.LetterStack.ReceiveLetter("LetterLabelFormedCaravan".Translate(), "LetterFormedCaravan".Translate(), LetterType.Good, o, null);
		}

		public static void StartFormingCaravan(List<Pawn> pawns, Faction faction, List<TransferableOneWay> transferables, IntVec3 meetingPoint, IntVec3 exitSpot, int startingTile)
		{
			if (startingTile < 0)
			{
				Log.Error("Can't start forming caravan because startingTile is invalid.");
				return;
			}
			if (!pawns.Any<Pawn>())
			{
				Log.Error("Can't start forming caravan with 0 pawns.");
				return;
			}
			if (pawns.Any((Pawn x) => x.Downed))
			{
				Log.Warning("Forming a caravan with a downed pawn. This shouldn't happen because we have to create a Lord.");
			}
			List<TransferableOneWay> list = transferables.ToList<TransferableOneWay>();
			list.RemoveAll((TransferableOneWay x) => x.CountToTransfer <= 0 || !x.HasAnyThing || x.AnyThing is Pawn);
			LordJob_FormAndSendCaravan lordJob = new LordJob_FormAndSendCaravan(list, meetingPoint, exitSpot, startingTile);
			LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, pawns[0].Map, pawns);
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (pawn.Spawned)
				{
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
				}
			}
		}

		public static void StopFormingCaravan(Lord lord)
		{
			lord.lordManager.RemoveLord(lord);
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].jobs.EndCurrentJob(JobCondition.InterruptForced, true);
			}
		}

		public static List<Thing> AllReachableColonyItems(Map map, bool allowEvenIfOutsideHomeArea = false, bool allowEvenIfReserved = false)
		{
			CaravanFormingUtility.tmpReachableItems.Clear();
			List<Thing> allThings = map.listerThings.AllThings;
			for (int i = 0; i < allThings.Count; i++)
			{
				Thing thing = allThings[i];
				if (thing.def.category == ThingCategory.Item && (allowEvenIfOutsideHomeArea || map.areaManager.Home[thing.Position] || thing.IsInAnyStorage()) && (!thing.Position.Fogged(thing.Map) && (allowEvenIfReserved || !map.reservationManager.IsReserved(thing, Faction.OfPlayer))) && thing.def.EverHaulable)
				{
					CaravanFormingUtility.tmpReachableItems.Add(thing);
				}
			}
			return CaravanFormingUtility.tmpReachableItems;
		}

		public static List<Pawn> AllSendablePawns(Map map, bool allowEvenIfDownedOrInMentalState = false, bool allowEvenIfPrisonerNotSecure = false)
		{
			CaravanFormingUtility.tmpSendablePawns.Clear();
			List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Pawn pawn = allPawnsSpawned[i];
				if ((allowEvenIfDownedOrInMentalState || !pawn.Downed) && (allowEvenIfDownedOrInMentalState || !pawn.InMentalState) && (!pawn.HostileTo(Faction.OfPlayer) && (pawn.Faction == Faction.OfPlayer || pawn.IsPrisonerOfColony)) && (allowEvenIfPrisonerNotSecure || !pawn.IsPrisoner || pawn.guest.PrisonerIsSecure) && (pawn.GetLord() == null || pawn.GetLord().LordJob is LordJob_VoluntarilyJoinable))
				{
					CaravanFormingUtility.tmpSendablePawns.Add(pawn);
				}
			}
			return CaravanFormingUtility.tmpSendablePawns;
		}
	}
}
