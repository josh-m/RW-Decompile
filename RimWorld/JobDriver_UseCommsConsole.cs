using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_UseCommsConsole : JobDriver
	{
		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate(Toil to)
			{
				Building_CommsConsole building_CommsConsole = (Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				return !building_CommsConsole.CanUseCommsNow;
			});
			Toil openComms = new Toil();
			openComms.initAction = delegate
			{
				Pawn actor = openComms.actor;
				Building_CommsConsole building_CommsConsole = (Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				if (building_CommsConsole.CanUseCommsNow)
				{
					actor.jobs.curJob.commTarget.TryOpenComms(actor);
				}
			};
			yield return openComms;
		}
	}
}
