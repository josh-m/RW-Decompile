using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_DoBill : WorkGiver_Scanner
	{
		private class DefCountList
		{
			private List<ThingDef> defs = new List<ThingDef>();

			private List<float> counts = new List<float>();

			public int Count
			{
				get
				{
					return this.defs.Count;
				}
			}

			public float this[ThingDef def]
			{
				get
				{
					int num = this.defs.IndexOf(def);
					if (num < 0)
					{
						return 0f;
					}
					return this.counts[num];
				}
				set
				{
					int num = this.defs.IndexOf(def);
					if (num < 0)
					{
						this.defs.Add(def);
						this.counts.Add(value);
						num = this.defs.Count - 1;
					}
					else
					{
						this.counts[num] = value;
					}
					this.CheckRemove(num);
				}
			}

			public float GetCount(int index)
			{
				return this.counts[index];
			}

			public void SetCount(int index, float val)
			{
				this.counts[index] = val;
				this.CheckRemove(index);
			}

			public ThingDef GetDef(int index)
			{
				return this.defs[index];
			}

			private void CheckRemove(int index)
			{
				if (this.counts[index] == 0f)
				{
					this.counts.RemoveAt(index);
					this.defs.RemoveAt(index);
				}
			}

			public void Clear()
			{
				this.defs.Clear();
				this.counts.Clear();
			}

			public void GenerateFrom(List<Thing> things)
			{
				this.Clear();
				for (int i = 0; i < things.Count; i++)
				{
					ThingDef def;
					ThingDef expr_1C = def = things[i].def;
					float num = this[def];
					this[expr_1C] = num + (float)things[i].stackCount;
				}
			}
		}

		private List<ThingAmount> chosenIngThings = new List<ThingAmount>();

		private static readonly IntRange ReCheckFailedBillTicksRange = new IntRange(500, 600);

		private static string MissingMaterialsTranslated;

		private static string MissingSkillTranslated;

		private static List<Thing> relevantThings = new List<Thing>();

		private static List<Thing> newRelevantThings = new List<Thing>();

		private static List<IngredientCount> ingredientsOrdered = new List<IngredientCount>();

		private static List<Thing> tmpMedicine = new List<Thing>();

		private static HashSet<Thing> assignedThings = new HashSet<Thing>();

		private static WorkGiver_DoBill.DefCountList availableCounts = new WorkGiver_DoBill.DefCountList();

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				if (this.def.fixedBillGiverDefs != null && this.def.fixedBillGiverDefs.Count == 1)
				{
					return ThingRequest.ForDef(this.def.fixedBillGiverDefs[0]);
				}
				return ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver);
			}
		}

		public WorkGiver_DoBill()
		{
			if (WorkGiver_DoBill.MissingSkillTranslated == null)
			{
				WorkGiver_DoBill.MissingSkillTranslated = "MissingSkill".Translate();
			}
			if (WorkGiver_DoBill.MissingMaterialsTranslated == null)
			{
				WorkGiver_DoBill.MissingMaterialsTranslated = "MissingMaterials".Translate();
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing thing)
		{
			IBillGiver billGiver = thing as IBillGiver;
			if (billGiver == null || !this.ThingIsUsableBillGiver(thing) || !billGiver.CurrentlyUsable() || !billGiver.BillStack.AnyShouldDoNow || !pawn.CanReserve(thing, 1) || thing.IsBurning() || thing.IsForbidden(pawn))
			{
				return null;
			}
			if (!pawn.CanReach(thing.InteractionCell, PathEndMode.OnCell, Danger.Some, false, TraverseMode.ByPawn))
			{
				return null;
			}
			billGiver.BillStack.RemoveIncompletableBills();
			return this.StartOrResumeBillJob(pawn, billGiver);
		}

		private static UnfinishedThing ClosestUnfinishedThingForBill(Pawn pawn, Bill_ProductionWithUft bill)
		{
			Predicate<Thing> predicate = (Thing t) => !t.IsForbidden(pawn) && ((UnfinishedThing)t).Recipe == bill.recipe && ((UnfinishedThing)t).Creator == pawn && ((UnfinishedThing)t).ingredients.TrueForAll((Thing x) => bill.ingredientFilter.Allows(x.def)) && pawn.CanReserve(t, 1);
			Predicate<Thing> validator = predicate;
			return (UnfinishedThing)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(bill.recipe.unfinishedThingDef), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
		}

		private static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
		{
			if (uft.Creator != pawn)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to get FinishUftJob for ",
					pawn,
					" finishing ",
					uft,
					" but its creator is ",
					uft.Creator
				}));
				return null;
			}
			Job job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, bill.billStack.billGiver, uft);
			if (job != null && job.targetA.Thing != uft)
			{
				return job;
			}
			return new Job(JobDefOf.DoBill, (Thing)bill.billStack.billGiver)
			{
				bill = bill,
				targetQueueB = new List<LocalTargetInfo>
				{
					uft
				},
				countQueue = new List<int>
				{
					1
				},
				haulMode = HaulMode.ToCellNonStorage
			};
		}

		private Job StartOrResumeBillJob(Pawn pawn, IBillGiver giver)
		{
			for (int i = 0; i < giver.BillStack.Count; i++)
			{
				Bill bill = giver.BillStack[i];
				if (bill.recipe.requiredGiverWorkType == null || bill.recipe.requiredGiverWorkType == this.def.workType)
				{
					if (Find.TickManager.TicksGame >= bill.lastIngredientSearchFailTicks + WorkGiver_DoBill.ReCheckFailedBillTicksRange.RandomInRange || FloatMenuMakerMap.making)
					{
						if (bill.ShouldDoNow())
						{
							if (bill.PawnAllowedToStartAnew(pawn))
							{
								if (!bill.recipe.PawnSatisfiesSkillRequirements(pawn))
								{
									JobFailReason.Is(WorkGiver_DoBill.MissingSkillTranslated);
								}
								else
								{
									Bill_ProductionWithUft bill_ProductionWithUft = bill as Bill_ProductionWithUft;
									if (bill_ProductionWithUft != null)
									{
										if (bill_ProductionWithUft.BoundUft != null)
										{
											if (bill_ProductionWithUft.BoundWorker != pawn || !pawn.CanReserveAndReach(bill_ProductionWithUft.BoundUft, PathEndMode.Touch, Danger.Deadly, 1) || bill_ProductionWithUft.BoundUft.IsForbidden(pawn))
											{
												goto IL_180;
											}
											return WorkGiver_DoBill.FinishUftJob(pawn, bill_ProductionWithUft.BoundUft, bill_ProductionWithUft);
										}
										else
										{
											UnfinishedThing unfinishedThing = WorkGiver_DoBill.ClosestUnfinishedThingForBill(pawn, bill_ProductionWithUft);
											if (unfinishedThing != null)
											{
												return WorkGiver_DoBill.FinishUftJob(pawn, unfinishedThing, bill_ProductionWithUft);
											}
										}
									}
									if (WorkGiver_DoBill.TryFindBestBillIngredients(bill, pawn, (Thing)giver, this.chosenIngThings))
									{
										return this.TryStartNewDoBillJob(pawn, bill, giver);
									}
									if (!FloatMenuMakerMap.making)
									{
										bill.lastIngredientSearchFailTicks = Find.TickManager.TicksGame;
									}
									else
									{
										JobFailReason.Is(WorkGiver_DoBill.MissingMaterialsTranslated);
									}
								}
							}
						}
					}
				}
				IL_180:;
			}
			return null;
		}

		private Job TryStartNewDoBillJob(Pawn pawn, Bill bill, IBillGiver giver)
		{
			Job job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, giver, null);
			if (job != null)
			{
				return job;
			}
			Job job2 = new Job(JobDefOf.DoBill, (Thing)giver);
			job2.targetQueueB = new List<LocalTargetInfo>(this.chosenIngThings.Count);
			job2.countQueue = new List<int>(this.chosenIngThings.Count);
			for (int i = 0; i < this.chosenIngThings.Count; i++)
			{
				job2.targetQueueB.Add(this.chosenIngThings[i].thing);
				job2.countQueue.Add(this.chosenIngThings[i].count);
			}
			job2.haulMode = HaulMode.ToCellNonStorage;
			job2.bill = bill;
			return job2;
		}

		private bool ThingIsUsableBillGiver(Thing thing)
		{
			Pawn pawn = thing as Pawn;
			Corpse corpse = thing as Corpse;
			Pawn pawn2 = null;
			if (corpse != null)
			{
				pawn2 = corpse.InnerPawn;
			}
			if (this.def.fixedBillGiverDefs != null && this.def.fixedBillGiverDefs.Contains(thing.def))
			{
				return true;
			}
			if (pawn != null)
			{
				if (this.def.billGiversAllHumanlikes && pawn.RaceProps.Humanlike)
				{
					return true;
				}
				if (this.def.billGiversAllMechanoids && pawn.RaceProps.IsMechanoid)
				{
					return true;
				}
				if (this.def.billGiversAllAnimals && pawn.RaceProps.Animal)
				{
					return true;
				}
			}
			if (corpse != null && pawn2 != null)
			{
				if (this.def.billGiversAllHumanlikesCorpses && pawn2.RaceProps.Humanlike)
				{
					return true;
				}
				if (this.def.billGiversAllMechanoidsCorpses && pawn2.RaceProps.IsMechanoid)
				{
					return true;
				}
				if (this.def.billGiversAllAnimalsCorpses && pawn2.RaceProps.Animal)
				{
					return true;
				}
			}
			return false;
		}

		private static bool TryFindBestBillIngredients(Bill bill, Pawn pawn, Thing billGiver, List<ThingAmount> chosen)
		{
			chosen.Clear();
			if (bill.recipe.ingredients.Count == 0)
			{
				return true;
			}
			IntVec3 billGiverRootCell = WorkGiver_DoBill.GetBillGiverRootCell(billGiver, pawn);
			Region validRegionAt = pawn.Map.regionGrid.GetValidRegionAt(billGiverRootCell);
			if (validRegionAt == null)
			{
				return false;
			}
			WorkGiver_DoBill.MakeIngredientsListInProcessingOrder(WorkGiver_DoBill.ingredientsOrdered, bill);
			WorkGiver_DoBill.relevantThings.Clear();
			bool foundAll = false;
			Predicate<Thing> baseValidator = (Thing t) => t.Spawned && !t.IsForbidden(pawn) && (t.Position - billGiver.Position).LengthHorizontalSquared < bill.ingredientSearchRadius * bill.ingredientSearchRadius && bill.recipe.fixedIngredientFilter.Allows(t) && bill.ingredientFilter.Allows(t) && bill.recipe.ingredients.Any((IngredientCount ingNeed) => ingNeed.filter.Allows(t)) && pawn.CanReserve(t, 1) && (!bill.CheckIngredientsIfSociallyProper || t.IsSociallyProper(pawn));
			bool billGiverIsPawn = billGiver is Pawn;
			if (billGiverIsPawn)
			{
				WorkGiver_DoBill.AddEveryMedicineToRelevantThings(pawn, billGiver, WorkGiver_DoBill.relevantThings, baseValidator, pawn.Map);
				if (WorkGiver_DoBill.TryFindBestBillIngredientsInSet(WorkGiver_DoBill.relevantThings, bill, chosen))
				{
					return true;
				}
			}
			TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParams, false);
			RegionProcessor regionProcessor = delegate(Region r)
			{
				WorkGiver_DoBill.newRelevantThings.Clear();
				List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
				for (int i = 0; i < list.Count; i++)
				{
					Thing thing = list[i];
					if (baseValidator(thing) && (!thing.def.IsMedicine || !billGiverIsPawn))
					{
						WorkGiver_DoBill.newRelevantThings.Add(thing);
					}
				}
				if (WorkGiver_DoBill.newRelevantThings.Count > 0)
				{
					Comparison<Thing> comparison = delegate(Thing t1, Thing t2)
					{
						float lengthHorizontalSquared = (t1.Position - pawn.Position).LengthHorizontalSquared;
						float lengthHorizontalSquared2 = (t2.Position - pawn.Position).LengthHorizontalSquared;
						return lengthHorizontalSquared.CompareTo(lengthHorizontalSquared2);
					};
					WorkGiver_DoBill.newRelevantThings.Sort(comparison);
					WorkGiver_DoBill.relevantThings.AddRange(WorkGiver_DoBill.newRelevantThings);
					WorkGiver_DoBill.newRelevantThings.Clear();
					if (WorkGiver_DoBill.TryFindBestBillIngredientsInSet(WorkGiver_DoBill.relevantThings, bill, chosen))
					{
						foundAll = true;
						return true;
					}
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(validRegionAt, entryCondition, regionProcessor, 99999);
			return foundAll;
		}

		private static IntVec3 GetBillGiverRootCell(Thing billGiver, Pawn forPawn)
		{
			Building building = billGiver as Building;
			if (building == null)
			{
				return billGiver.Position;
			}
			if (building.def.hasInteractionCell)
			{
				return building.InteractionCell;
			}
			Log.Error("Tried to find bill ingredients for " + billGiver + " which has no interaction cell.");
			return forPawn.Position;
		}

		private static void AddEveryMedicineToRelevantThings(Pawn pawn, Thing billGiver, List<Thing> relevantThings, Predicate<Thing> baseValidator, Map map)
		{
			MedicalCareCategory medicalCareCategory = WorkGiver_DoBill.GetMedicalCareCategory(billGiver);
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
			WorkGiver_DoBill.tmpMedicine.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (medicalCareCategory.AllowsMedicine(thing.def) && baseValidator(thing) && pawn.CanReach(thing, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					WorkGiver_DoBill.tmpMedicine.Add(thing);
				}
			}
			WorkGiver_DoBill.tmpMedicine.SortBy((Thing x) => -x.GetStatValue(StatDefOf.MedicalPotency, true), (Thing x) => x.Position.DistanceToSquared(billGiver.Position));
			relevantThings.AddRange(WorkGiver_DoBill.tmpMedicine);
			WorkGiver_DoBill.tmpMedicine.Clear();
		}

		private static MedicalCareCategory GetMedicalCareCategory(Thing billGiver)
		{
			Pawn pawn = billGiver as Pawn;
			if (pawn != null && pawn.playerSettings != null)
			{
				return pawn.playerSettings.medCare;
			}
			return MedicalCareCategory.Best;
		}

		private static void MakeIngredientsListInProcessingOrder(List<IngredientCount> ingredientsOrdered, Bill bill)
		{
			ingredientsOrdered.Clear();
			if (bill.recipe.productHasIngredientStuff)
			{
				ingredientsOrdered.Add(bill.recipe.ingredients[0]);
			}
			for (int i = 0; i < bill.recipe.ingredients.Count; i++)
			{
				if (!bill.recipe.productHasIngredientStuff || i != 0)
				{
					IngredientCount ingredientCount = bill.recipe.ingredients[i];
					if (ingredientCount.filter.AllowedDefCount == 1)
					{
						ingredientsOrdered.Add(ingredientCount);
					}
				}
			}
			for (int j = 0; j < bill.recipe.ingredients.Count; j++)
			{
				IngredientCount item = bill.recipe.ingredients[j];
				if (!ingredientsOrdered.Contains(item))
				{
					ingredientsOrdered.Add(item);
				}
			}
		}

		private static bool TryFindBestBillIngredientsInSet(List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
		{
			if (bill.recipe.allowMixingIngredients)
			{
				return WorkGiver_DoBill.TryFindBestBillIngredientsInSet_AllowMix(availableThings, bill, chosen);
			}
			return WorkGiver_DoBill.TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen);
		}

		private static bool TryFindBestBillIngredientsInSet_NoMix(List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
		{
			RecipeDef recipe = bill.recipe;
			chosen.Clear();
			WorkGiver_DoBill.assignedThings.Clear();
			WorkGiver_DoBill.availableCounts.Clear();
			WorkGiver_DoBill.availableCounts.GenerateFrom(availableThings);
			for (int i = 0; i < WorkGiver_DoBill.ingredientsOrdered.Count; i++)
			{
				IngredientCount ingredientCount = recipe.ingredients[i];
				bool flag = false;
				for (int j = 0; j < WorkGiver_DoBill.availableCounts.Count; j++)
				{
					float num = (float)ingredientCount.CountRequiredOfFor(WorkGiver_DoBill.availableCounts.GetDef(j), bill.recipe);
					if (num <= WorkGiver_DoBill.availableCounts.GetCount(j))
					{
						if (ingredientCount.filter.Allows(WorkGiver_DoBill.availableCounts.GetDef(j)))
						{
							for (int k = 0; k < availableThings.Count; k++)
							{
								if (availableThings[k].def == WorkGiver_DoBill.availableCounts.GetDef(j))
								{
									if (!WorkGiver_DoBill.assignedThings.Contains(availableThings[k]))
									{
										int num2 = Mathf.Min(Mathf.FloorToInt(num), availableThings[k].stackCount);
										ThingAmount.AddToList(chosen, availableThings[k], num2);
										num -= (float)num2;
										WorkGiver_DoBill.assignedThings.Add(availableThings[k]);
										if (num < 0.001f)
										{
											flag = true;
											float num3 = WorkGiver_DoBill.availableCounts.GetCount(j);
											num3 -= ingredientCount.GetBaseCount();
											WorkGiver_DoBill.availableCounts.SetCount(j, num3);
											break;
										}
									}
								}
							}
							if (flag)
							{
								break;
							}
						}
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}

		private static bool TryFindBestBillIngredientsInSet_AllowMix(List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
		{
			chosen.Clear();
			for (int i = 0; i < bill.recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = bill.recipe.ingredients[i];
				float num = ingredientCount.GetBaseCount();
				for (int j = 0; j < availableThings.Count; j++)
				{
					Thing thing = availableThings[j];
					if (ingredientCount.filter.Allows(thing))
					{
						float num2 = bill.recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
						int num3 = Mathf.Min(Mathf.CeilToInt(num / num2), thing.stackCount);
						ThingAmount.AddToList(chosen, thing, num3);
						num -= (float)num3 * num2;
						if (num <= 0.0001f)
						{
							break;
						}
					}
				}
				if (num > 0.0001f)
				{
					return false;
				}
			}
			return true;
		}
	}
}
