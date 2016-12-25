using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_ReactToCloseMeleeThreat : ThinkNode_JobGiver
	{
		private const float HarmForgetDistance = 3f;

		private const int MeleeHarmForgetDelay = 400;

		private const int MaxMeleeChaseTicks = 200;

		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn meleeThreat = pawn.mindState.meleeThreat;
			if (meleeThreat == null)
			{
				return null;
			}
			if (this.IsHunting(pawn, meleeThreat))
			{
				return null;
			}
			if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
			{
				return null;
			}
			if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse && pawn.playerSettings.hostilityResponse != HostilityResponseMode.Attack)
			{
				return null;
			}
			if (meleeThreat.Destroyed || meleeThreat.Downed || Find.TickManager.TicksGame > pawn.mindState.lastMeleeThreatHarmTick + 400 || (pawn.Position - meleeThreat.Position).LengthHorizontalSquared > 9f || !GenSight.LineOfSight(pawn.Position, meleeThreat.Position, false))
			{
				pawn.mindState.meleeThreat = null;
				return null;
			}
			if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				return null;
			}
			return new Job(JobDefOf.AttackMelee, meleeThreat)
			{
				maxNumMeleeAttacks = 1,
				expiryInterval = 200
			};
		}

		private bool IsHunting(Pawn pawn, Pawn prey)
		{
			if (pawn.CurJob == null)
			{
				return false;
			}
			JobDriver_Hunt jobDriver_Hunt = pawn.jobs.curDriver as JobDriver_Hunt;
			if (jobDriver_Hunt != null)
			{
				return jobDriver_Hunt.Victim == prey;
			}
			JobDriver_PredatorHunt jobDriver_PredatorHunt = pawn.jobs.curDriver as JobDriver_PredatorHunt;
			return jobDriver_PredatorHunt != null && jobDriver_PredatorHunt.Prey == prey;
		}
	}
}
