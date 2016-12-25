using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_EnterTransporter : JobDriver
	{
		private TargetIndex TransporterInd = TargetIndex.A;

		private CompTransporter Transporter
		{
			get
			{
				Thing thing = base.CurJob.GetTarget(this.TransporterInd).Thing;
				if (thing == null)
				{
					return null;
				}
				return thing.TryGetComp<CompTransporter>();
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(this.TransporterInd);
			yield return Toils_Reserve.Reserve(this.TransporterInd, 1);
			yield return Toils_Goto.GotoThing(this.TransporterInd, PathEndMode.Touch);
			yield return new Toil
			{
				initAction = delegate
				{
					CompTransporter transporter = this.<>f__this.Transporter;
					transporter.GetInnerContainer().TryAdd(this.<>f__this.pawn, true);
					transporter.Notify_PawnEnteredTransporterOnHisOwn(this.<>f__this.pawn);
				}
			};
		}
	}
}
