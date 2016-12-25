using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Train : WorkGiver_InteractAnimal
	{
		public const int MinTrainInterval = 15000;

		[DebuggerHidden]
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			List<Pawn> pawnList = Find.MapPawns.SpawnedPawnsInFaction(pawn.Faction);
			for (int i = 0; i < pawnList.Count; i++)
			{
				yield return pawnList[i];
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || !pawn2.RaceProps.Animal)
			{
				return null;
			}
			if (pawn2.Faction != pawn.Faction)
			{
				return null;
			}
			if (Find.TickManager.TicksGame < pawn2.mindState.lastAssignedInteractTime + 15000)
			{
				JobFailReason.Is(WorkGiver_InteractAnimal.AnimalInteractedTooRecentlyTrans);
				return null;
			}
			if (pawn2.training == null)
			{
				return null;
			}
			if (pawn2.training.NextTrainableToTrain() == null)
			{
				return null;
			}
			if (!this.CanInteractWithAnimal(pawn, pawn2))
			{
				return null;
			}
			if (pawn2.RaceProps.EatsFood && !base.HasFoodToInteractAnimal(pawn, pawn2))
			{
				Job job = base.TakeFoodForAnimalInteractJob(pawn, pawn2);
				if (job == null)
				{
					JobFailReason.Is(WorkGiver_InteractAnimal.NoUsableFoodTrans);
				}
				return job;
			}
			return new Job(JobDefOf.Train, t);
		}
	}
}
