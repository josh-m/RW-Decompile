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

		public override string GetReport()
		{
			Thing thing;
			if (this.pawn.carrier.CarriedThing != null)
			{
				thing = this.pawn.carrier.CarriedThing;
			}
			else
			{
				thing = base.TargetThingA;
			}
			return "ReportHaulingTo".Translate(new object[]
			{
				thing.LabelCap,
				base.CurJob.targetB.Thing.LabelShort
			});
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
			this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Reserve.ReserveQueue(TargetIndex.A, 1);
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			yield return Toils_Reserve.ReserveQueue(TargetIndex.B, 1);
			Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return getToHaulTarget;
			yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			yield return Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(getToHaulTarget, TargetIndex.A);
			Toil carryToContainer = Toils_Haul.CarryHauledThingToContainer();
			yield return carryToContainer;
			yield return Toils_Goto.MoveOffTargetBlueprint(TargetIndex.B);
			yield return Toils_Construct.MakeSolidThingFromBlueprintIfNecessary(TargetIndex.B);
			yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B);
			yield return Toils_Haul.JumpToCarryToNextContainerIfPossible(carryToContainer);
		}
	}
}
