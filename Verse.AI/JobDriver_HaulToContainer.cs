using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_HaulToContainer : JobDriver
	{
		private const TargetIndex CarryThingIndex = TargetIndex.A;

		private const TargetIndex DestIndex = TargetIndex.B;

		private const TargetIndex PrimaryDestIndex = TargetIndex.C;

		private Thing Container
		{
			get
			{
				return (Thing)this.job.GetTarget(TargetIndex.B);
			}
		}

		public override string GetReport()
		{
			Thing thing;
			if (this.pawn.CurJob == this.job && this.pawn.carryTracker.CarriedThing != null)
			{
				thing = this.pawn.carryTracker.CarriedThing;
			}
			else
			{
				thing = base.TargetThingA;
			}
			if (thing == null || !this.job.targetB.HasThing)
			{
				return "ReportHaulingUnknown".Translate();
			}
			return "ReportHaulingTo".Translate(new object[]
			{
				thing.LabelCap,
				this.job.targetB.Thing.LabelShort
			});
		}

		public override bool TryMakePreToilReservations()
		{
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 1, -1, null);
			return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null) && this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			this.FailOn(() => TransporterUtility.WasLoadingCanceled(this.$this.Container));
			Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return getToHaulTarget;
			yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false);
			yield return Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(getToHaulTarget, TargetIndex.A);
			Toil carryToContainer = Toils_Haul.CarryHauledThingToContainer();
			yield return carryToContainer;
			yield return Toils_Goto.MoveOffTargetBlueprint(TargetIndex.B);
			yield return Toils_Construct.MakeSolidThingFromBlueprintIfNecessary(TargetIndex.B, TargetIndex.C);
			yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.C);
			yield return Toils_Haul.JumpToCarryToNextContainerIfPossible(carryToContainer, TargetIndex.C);
		}
	}
}
