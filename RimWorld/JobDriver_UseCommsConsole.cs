using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_UseCommsConsole : JobDriver
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate(Toil to)
			{
				Building_CommsConsole building_CommsConsole = (Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				return !building_CommsConsole.CanUseCommsNow;
			});
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<openComms>__0.actor;
					Building_CommsConsole building_CommsConsole = (Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
					if (building_CommsConsole.CanUseCommsNow)
					{
						actor.jobs.curJob.commTarget.TryOpenComms(actor);
					}
				}
			};
		}
	}
}
