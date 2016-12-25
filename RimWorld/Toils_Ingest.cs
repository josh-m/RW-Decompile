using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Toils_Ingest
	{
		private static List<IntVec3> spotSearchList = new List<IntVec3>();

		private static List<IntVec3> cardinals = GenAdj.CardinalDirections.ToList<IntVec3>();

		private static List<IntVec3> diagonals = GenAdj.DiagonalDirections.ToList<IntVec3>();

		public static Toil TakeMealFromDispenser(TargetIndex ind, Pawn eater)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Building_NutrientPasteDispenser building_NutrientPasteDispenser = (Building_NutrientPasteDispenser)curJob.GetTarget(ind).Thing;
				Thing thing = building_NutrientPasteDispenser.TryDispenseFood();
				if (thing == null)
				{
					actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
					return;
				}
				actor.carryTracker.TryStartCarry(thing);
				actor.jobs.curJob.targetA = actor.carryTracker.CarriedThing;
			};
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = Building_NutrientPasteDispenser.CollectDuration;
			return toil;
		}

		public static Toil PickupIngestible(TargetIndex ind, Pawn eater)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(ind).Thing;
				if (curJob.count <= 0)
				{
					Log.Error("Tried to do PickupIngestible toil with job.maxNumToCarry = " + curJob.count);
					actor.jobs.EndCurrentJob(JobCondition.Errored, true);
					return;
				}
				int count = Mathf.Min(thing.stackCount, curJob.count);
				actor.carryTracker.TryStartCarry(thing, count);
				if (thing != actor.carryTracker.CarriedThing && actor.Map.reservationManager.ReservedBy(thing, actor))
				{
					actor.Map.reservationManager.Release(thing, actor);
				}
				actor.jobs.curJob.targetA = actor.carryTracker.CarriedThing;
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}

		public static Toil CarryIngestibleToChewSpot(Pawn pawn, TargetIndex ingestibleInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Thing thing = null;
				Thing thing2 = actor.CurJob.GetTarget(ingestibleInd).Thing;
				if (thing2.def.ingestible.chairSearchRadius > 0f)
				{
					Predicate<Thing> validator = delegate(Thing t)
					{
						if (t.def.building == null || !t.def.building.isSittable)
						{
							return false;
						}
						if (t.IsForbidden(pawn))
						{
							return false;
						}
						if (!actor.CanReserve(t, 1))
						{
							return false;
						}
						if (!t.IsSociallyProper(actor))
						{
							return false;
						}
						if (t.IsBurning())
						{
							return false;
						}
						if (t.HostileTo(pawn))
						{
							return false;
						}
						bool result = false;
						for (int i = 0; i < 4; i++)
						{
							IntVec3 c = t.Position + GenAdj.CardinalDirections[i];
							Building edifice = c.GetEdifice(t.Map);
							if (edifice != null && edifice.def.surfaceType == SurfaceType.Eat)
							{
								result = true;
								break;
							}
						}
						return result;
					};
					thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(actor, Danger.Deadly, TraverseMode.ByPawn, false), thing2.def.ingestible.chairSearchRadius, validator, null, -1, false);
				}
				IntVec3 intVec;
				if (thing != null)
				{
					intVec = thing.Position;
					actor.Reserve(thing, 1);
				}
				else
				{
					intVec = RCellFinder.SpotToChewStandingNear(actor, actor.CurJob.targetA.Thing);
				}
				actor.Map.pawnDestinationManager.ReserveDestinationFor(actor, intVec);
				actor.pather.StartPath(intVec, PathEndMode.OnCell);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		public static bool TryFindAdjacentIngestionPlaceSpot(IntVec3 root, ThingDef ingestibleDef, Pawn pawn, out IntVec3 placeSpot)
		{
			placeSpot = IntVec3.Invalid;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = root + GenAdj.CardinalDirections[i];
				if (intVec.HasEatSurface(pawn.Map))
				{
					if (!(from t in pawn.Map.thingGrid.ThingsAt(intVec)
					where t.def == ingestibleDef
					select t).Any<Thing>())
					{
						if (!intVec.IsForbidden(pawn))
						{
							placeSpot = intVec;
							return true;
						}
					}
				}
			}
			if (!placeSpot.IsValid)
			{
				Toils_Ingest.spotSearchList.Clear();
				Toils_Ingest.cardinals.Shuffle<IntVec3>();
				for (int j = 0; j < 4; j++)
				{
					Toils_Ingest.spotSearchList.Add(Toils_Ingest.cardinals[j]);
				}
				Toils_Ingest.diagonals.Shuffle<IntVec3>();
				for (int k = 0; k < 4; k++)
				{
					Toils_Ingest.spotSearchList.Add(Toils_Ingest.diagonals[k]);
				}
				Toils_Ingest.spotSearchList.Add(IntVec3.Zero);
				for (int l = 0; l < Toils_Ingest.spotSearchList.Count; l++)
				{
					IntVec3 intVec2 = root + Toils_Ingest.spotSearchList[l];
					if (intVec2.Walkable(pawn.Map) && !intVec2.IsForbidden(pawn))
					{
						if (!(from t in pawn.Map.thingGrid.ThingsAt(intVec2)
						where t.def == ingestibleDef
						select t).Any<Thing>())
						{
							placeSpot = intVec2;
							return true;
						}
					}
				}
			}
			return false;
		}

		public static Toil FindAdjacentEatSurface(TargetIndex eatSurfaceInd, TargetIndex foodInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				IntVec3 position = actor.Position;
				Map map = actor.Map;
				for (int i = 0; i < 4; i++)
				{
					Rot4 rot = new Rot4(i);
					IntVec3 intVec = position + rot.FacingCell;
					if (intVec.HasEatSurface(map))
					{
						toil.actor.CurJob.SetTarget(eatSurfaceInd, intVec);
						toil.actor.jobs.curDriver.rotateToFace = eatSurfaceInd;
						Thing thing = toil.actor.CurJob.GetTarget(foodInd).Thing;
						if (thing.def.rotatable)
						{
							thing.Rotation = Rot4.FromIntVec3(intVec - toil.actor.Position);
						}
						return;
					}
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}

		public static Toil ChewIngestible(Pawn chewer, float durationMultiplier, TargetIndex ingestibleInd, TargetIndex eatSurfaceInd = TargetIndex.None)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Thing thing = actor.CurJob.GetTarget(ingestibleInd).Thing;
				actor.Drawer.rotator.FaceCell(chewer.Position);
				if (!thing.IngestibleNow)
				{
					chewer.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					return;
				}
				actor.jobs.curDriver.ticksLeftThisToil = Mathf.RoundToInt((float)thing.def.ingestible.baseIngestTicks * durationMultiplier);
				if (thing.Spawned)
				{
					thing.Map.physicalInteractionReservationManager.Reserve(chewer, thing);
				}
			};
			toil.tickAction = delegate
			{
				toil.actor.GainComfortFromCellIfPossible();
			};
			toil.WithProgressBar(ingestibleInd, delegate
			{
				Pawn actor = toil.actor;
				Thing thing = actor.CurJob.GetTarget(ingestibleInd).Thing;
				if (thing == null)
				{
					return 1f;
				}
				return 1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / Mathf.Round((float)thing.def.ingestible.baseIngestTicks * durationMultiplier);
			}, false, -0.5f);
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.FailOnDestroyedOrNull(ingestibleInd);
			toil.AddFinishAction(delegate
			{
				if (chewer == null)
				{
					return;
				}
				if (chewer.CurJob == null)
				{
					return;
				}
				Thing thing = chewer.CurJob.GetTarget(ingestibleInd).Thing;
				if (thing == null)
				{
					return;
				}
				if (chewer.Map.physicalInteractionReservationManager.IsReservedBy(chewer, thing))
				{
					chewer.Map.physicalInteractionReservationManager.Release(chewer, thing);
				}
			});
			Toils_Ingest.AddIngestionEffects(toil, chewer, ingestibleInd, eatSurfaceInd);
			return toil;
		}

		public static Toil AddIngestionEffects(Toil toil, Pawn chewer, TargetIndex ingestibleInd, TargetIndex eatSurfaceInd)
		{
			toil.WithEffect(delegate
			{
				LocalTargetInfo target = toil.actor.CurJob.GetTarget(ingestibleInd);
				if (!target.HasThing)
				{
					return null;
				}
				EffecterDef result = target.Thing.def.ingestible.ingestEffect;
				if (chewer.RaceProps.intelligence < Intelligence.ToolUser && target.Thing.def.ingestible.ingestEffectEat != null)
				{
					result = target.Thing.def.ingestible.ingestEffectEat;
				}
				return result;
			}, delegate
			{
				if (!toil.actor.CurJob.GetTarget(ingestibleInd).HasThing)
				{
					return null;
				}
				Thing thing = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
				if (chewer != toil.actor)
				{
					return chewer;
				}
				if (eatSurfaceInd != TargetIndex.None && toil.actor.CurJob.GetTarget(eatSurfaceInd).IsValid)
				{
					return toil.actor.CurJob.GetTarget(eatSurfaceInd);
				}
				return thing;
			});
			toil.PlaySustainerOrSound(delegate
			{
				if (!chewer.RaceProps.Humanlike)
				{
					return null;
				}
				LocalTargetInfo target = toil.actor.CurJob.GetTarget(ingestibleInd);
				if (!target.HasThing)
				{
					return null;
				}
				return target.Thing.def.ingestible.ingestSound;
			});
			return toil;
		}

		public static Toil FinalizeIngest(Pawn ingester, TargetIndex ingestibleInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(ingestibleInd).Thing;
				if (ingester.needs.mood != null && thing.def.IsNutritionGivingIngestible && thing.def.ingestible.chairSearchRadius > 10f)
				{
					if (!(ingester.Position + ingester.Rotation.FacingCell).HasEatSurface(actor.Map) && ingester.GetPosture() == PawnPosture.Standing)
					{
						ingester.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.AteWithoutTable, null);
					}
					Room room = ingester.GetRoom();
					if (room != null)
					{
						int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
						if (ThoughtDefOf.AteInImpressiveDiningRoom.stages[scoreStageIndex] != null)
						{
							ingester.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtMaker.MakeThought(ThoughtDefOf.AteInImpressiveDiningRoom, scoreStageIndex), null);
						}
					}
				}
				float num = ingester.needs.food.NutritionWanted;
				if (curJob.overeat)
				{
					num = Mathf.Max(num, 0.75f);
				}
				float num2 = thing.Ingested(ingester, num);
				if (!ingester.Dead)
				{
					ingester.needs.food.CurLevel += num2;
				}
				ingester.records.AddTo(RecordDefOf.NutritionEaten, num2);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
