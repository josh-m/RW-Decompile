using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public static class Toils_Recipe
	{
		public static Toil MakeUnfinishedThingIfNeeded()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (!curJob.RecipeDef.UsesUnfinishedThing)
				{
					return;
				}
				if (curJob.GetTarget(TargetIndex.B).Thing is UnfinishedThing)
				{
					return;
				}
				Thing thing;
				List<Thing> ingredients = Toils_Recipe.ProcessedIngredients(curJob, out thing);
				ThingDef stuff = (!curJob.RecipeDef.unfinishedThingDef.MadeFromStuff) ? null : thing.def;
				UnfinishedThing unfinishedThing = (UnfinishedThing)ThingMaker.MakeThing(curJob.RecipeDef.unfinishedThingDef, stuff);
				unfinishedThing.Creator = actor;
				unfinishedThing.BoundBill = (Bill_ProductionWithUft)curJob.bill;
				unfinishedThing.ingredients = ingredients;
				CompColorable compColorable = unfinishedThing.TryGetComp<CompColorable>();
				if (compColorable != null)
				{
					compColorable.Color = thing.DrawColor;
				}
				GenSpawn.Spawn(unfinishedThing, curJob.GetTarget(TargetIndex.A).Cell);
				curJob.SetTarget(TargetIndex.B, unfinishedThing);
				Find.Reservations.Reserve(actor, unfinishedThing, 1);
			};
			return toil;
		}

		public static Toil DoRecipeWork()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				if (unfinishedThing != null && unfinishedThing.Initialized)
				{
					jobDriver_DoBill.workLeft = unfinishedThing.workLeft;
				}
				else
				{
					jobDriver_DoBill.workLeft = curJob.bill.recipe.WorkAmountTotal((unfinishedThing == null) ? null : unfinishedThing.Stuff);
					if (unfinishedThing != null)
					{
						unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
					}
				}
				jobDriver_DoBill.billStartTick = Find.TickManager.TicksGame;
				jobDriver_DoBill.ticksSpentDoingRecipeWork = 0;
				curJob.bill.Notify_DoBillStarted();
			};
			toil.tickAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				if (unfinishedThing != null && unfinishedThing.Destroyed)
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
					return;
				}
				jobDriver_DoBill.ticksSpentDoingRecipeWork++;
				curJob.bill.Notify_PawnDidWork(actor);
				IBillGiverWithTickAction billGiverWithTickAction = toil.actor.CurJob.GetTarget(TargetIndex.A).Thing as IBillGiverWithTickAction;
				if (billGiverWithTickAction != null)
				{
					billGiverWithTickAction.BillTick();
				}
				if (curJob.RecipeDef.workSkill != null && curJob.RecipeDef.UsesUnfinishedThing)
				{
					actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(0.11f * curJob.RecipeDef.workSkillLearnFactor);
				}
				float num = (curJob.RecipeDef.workSpeedStat != null) ? actor.GetStatValue(curJob.RecipeDef.workSpeedStat, true) : 1f;
				Building_WorkTable building_WorkTable = jobDriver_DoBill.BillGiver as Building_WorkTable;
				if (building_WorkTable != null)
				{
					num *= building_WorkTable.GetStatValue(StatDefOf.WorkTableWorkSpeedFactor, true);
				}
				if (DebugSettings.fastCrafting)
				{
					num *= 30f;
				}
				jobDriver_DoBill.workLeft -= num;
				if (unfinishedThing != null)
				{
					unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
				}
				actor.GainComfortFromCellIfPossible();
				if (jobDriver_DoBill.workLeft <= 0f)
				{
					jobDriver_DoBill.ReadyForNextToil();
				}
				if (curJob.bill.recipe.UsesUnfinishedThing)
				{
					int num2 = Find.TickManager.TicksGame - jobDriver_DoBill.billStartTick;
					if (num2 >= 3000 && num2 % 1000 == 0)
					{
						actor.jobs.CheckForJobOverride();
					}
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, TargetIndex.A);
			toil.PlaySustainerOrSound(() => toil.actor.CurJob.bill.recipe.soundWorking);
			toil.WithProgressBar(TargetIndex.A, delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.CurJob;
				UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
				return 1f - ((JobDriver_DoBill)actor.jobs.curDriver).workLeft / curJob.bill.recipe.WorkAmountTotal((unfinishedThing == null) ? null : unfinishedThing.Stuff);
			}, false, -0.5f);
			toil.FailOn(() => toil.actor.CurJob.bill.suspended);
			return toil;
		}

		public static Toil FinishRecipeAndStartStoringProduct()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
				if (curJob.RecipeDef.workSkill != null && !curJob.RecipeDef.UsesUnfinishedThing)
				{
					float xp = (float)jobDriver_DoBill.ticksSpentDoingRecipeWork * 0.11f * curJob.RecipeDef.workSkillLearnFactor;
					actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(xp);
				}
				Thing dominantIngredient;
				List<Thing> ingredients = Toils_Recipe.ProcessedIngredients(curJob, out dominantIngredient);
				List<Thing> list = GenRecipe.MakeRecipeProducts(curJob.RecipeDef, actor, ingredients, dominantIngredient).ToList<Thing>();
				curJob.bill.Notify_IterationCompleted(actor, ingredients);
				RecordsUtility.Notify_BillDone(actor, list);
				if (list.Count == 0)
				{
					actor.jobs.EndCurrentJob(JobCondition.Succeeded);
					return;
				}
				if (curJob.bill.GetStoreMode() == BillStoreMode.DropOnFloor)
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (!GenPlace.TryPlaceThing(list[i], actor.Position, ThingPlaceMode.Near, null))
						{
							Log.Error(string.Concat(new object[]
							{
								actor,
								" could not drop recipe product ",
								list[i],
								" near ",
								actor.Position
							}));
						}
					}
					actor.jobs.EndCurrentJob(JobCondition.Succeeded);
					return;
				}
				if (list.Count > 1)
				{
					for (int j = 1; j < list.Count; j++)
					{
						if (!GenPlace.TryPlaceThing(list[j], actor.Position, ThingPlaceMode.Near, null))
						{
							Log.Error(string.Concat(new object[]
							{
								actor,
								" could not drop recipe product ",
								list[j],
								" near ",
								actor.Position
							}));
						}
					}
				}
				list[0].SetPositionDirect(actor.Position);
				IntVec3 vec;
				if (StoreUtility.TryFindBestBetterStoreCellFor(list[0], actor, StoragePriority.Unstored, actor.Faction, out vec, true))
				{
					actor.carrier.TryStartCarry(list[0]);
					curJob.targetB = vec;
					curJob.targetA = list[0];
					curJob.maxNumToCarry = 99999;
					return;
				}
				if (!GenPlace.TryPlaceThing(list[0], actor.Position, ThingPlaceMode.Near, null))
				{
					Log.Error(string.Concat(new object[]
					{
						"Bill doer could not drop product ",
						list[0],
						" near ",
						actor.Position
					}));
				}
				actor.jobs.EndCurrentJob(JobCondition.Succeeded);
			};
			return toil;
		}

		private static Thing GetDominantIngredient(RecipeDef recipe, List<Thing> ingredients)
		{
			if (recipe.productHasIngredientStuff)
			{
				return ingredients[0];
			}
			if (recipe.products.Any((ThingCount x) => x.thingDef.MadeFromStuff))
			{
				return (from x in ingredients
				where x.def.IsStuff
				select x).RandomElementByWeight((Thing x) => (float)x.stackCount);
			}
			return ingredients.RandomElementByWeight((Thing x) => (float)x.stackCount);
		}

		private static List<Thing> ProcessedIngredients(Job job, out Thing dominantIngredient)
		{
			UnfinishedThing uft = job.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
			if (uft != null)
			{
				if (uft.def.MadeFromStuff)
				{
					dominantIngredient = uft.ingredients.First((Thing ing) => ing.def == uft.Stuff);
				}
				else
				{
					dominantIngredient = null;
				}
				List<Thing> ingredients = uft.ingredients;
				uft.Destroy(DestroyMode.Vanish);
				job.placedThings = null;
				return ingredients;
			}
			List<Thing> list = new List<Thing>();
			if (job.placedThings != null)
			{
				for (int i = 0; i < job.placedThings.Count; i++)
				{
					Thing thing = job.placedThings[i].Split();
					if (list.Contains(thing))
					{
						Log.Error("Tried to add ingredient from job placed targets twice: " + thing);
					}
					else
					{
						list.Add(thing);
						IStrippable strippable = thing as IStrippable;
						if (strippable != null)
						{
							strippable.Strip();
						}
						if (job.RecipeDef.UsesUnfinishedThing)
						{
							Find.DesignationManager.RemoveAllDesignationsOn(thing, false);
							if (thing.Spawned)
							{
								thing.DeSpawn();
							}
						}
						else
						{
							thing.Destroy(DestroyMode.Vanish);
						}
					}
				}
			}
			job.placedThings = null;
			if (list.NullOrEmpty<Thing>())
			{
				dominantIngredient = null;
			}
			else
			{
				dominantIngredient = Toils_Recipe.GetDominantIngredient(job.RecipeDef, list);
			}
			return list;
		}
	}
}
