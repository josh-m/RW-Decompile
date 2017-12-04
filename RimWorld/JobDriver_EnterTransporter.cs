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
				Thing thing = this.job.GetTarget(this.TransporterInd).Thing;
				if (thing == null)
				{
					return null;
				}
				return thing.TryGetComp<CompTransporter>();
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.GetTarget(this.TransporterInd), this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(this.TransporterInd);
			this.FailOn(() => !this.$this.Transporter.LoadingInProgressOrReadyToLaunch);
			yield return Toils_Goto.GotoThing(this.TransporterInd, PathEndMode.Touch);
			yield return new Toil
			{
				initAction = delegate
				{
					CompTransporter transporter = this.$this.Transporter;
					this.$this.pawn.DeSpawn();
					transporter.GetDirectlyHeldThings().TryAdd(this.$this.pawn, true);
				}
			};
		}
	}
}
