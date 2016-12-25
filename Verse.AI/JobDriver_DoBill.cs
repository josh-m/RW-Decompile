using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse.AI
{
	public class JobDriver_DoBill : JobDriver
	{
		public const TargetIndex BillGiverInd = TargetIndex.A;

		public const TargetIndex IngredientInd = TargetIndex.B;

		public const TargetIndex IngredientPlaceCellInd = TargetIndex.C;

		public float workLeft;

		public int billStartTick;

		public int ticksSpentDoingRecipeWork;

		public IBillGiver BillGiver
		{
			get
			{
				IBillGiver billGiver = this.pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing as IBillGiver;
				if (billGiver == null)
				{
					throw new InvalidOperationException("DoBill on non-Billgiver.");
				}
				return billGiver;
			}
		}

		public override string GetReport()
		{
			if (this.pawn.jobs.curJob.RecipeDef != null)
			{
				return base.ReportStringProcessed(this.pawn.jobs.curJob.RecipeDef.jobString);
			}
			return base.GetReport();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<float>(ref this.workLeft, "workLeft", 0f, false);
			Scribe_Values.LookValue<int>(ref this.billStartTick, "billStartTick", 0, false);
			Scribe_Values.LookValue<int>(ref this.ticksSpentDoingRecipeWork, "ticksSpentDoingRecipeWork", 0, false);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.AddEndCondition(delegate
			{
				Thing thing = this.<>f__this.GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
				if (thing is Building && !thing.Spawned)
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOn(delegate
			{
				IBillGiver billGiver = this.<>f__this.pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing as IBillGiver;
				if (billGiver != null)
				{
					if (this.<>f__this.pawn.jobs.curJob.bill.DeletedOrDereferenced)
					{
						return true;
					}
					if (!billGiver.CurrentlyUsable())
					{
						return true;
					}
				}
				return false;
			});
			Toil gotoBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Reserve.ReserveQueue(TargetIndex.B, 1);
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.<>f__this.CurJob.targetQueueB != null && this.<>f__this.CurJob.targetQueueB.Count == 1)
					{
						UnfinishedThing unfinishedThing = this.<>f__this.CurJob.targetQueueB[0].Thing as UnfinishedThing;
						if (unfinishedThing != null)
						{
							unfinishedThing.BoundBill = (Bill_ProductionWithUft)this.<>f__this.CurJob.bill;
						}
					}
				}
			};
			yield return Toils_Jump.JumpIf(gotoBillGiver, () => this.<>f__this.CurJob.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());
			Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
			yield return extract;
			Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return getToHaulTarget;
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, true, false);
			yield return JobDriver_DoBill.JumpToCollectNextIntoHandsForBill(getToHaulTarget, TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDestroyedOrNull(TargetIndex.B);
			Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
			yield return findPlaceTarget;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, findPlaceTarget, false);
			yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, extract);
			yield return gotoBillGiver;
			yield return Toils_Recipe.MakeUnfinishedThingIfNeeded();
			yield return Toils_Recipe.DoRecipeWork().FailOnDespawnedOrForbiddenPlacedThings();
			yield return Toils_Recipe.FinishRecipeAndStartStoringProduct();
			if (!base.CurJob.RecipeDef.products.NullOrEmpty<ThingCountClass>() || !base.CurJob.RecipeDef.specialProducts.NullOrEmpty<SpecialProductType>())
			{
				yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
				Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
				yield return carryToCell;
				yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
				yield return new Toil
				{
					initAction = delegate
					{
						Bill_Production bill_Production = this.<recount>__6.actor.jobs.curJob.bill as Bill_Production;
						if (bill_Production != null && bill_Production.repeatMode == BillRepeatMode.TargetCount)
						{
							this.<>f__this.Map.resourceCounter.UpdateResourceCounts();
						}
					}
				};
			}
		}

		private static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor + " who is not carrying something.");
					return;
				}
				if (actor.carryTracker.Full)
				{
					return;
				}
				Job curJob = actor.jobs.curJob;
				List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
				if (targetQueue.NullOrEmpty<LocalTargetInfo>())
				{
					return;
				}
				for (int i = 0; i < targetQueue.Count; i++)
				{
					if (GenAI.CanUseItemForWork(actor, targetQueue[i].Thing))
					{
						if (targetQueue[i].Thing.CanStackWith(actor.carryTracker.CarriedThing))
						{
							if ((actor.Position - targetQueue[i].Thing.Position).LengthHorizontalSquared <= 64f)
							{
								int num = (actor.carryTracker.CarriedThing != null) ? actor.carryTracker.CarriedThing.stackCount : 0;
								int num2 = curJob.countQueue[i];
								num2 = Mathf.Min(num2, targetQueue[i].Thing.def.stackLimit - num);
								num2 = Mathf.Min(num2, actor.carryTracker.AvailableStackSpace(targetQueue[i].Thing.def));
								if (num2 > 0)
								{
									curJob.count = num2;
									curJob.SetTarget(ind, targetQueue[i].Thing);
									List<int> countQueue;
									List<int> expr_1B2 = countQueue = curJob.countQueue;
									int num3;
									int expr_1B6 = num3 = i;
									num3 = countQueue[num3];
									expr_1B2[expr_1B6] = num3 - num2;
									if (curJob.countQueue[i] == 0)
									{
										curJob.countQueue.RemoveAt(i);
										targetQueue.RemoveAt(i);
									}
									actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
									return;
								}
							}
						}
					}
				}
			};
			return toil;
		}
	}
}
