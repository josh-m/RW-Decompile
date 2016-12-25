using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FoodDeliver : JobDriver
	{
		private const TargetIndex FoodSourceInd = TargetIndex.A;

		private const TargetIndex DelivereeInd = TargetIndex.B;

		private Pawn Deliveree
		{
			get
			{
				return (Pawn)base.CurJob.targetB.Thing;
			}
		}

		public override string GetReport()
		{
			if (base.CurJob.GetTarget(TargetIndex.A).Thing is Building_NutrientPasteDispenser)
			{
				return base.CurJob.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label).Replace("TargetB", ((Pawn)((Thing)base.CurJob.targetB)).LabelShort);
			}
			return base.GetReport();
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			if (this.pawn.inventory != null && this.pawn.inventory.Contains(base.TargetThingA))
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
			}
			else if (base.TargetThingA is Building_NutrientPasteDispenser)
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
				yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, this.pawn);
			}
			else
			{
				yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
				yield return Toils_Ingest.PickupIngestible(TargetIndex.A, this.Deliveree);
			}
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = this.<toil>__0.actor;
				Job curJob = actor.jobs.curJob;
				actor.pather.StartPath(curJob.targetC, PathEndMode.OnCell);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			toil.AddFailCondition(delegate
			{
				Pawn pawn = (Pawn)this.<toil>__0.actor.jobs.curJob.targetB.Thing;
				return pawn.Downed || !pawn.IsPrisonerOfColony || !pawn.guest.ShouldBeBroughtFood;
			});
			yield return toil;
			yield return new Toil
			{
				initAction = delegate
				{
					Thing thing;
					this.<>f__this.pawn.carryTracker.TryDropCarriedThing(this.<toil>__1.actor.jobs.curJob.targetC.Cell, ThingPlaceMode.Direct, out thing, null);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
