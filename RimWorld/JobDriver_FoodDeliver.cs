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

		private bool usingNutrientPasteDispenser;

		private bool eatingFromInventory;

		private Pawn Deliveree
		{
			get
			{
				return (Pawn)base.CurJob.targetB.Thing;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.usingNutrientPasteDispenser, "usingNutrientPasteDispenser", false, false);
			Scribe_Values.Look<bool>(ref this.eatingFromInventory, "eatingFromInventory", false, false);
		}

		public override string GetReport()
		{
			if (base.CurJob.GetTarget(TargetIndex.A).Thing is Building_NutrientPasteDispenser)
			{
				return base.CurJob.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label).Replace("TargetB", ((Pawn)((Thing)base.CurJob.targetB)).LabelShort);
			}
			return base.GetReport();
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			this.usingNutrientPasteDispenser = (base.TargetThingA is Building_NutrientPasteDispenser);
			this.eatingFromInventory = (this.pawn.inventory != null && this.pawn.inventory.Contains(base.TargetThingA));
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			if (this.eatingFromInventory)
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
			}
			else if (this.usingNutrientPasteDispenser)
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
				yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, this.pawn);
			}
			else
			{
				yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
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
				return !pawn.IsPrisonerOfColony || !pawn.guest.CanBeBroughtFood;
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
