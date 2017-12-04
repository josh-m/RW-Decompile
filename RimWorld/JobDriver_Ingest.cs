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
		private bool usingNutrientPasteDispenser;

		private bool eatingFromInventory;

		public const float EatCorpseBodyPartsUntilFoodLevelPct = 0.9f;

		public const TargetIndex IngestibleSourceInd = TargetIndex.A;

		private const TargetIndex TableCellInd = TargetIndex.B;

		private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

		private Thing IngestibleSource
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		private float ChewDurationMultiplier
		{
			get
			{
				Thing ingestibleSource = this.IngestibleSource;
				if (ingestibleSource.def.ingestible != null && !ingestibleSource.def.ingestible.useEatingSpeedStat)
				{
					return 1f;
				}
				return 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true);
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
			if (this.usingNutrientPasteDispenser)
			{
				return this.job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label);
			}
			Thing thing = this.job.targetA.Thing;
			if (thing != null && thing.def.ingestible != null)
			{
				if (!thing.def.ingestible.ingestReportStringEat.NullOrEmpty() && (thing.def.ingestible.ingestReportString.NullOrEmpty() || this.pawn.RaceProps.intelligence < Intelligence.ToolUser))
				{
					return string.Format(thing.def.ingestible.ingestReportStringEat, this.job.targetA.Thing.LabelShort);
				}
				if (!thing.def.ingestible.ingestReportString.NullOrEmpty())
				{
					return string.Format(thing.def.ingestible.ingestReportString, this.job.targetA.Thing.LabelShort);
				}
			}
			return base.GetReport();
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			this.usingNutrientPasteDispenser = (this.IngestibleSource is Building_NutrientPasteDispenser);
			this.eatingFromInventory = (this.pawn.inventory != null && this.pawn.inventory.Contains(this.IngestibleSource));
		}

		public override bool TryMakePreToilReservations()
		{
			if (this.pawn.Faction != null && !(this.IngestibleSource is Building_NutrientPasteDispenser))
			{
				Thing ingestibleSource = this.IngestibleSource;
				int num = FoodUtility.WillIngestStackCountOf(this.pawn, ingestibleSource.def);
				if (num >= ingestibleSource.stackCount && ingestibleSource.Spawned && !this.pawn.Reserve(ingestibleSource, this.job, 1, -1, null))
				{
					return false;
				}
			}
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!this.usingNutrientPasteDispenser)
			{
				this.FailOn(() => !this.$this.IngestibleSource.Destroyed && !this.$this.IngestibleSource.IngestibleNow);
			}
			Toil chew = Toils_Ingest.ChewIngestible(this.pawn, this.ChewDurationMultiplier, TargetIndex.A, TargetIndex.B).FailOn((Toil x) => !this.$this.IngestibleSource.Spawned && (this.$this.pawn.carryTracker == null || this.$this.pawn.carryTracker.CarriedThing != this.$this.IngestibleSource)).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			foreach (Toil toil in this.PrepareToIngestToils(chew))
			{
				yield return toil;
			}
			yield return chew;
			yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A);
			yield return Toils_Jump.JumpIf(chew, () => this.$this.job.GetTarget(TargetIndex.A).Thing is Corpse && this.$this.pawn.needs.food.CurLevelPercentage < 0.9f);
		}

		private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
		{
			if (this.usingNutrientPasteDispenser)
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
			if (this.eatingFromInventory)
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
			}
			else
			{
				yield return this.ReserveFoodIfWillIngestWholeStack();
				Toil gotoToPickup = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
				yield return Toils_Jump.JumpIf(gotoToPickup, () => this.$this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
				yield return Toils_Jump.Jump(chewToil);
				yield return gotoToPickup;
				yield return Toils_Ingest.PickupIngestible(TargetIndex.A, this.pawn);
				Toil reserveExtraFoodToCollect = Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);
				Toil findExtraFoodToCollect = new Toil();
				findExtraFoodToCollect.initAction = delegate
				{
					if (this.$this.pawn.inventory.innerContainer.TotalStackCountOfDef(this.$this.IngestibleSource.def) < this.$this.job.takeExtraIngestibles)
					{
						Thing thing = GenClosest.ClosestThingReachable(this.$this.pawn.Position, this.$this.pawn.Map, ThingRequest.ForDef(this.$this.IngestibleSource.def), PathEndMode.Touch, TraverseParms.For(this.$this.pawn, Danger.Deadly, TraverseMode.ByPawn, false), 12f, (Thing x) => this.$this.pawn.CanReserve(x, 1, -1, null, false) && !x.IsForbidden(this.$this.pawn) && x.IsSociallyProper(this.$this.pawn), null, 0, -1, false, RegionType.Set_Passable, false);
						if (thing != null)
						{
							this.$this.job.SetTarget(TargetIndex.C, thing);
							this.$this.JumpToToil(reserveExtraFoodToCollect);
						}
					}
				};
				findExtraFoodToCollect.defaultCompleteMode = ToilCompleteMode.Instant;
				yield return Toils_Jump.Jump(findExtraFoodToCollect);
				yield return reserveExtraFoodToCollect;
				yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
				yield return Toils_Haul.TakeToInventory(TargetIndex.C, () => this.$this.job.takeExtraIngestibles - this.$this.pawn.inventory.innerContainer.TotalStackCountOfDef(this.$this.IngestibleSource.def));
				yield return findExtraFoodToCollect;
			}
			yield return Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, TargetIndex.A).FailOnDestroyedOrNull(TargetIndex.A);
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
					Thing thing = this.job.GetTarget(TargetIndex.A).Thing;
					if (this.pawn.carryTracker.CarriedThing == thing)
					{
						return;
					}
					int num = FoodUtility.WillIngestStackCountOf(this.pawn, thing.def);
					if (num >= thing.stackCount)
					{
						if (!thing.Spawned)
						{
							this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
							return;
						}
						this.pawn.Reserve(thing, this.job, 1, -1, null);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			IntVec3 cell = this.job.GetTarget(TargetIndex.B).Cell;
			return JobDriver_Ingest.ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, cell, this.pawn);
		}

		public static bool ModifyCarriedThingDrawPosWorker(ref Vector3 drawPos, ref bool behind, ref bool flip, IntVec3 placeCell, Pawn pawn)
		{
			if (pawn.pather.Moving)
			{
				return false;
			}
			Thing carriedThing = pawn.carryTracker.CarriedThing;
			if (carriedThing == null || !carriedThing.IngestibleNow)
			{
				return false;
			}
			if (placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && placeCell.HasEatSurface(pawn.Map) && carriedThing.def.ingestible.ingestHoldUsesTable)
			{
				drawPos = new Vector3((float)placeCell.x + 0.5f, drawPos.y, (float)placeCell.z + 0.5f);
				return true;
			}
			if (carriedThing.def.ingestible.ingestHoldOffsetStanding != null)
			{
				HoldOffset holdOffset = carriedThing.def.ingestible.ingestHoldOffsetStanding.Pick(pawn.Rotation);
				if (holdOffset != null)
				{
					drawPos += holdOffset.offset;
					behind = holdOffset.behind;
					flip = holdOffset.flip;
					return true;
				}
			}
			return false;
		}
	}
}
