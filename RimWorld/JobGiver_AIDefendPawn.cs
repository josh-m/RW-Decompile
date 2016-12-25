using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobGiver_AIDefendPawn : JobGiver_AIFightEnemy
	{
		private bool attackMeleeThreatEvenIfNotHostile;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_AIDefendPawn jobGiver_AIDefendPawn = (JobGiver_AIDefendPawn)base.DeepCopy(resolve);
			jobGiver_AIDefendPawn.attackMeleeThreatEvenIfNotHostile = this.attackMeleeThreatEvenIfNotHostile;
			return jobGiver_AIDefendPawn;
		}

		protected abstract Pawn GetDefendee(Pawn pawn);

		protected override IntVec3 GetFlagPosition(Pawn pawn)
		{
			Pawn defendee = this.GetDefendee(pawn);
			if (defendee.Spawned || defendee.CarriedBy != null)
			{
				return defendee.PositionHeld;
			}
			return IntVec3.Invalid;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn defendee = this.GetDefendee(pawn);
			Pawn carriedBy = defendee.CarriedBy;
			if (carriedBy != null)
			{
				if (!pawn.CanReach(carriedBy, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					return null;
				}
			}
			else if (!defendee.Spawned || !pawn.CanReach(defendee, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
			{
				return null;
			}
			return base.TryGiveJob(pawn);
		}

		protected override Thing FindAttackTarget(Pawn pawn)
		{
			if (this.attackMeleeThreatEvenIfNotHostile)
			{
				Pawn defendee = this.GetDefendee(pawn);
				if (defendee.Spawned && !defendee.InMentalState && defendee.mindState.meleeThreat != null && defendee.mindState.meleeThreat != pawn && pawn.CanReach(defendee.mindState.meleeThreat, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					return defendee.mindState.meleeThreat;
				}
			}
			return base.FindAttackTarget(pawn);
		}

		protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
		{
			Verb verb = pawn.TryGetAttackVerb(!pawn.IsColonist);
			if (verb == null)
			{
				dest = IntVec3.Invalid;
				return false;
			}
			return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
			{
				caster = pawn,
				target = pawn.mindState.enemyTarget,
				verb = verb,
				maxRangeFromTarget = 9999f,
				locus = this.GetDefendee(pawn).PositionHeld,
				maxRangeFromLocus = this.GetFlagRadius(pawn),
				wantCoverFromTarget = (verb.verbProps.range > 7f),
				maxRegionsRadius = 50
			}, out dest);
		}
	}
}
