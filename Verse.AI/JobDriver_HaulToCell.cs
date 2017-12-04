using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_HaulToCell : JobDriver
	{
		private bool forbiddenInitially;

		private const TargetIndex HaulableInd = TargetIndex.A;

		private const TargetIndex StoreCellInd = TargetIndex.B;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.forbiddenInitially, "forbiddenInitially", false, false);
		}

		public override string GetReport()
		{
			IntVec3 cell = this.job.targetB.Cell;
			Thing thing;
			if (this.pawn.CurJob == this.job && this.pawn.carryTracker.CarriedThing != null)
			{
				thing = this.pawn.carryTracker.CarriedThing;
			}
			else
			{
				thing = base.TargetThingA;
			}
			if (thing == null)
			{
				return "ReportHaulingUnknown".Translate();
			}
			string text = null;
			SlotGroup slotGroup = cell.GetSlotGroup(base.Map);
			if (slotGroup != null)
			{
				text = slotGroup.parent.SlotYielderLabel();
			}
			if (text != null)
			{
				return "ReportHaulingTo".Translate(new object[]
				{
					thing.LabelCap,
					text
				});
			}
			return "ReportHauling".Translate(new object[]
			{
				thing.LabelCap
			});
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, 1, -1, null) && this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			if (base.TargetThingA != null)
			{
				this.forbiddenInitially = base.TargetThingA.IsForbidden(this.pawn);
			}
			else
			{
				this.forbiddenInitially = false;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.B);
			if (!this.forbiddenInitially)
			{
				this.FailOnForbidden(TargetIndex.A);
			}
			Toil reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return reserveTargetA;
			Toil toilGoto = null;
			toilGoto = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A).FailOn(delegate
			{
				Pawn actor = toilGoto.actor;
				Job curJob = actor.jobs.curJob;
				if (curJob.haulMode == HaulMode.ToCellStorage)
				{
					Thing thing = curJob.GetTarget(TargetIndex.A).Thing;
					IntVec3 cell = actor.jobs.curJob.GetTarget(TargetIndex.B).Cell;
					if (!cell.IsValidStorageFor(this.$this.Map, thing))
					{
						return true;
					}
				}
				return false;
			});
			yield return toilGoto;
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false);
			if (this.job.haulOpportunisticDuplicates)
			{
				yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveTargetA, TargetIndex.A, TargetIndex.B, false, null);
			}
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
		}
	}
}
