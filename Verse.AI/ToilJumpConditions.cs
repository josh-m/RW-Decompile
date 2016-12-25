using System;

namespace Verse.AI
{
	public static class ToilJumpConditions
	{
		public static Toil JumpIf(this Toil toil, Func<bool> jumpCondition, Toil jumpToil)
		{
			toil.AddPreTickAction(delegate
			{
				if (jumpCondition())
				{
					toil.actor.jobs.curDriver.SetNextToil(jumpToil);
					toil.actor.jobs.curDriver.ReadyForNextToil();
					return;
				}
			});
			return toil;
		}

		public static Toil JumpIfDespawnedOrNull(this Toil toil, TargetIndex ind, Toil jumpToil)
		{
			return toil.JumpIf(delegate
			{
				Thing thing = toil.actor.jobs.curJob.GetTarget(ind).Thing;
				return thing == null || !thing.Spawned;
			}, jumpToil);
		}
	}
}
