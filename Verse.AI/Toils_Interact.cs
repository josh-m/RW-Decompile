using System;

namespace Verse.AI
{
	internal class Toils_Interact
	{
		public static Toil DestroyThing(TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Thing thing = actor.jobs.curJob.GetTarget(ind).Thing;
				if (!thing.Destroyed)
				{
					thing.Destroy(DestroyMode.Vanish);
				}
			};
			return toil;
		}
	}
}
