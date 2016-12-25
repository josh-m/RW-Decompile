using System;
using UnityEngine;

namespace Verse.AI
{
	public static class Toils_Combat
	{
		public static Toil TrySetJobToUseAttackVerb()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				bool allowManualCastWeapons = !actor.IsColonist;
				Verb verb = actor.TryGetAttackVerb(allowManualCastWeapons);
				if (verb == null)
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					return;
				}
				curJob.verbToUse = verb;
			};
			return toil;
		}

		public static Toil GotoCastPosition(TargetIndex targetInd, bool closeIfDowned = false)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(targetInd).Thing;
				Pawn pawn = thing as Pawn;
				IntVec3 intVec;
				if (!CastPositionFinder.TryFindCastPosition(new CastPositionRequest
				{
					caster = toil.actor,
					target = thing,
					verb = curJob.verbToUse,
					maxRangeFromTarget = ((closeIfDowned && pawn != null && pawn.Downed) ? Mathf.Min(curJob.verbToUse.verbProps.range, (float)pawn.RaceProps.executionRange) : curJob.verbToUse.verbProps.range),
					wantCoverFromTarget = false
				}, out intVec))
				{
					toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					return;
				}
				toil.actor.pather.StartPath(intVec, PathEndMode.OnCell);
				actor.Map.pawnDestinationManager.ReserveDestinationFor(actor, intVec);
			};
			toil.FailOnDespawnedOrNull(targetInd);
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		public static Toil CastVerb(TargetIndex targetInd, bool canFreeIntercept = true)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Verb arg_43_0 = toil.actor.jobs.curJob.verbToUse;
				bool canFreeIntercept2 = canFreeIntercept;
				arg_43_0.TryStartCastOn(toil.actor.jobs.curJob.GetTarget(targetInd), false, canFreeIntercept2);
			};
			toil.defaultCompleteMode = ToilCompleteMode.FinishedBusy;
			return toil;
		}

		public static Toil FollowAndMeleeAttack(TargetIndex targetInd, Action hitAction)
		{
			Toil followAndAttack = new Toil();
			followAndAttack.tickAction = delegate
			{
				Pawn actor = followAndAttack.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver curDriver = actor.jobs.curDriver;
				Thing thing = curJob.GetTarget(targetInd).Thing;
				Pawn pawn = thing as Pawn;
				if (!thing.Spawned)
				{
					curDriver.ReadyForNextToil();
					return;
				}
				if (thing != actor.pather.Destination.Thing || (!actor.pather.Moving && !actor.Position.AdjacentTo8WayOrInside(thing)))
				{
					actor.pather.StartPath(thing, PathEndMode.Touch);
				}
				else if (actor.Position.AdjacentTo8WayOrInside(thing))
				{
					if (pawn != null && pawn.Downed && !curJob.killIncappedTarget)
					{
						curDriver.ReadyForNextToil();
						return;
					}
					hitAction();
				}
			};
			followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
			return followAndAttack;
		}
	}
}
