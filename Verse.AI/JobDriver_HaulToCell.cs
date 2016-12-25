using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_HaulToCell : JobDriver
	{
		private const TargetIndex HaulableInd = TargetIndex.A;

		private const TargetIndex StoreCellInd = TargetIndex.B;

		public override string GetReport()
		{
			IntVec3 cell = this.pawn.jobs.curJob.targetB.Cell;
			Thing thing;
			if (this.pawn.carryTracker.CarriedThing != null)
			{
				thing = this.pawn.carryTracker.CarriedThing;
			}
			else
			{
				thing = base.TargetThingA;
			}
			string text = null;
			SlotGroup slotGroup = cell.GetSlotGroup(base.Map);
			if (slotGroup != null)
			{
				text = slotGroup.parent.SlotYielderLabel();
			}
			string result;
			if (text != null)
			{
				result = "ReportHaulingTo".Translate(new object[]
				{
					thing.LabelCap,
					text
				});
			}
			else
			{
				result = "ReportHauling".Translate(new object[]
				{
					thing.LabelCap
				});
			}
			return result;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.B);
			if (!base.TargetThingA.IsForbidden(this.pawn))
			{
				this.FailOnForbidden(TargetIndex.A);
			}
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			Toil reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return reserveTargetA;
			Toil toilGoto = null;
			toilGoto = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A).FailOn(delegate
			{
				Pawn actor = this.<toilGoto>__1.actor;
				Job curJob = actor.jobs.curJob;
				if (curJob.haulMode == HaulMode.ToCellStorage)
				{
					Thing thing = curJob.GetTarget(TargetIndex.A).Thing;
					IntVec3 cell = actor.jobs.curJob.GetTarget(TargetIndex.B).Cell;
					if (!cell.IsValidStorageFor(this.<>f__this.Map, thing))
					{
						return true;
					}
				}
				return false;
			});
			yield return toilGoto;
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
			if (base.CurJob.haulOpportunisticDuplicates)
			{
				yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveTargetA, TargetIndex.A, TargetIndex.B, false, null);
			}
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
		}
	}
}
