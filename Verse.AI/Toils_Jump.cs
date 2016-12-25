using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public static class Toils_Jump
	{
		public static Toil Jump(Toil jumpTarget)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.jobs.curDriver.JumpToToil(jumpTarget);
			};
			return toil;
		}

		public static Toil JumpIf(Toil jumpTarget, Func<bool> condition)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (condition())
				{
					toil.actor.jobs.curDriver.JumpToToil(jumpTarget);
				}
			};
			return toil;
		}

		public static Toil JumpIfTargetDespawnedOrNull(TargetIndex ind, Toil jumpToil)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Thing thing = toil.actor.jobs.curJob.GetTarget(ind).Thing;
				if (thing == null || !thing.Spawned)
				{
					toil.actor.jobs.curDriver.JumpToToil(jumpToil);
				}
			};
			return toil;
		}

		public static Toil JumpIfTargetNotHittable(TargetIndex ind, Toil jumpToil)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				LocalTargetInfo target = curJob.GetTarget(ind);
				if (!curJob.verbToUse.CanHitTarget(target))
				{
					actor.jobs.curDriver.JumpToToil(jumpToil);
				}
			};
			return toil;
		}

		public static Toil JumpIfTargetDownedDistant(TargetIndex ind, Toil jumpToil)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Pawn pawn = curJob.GetTarget(ind).Thing as Pawn;
				int executionRange = pawn.RaceProps.executionRange;
				if (pawn != null && pawn.Downed && (actor.Position - pawn.Position).LengthHorizontalSquared > (float)(executionRange * executionRange))
				{
					actor.jobs.curDriver.JumpToToil(jumpToil);
				}
			};
			return toil;
		}

		public static Toil JumpIfHaveTargetInQueue(TargetIndex ind, Toil jumpToil)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
				if (!targetQueue.NullOrEmpty<LocalTargetInfo>())
				{
					actor.jobs.curDriver.JumpToToil(jumpToil);
				}
			};
			return toil;
		}
	}
}
