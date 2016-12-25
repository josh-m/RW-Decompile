using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Ingest : JobDriver
	{
		public const float EatCorpseBodyPartsUntilFoodLevelPct = 0.9f;

		private const TargetIndex IngestibleSourceInd = TargetIndex.A;

		private const TargetIndex TableCellInd = TargetIndex.B;

		private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

		private Thing IngestibleSource
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		private bool UsingNutrientPasteDispenser
		{
			get
			{
				return this.IngestibleSource is Building_NutrientPasteDispenser;
			}
		}

		public override string GetReport()
		{
			if (this.UsingNutrientPasteDispenser)
			{
				return base.CurJob.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label);
			}
			Thing thing = this.pawn.CurJob.targetA.Thing;
			if (thing != null && thing.def.ingestible != null && !thing.def.ingestible.ingestReportString.NullOrEmpty())
			{
				return string.Format(thing.def.ingestible.ingestReportString, this.pawn.CurJob.targetA.Thing.LabelShort);
			}
			return base.GetReport();
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!this.UsingNutrientPasteDispenser)
			{
				this.FailOn(() => !this.<>f__this.IngestibleSource.IngestibleNow);
			}
			float durationMultiplier = 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true);
			Toil chew = Toils_Ingest.ChewIngestible(this.pawn, durationMultiplier, TargetIndex.A, TargetIndex.B).FailOn((Toil x) => !this.<>f__this.IngestibleSource.Spawned && (this.<>f__this.pawn.carrier == null || this.<>f__this.pawn.carrier.CarriedThing != this.<>f__this.IngestibleSource));
			foreach (Toil toil in this.PrepareToIngestToils(chew))
			{
				yield return toil;
			}
			yield return chew;
			yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A);
			yield return Toils_Jump.JumpIf(chew, () => this.<>f__this.CurJob.GetTarget(TargetIndex.A).Thing is Corpse && this.<>f__this.pawn.needs.food.CurLevelPercentage < 0.9f);
		}

		private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
		{
			if (this.UsingNutrientPasteDispenser)
			{
				return this.PrepareToIngestToils_Dispenser();
			}
			if (this.pawn.RaceProps.ToolUser)
			{
				return this.PrepareToIngestToils_ToolUser(chewToil);
			}
			return this.PrepareToIngestToils_NonToolUser();
		}

		[DebuggerHidden]
		private IEnumerable<Toil> PrepareToIngestToils_Dispenser()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, this.pawn);
			yield return Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, TargetIndex.A).FailOnDestroyedNullOrForbidden(TargetIndex.A);
			yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
		}

		[DebuggerHidden]
		private IEnumerable<Toil> PrepareToIngestToils_ToolUser(Toil chewToil)
		{
			if (this.pawn.inventory != null && this.pawn.inventory.Contains(this.IngestibleSource))
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
			}
			else
			{
				yield return this.ReserveFoodIfWillIngestWholeStack();
				Toil gotoToPickup = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
				yield return Toils_Jump.JumpIf(gotoToPickup, () => this.<>f__this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
				yield return Toils_Jump.Jump(chewToil);
				yield return gotoToPickup;
				yield return Toils_Ingest.PickupIngestible(TargetIndex.A, this.pawn);
				Toil reserveExtraFoodToCollect = Toils_Reserve.Reserve(TargetIndex.C, 1);
				Toil findExtraFoodToCollect = new Toil();
				findExtraFoodToCollect.initAction = delegate
				{
					if (this.<>f__this.pawn.inventory.container.TotalStackCountOfDef(this.<>f__this.IngestibleSource.def) < this.<>f__this.CurJob.takeExtraIngestibles)
					{
						Predicate<Thing> validator = (Thing x) => this.<>f__this.pawn.CanReserve(x, 1) && !x.IsForbidden(this.<>f__this.pawn) && x.IsSociallyProper(this.<>f__this.pawn);
						Thing thing = GenClosest.ClosestThingReachable(this.<>f__this.pawn.Position, ThingRequest.ForDef(this.<>f__this.IngestibleSource.def), PathEndMode.Touch, TraverseParms.For(this.<>f__this.pawn, Danger.Deadly, TraverseMode.ByPawn, false), 12f, validator, null, -1, false);
						if (thing != null)
						{
							this.<>f__this.pawn.CurJob.SetTarget(TargetIndex.C, thing);
							this.<>f__this.JumpToToil(this.<reserveExtraFoodToCollect>__1);
						}
					}
				};
				findExtraFoodToCollect.defaultCompleteMode = ToilCompleteMode.Instant;
				yield return Toils_Jump.Jump(findExtraFoodToCollect);
				yield return reserveExtraFoodToCollect;
				yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
				yield return Toils_Haul.TakeToInventory(TargetIndex.C, () => this.<>f__this.CurJob.takeExtraIngestibles - this.<>f__this.pawn.inventory.container.TotalStackCountOfDef(this.<>f__this.IngestibleSource.def));
				yield return findExtraFoodToCollect;
			}
			yield return Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, TargetIndex.A).FailOnDestroyedNullOrForbidden(TargetIndex.A);
			yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
		}

		[DebuggerHidden]
		private IEnumerable<Toil> PrepareToIngestToils_NonToolUser()
		{
			yield return this.ReserveFoodIfWillIngestWholeStack();
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		}

		private Toil ReserveFoodIfWillIngestWholeStack()
		{
			return new Toil
			{
				initAction = delegate
				{
					if (this.pawn.Faction == null)
					{
						return;
					}
					Thing thing = this.pawn.CurJob.GetTarget(TargetIndex.A).Thing;
					int num = FoodUtility.WillIngestStackCountOf(this.pawn, thing.def);
					if (num >= thing.stackCount)
					{
						if (!thing.Spawned || !Find.Reservations.CanReserve(this.pawn, thing, 1))
						{
							this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
							return;
						}
						Find.Reservations.Reserve(this.pawn, thing, 1);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos)
		{
			IntVec3 cell = base.CurJob.GetTarget(TargetIndex.B).Cell;
			return JobDriver_Ingest.ModifyCarriedThingDrawPosWorker(ref drawPos, cell, this.pawn);
		}

		public static bool ModifyCarriedThingDrawPosWorker(ref Vector3 drawPos, IntVec3 placeCell, Pawn pawn)
		{
			if (pawn.pather.Moving)
			{
				return false;
			}
			Thing carriedThing = pawn.carrier.CarriedThing;
			if (carriedThing == null || !carriedThing.IngestibleNow)
			{
				return false;
			}
			if (placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && placeCell.HasEatSurface() && carriedThing.def.ingestible.ingestHoldUsesTable)
			{
				drawPos = new Vector3((float)placeCell.x + 0.5f, drawPos.y, (float)placeCell.z + 0.5f);
				return true;
			}
			if (carriedThing.def.ingestible.ingestHoldOffsetStanding.x > -500f)
			{
				drawPos += carriedThing.def.ingestible.ingestHoldOffsetStanding;
				return true;
			}
			return false;
		}
	}
}
