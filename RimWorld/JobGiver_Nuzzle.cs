using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Nuzzle : ThinkNode_JobGiver
	{
		private const float MaxNuzzleDistance = 15f;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.RaceProps.nuzzleMtbHours <= 0f)
			{
				return null;
			}
			List<Pawn> source = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
			Pawn t;
			if (!(from p in source
			where p.RaceProps.Humanlike && p.Position.InHorDistOf(pawn.Position, 15f) && pawn.GetRoom() == p.GetRoom() && !p.Position.IsForbidden(pawn) && p.CanCasuallyInteractNow(false)
			select p).TryRandomElement(out t))
			{
				return null;
			}
			return new Job(JobDefOf.Nuzzle, t)
			{
				locomotionUrgency = LocomotionUrgency.Walk,
				expiryInterval = 3000
			};
		}
	}
}
