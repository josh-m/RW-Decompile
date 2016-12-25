using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse.AI
{
	public class Toils_Haul
	{
		public static bool ErrorCheckForCarry(Thing haulThing, Pawn actor)
		{
			if (!haulThing.Spawned)
			{
				Log.Message(string.Concat(new object[]
				{
					actor,
					" tried to start carry ",
					haulThing,
					" which isn't spawned."
				}));
				actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				return true;
			}
			if (haulThing.stackCount == 0)
			{
				Log.Message(string.Concat(new object[]
				{
					actor,
					" tried to start carry ",
					haulThing,
					" which had stackcount 0."
				}));
				actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				return true;
			}
			if (actor.jobs.curJob.maxNumToCarry <= 0)
			{
				Log.Error(string.Concat(new object[]
				{
					"Invalid maxNumToCarry: ",
					actor.jobs.curJob.maxNumToCarry,
					", setting to 1. Job was ",
					actor.jobs.curJob
				}));
				actor.jobs.curJob.maxNumToCarry = 1;
			}
			return false;
		}

		public static Toil StartCarryThing(TargetIndex haulableInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(haulableInd).Thing;
				if (Toils_Haul.ErrorCheckForCarry(thing, actor))
				{
					return;
				}
				int num = Mathf.Min(new int[]
				{
					curJob.maxNumToCarry - actor.carrier.container.TotalStackCount,
					actor.carrier.AvailableStackSpace(thing.def),
					thing.stackCount
				});
				if (num > 0 && actor.carrier.TryStartCarry(thing, num))
				{
					if (thing.Spawned && Find.Reservations.IsReserved(thing, actor.Faction) && (curJob.targetQueueA == null || !curJob.targetQueueA.Contains(thing)) && (curJob.targetQueueB == null || !curJob.targetQueueB.Contains(thing)))
					{
						Find.Reservations.Release(thing, actor);
					}
					actor.records.Increment(RecordDefOf.ThingsHauled);
				}
				if (actor.carrier.CarriedThing != null)
				{
					curJob.SetTarget(haulableInd, actor.carrier.CarriedThing);
					actor.Reserve(actor.carrier.CarriedThing, 1);
				}
				else
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
			};
			return toil;
		}

		public static Toil JumpIfAlsoCollectingNextTargetInQueue(Toil gotoGetTargetToil, TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				List<TargetInfo> targetQueue = curJob.GetTargetQueue(ind);
				if (targetQueue.NullOrEmpty<TargetInfo>())
				{
					return;
				}
				if (actor.carrier.CarriedThing == null)
				{
					Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor + " who is not carrying something.");
					return;
				}
				for (int i = 0; i < targetQueue.Count; i++)
				{
					if (!GenAI.CanUseItemForWork(actor, targetQueue[i].Thing))
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable);
						return;
					}
					if (targetQueue[i].Thing.def == actor.carrier.CarriedThing.def)
					{
						curJob.SetTarget(ind, targetQueue[i].Thing);
						targetQueue.RemoveAt(i);
						actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
						break;
					}
				}
			};
			return toil;
		}

		public static Toil CheckForGetOpportunityDuplicate(Toil getHaulTargetToil, TargetIndex haulableInd, TargetIndex storeCellInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (actor.carrier.CarriedThing.def.stackLimit == 1)
				{
					return;
				}
				if (actor.carrier.Full)
				{
					return;
				}
				int num = curJob.maxNumToCarry - actor.carrier.CarriedThing.stackCount;
				if (num <= 0)
				{
					return;
				}
				Predicate<Thing> validator = (Thing t) => t.Spawned && t.def == actor.carrier.CarriedThing.def && t.CanStackWith(actor.carrier.CarriedThing) && !t.IsForbidden(actor) && !t.IsInValidStorage() && (storeCellInd == TargetIndex.None || curJob.GetTarget(storeCellInd).Cell.IsValidStorageFor(t)) && actor.CanReserve(t, 1);
				Thing thing = GenClosest.ClosestThingReachable(actor.Position, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.ClosestTouch, TraverseParms.For(actor, Danger.Deadly, TraverseMode.ByPawn, false), 8f, validator, null, -1, false);
				if (thing != null)
				{
					curJob.SetTarget(haulableInd, thing);
					actor.jobs.curDriver.SetNextToil(getHaulTargetToil);
					actor.jobs.curDriver.SetCompleteMode(ToilCompleteMode.Instant);
				}
			};
			return toil;
		}

		public static Toil CarryHauledThingToCell(TargetIndex squareIndex)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				IntVec3 cell = toil.actor.jobs.curJob.GetTarget(squareIndex).Cell;
				toil.actor.pather.StartPath(cell, PathEndMode.ClosestTouch);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.AddFailCondition(delegate
			{
				Pawn actor = toil.actor;
				IntVec3 cell = actor.jobs.curJob.GetTarget(squareIndex).Cell;
				return actor.jobs.curJob.haulMode == HaulMode.ToCellStorage && !cell.IsValidStorageFor(actor.carrier.CarriedThing);
			});
			return toil;
		}

		public static Toil PlaceCarriedThingInCellFacing(TargetIndex facingTargetInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				TargetInfo target = actor.CurJob.GetTarget(facingTargetInd);
				IntVec3 b;
				if (target.HasThing)
				{
					b = target.Thing.OccupiedRect().ClosestCellTo(actor.Position);
				}
				else
				{
					b = target.Cell;
				}
				IntVec3 dropLoc = actor.Position + PawnRotator.RotFromAngleBiased((actor.Position - b).AngleFlat).FacingCell;
				Thing thing;
				if (!actor.carrier.TryDropCarriedThing(dropLoc, ThingPlaceMode.Direct, out thing, null))
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
			};
			return toil;
		}

		public static Toil PlaceHauledThingInCell(TargetIndex cellInd, Toil nextToilOnPlaceFailOrIncomplete, bool storageMode)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				IntVec3 cell = curJob.GetTarget(cellInd).Cell;
				SlotGroup slotGroup = Find.SlotGroupManager.SlotGroupAt(cell);
				if (slotGroup != null && slotGroup.Settings.AllowedToAccept(actor.carrier.CarriedThing))
				{
					Find.DesignationManager.RemoveAllDesignationsOn(actor.carrier.CarriedThing, false);
				}
				Action<Thing, int> placedAction = null;
				if (curJob.def == JobDefOf.DoBill)
				{
					placedAction = delegate(Thing th, int added)
					{
						if (curJob.placedThings == null)
						{
							curJob.placedThings = new List<ThingStackPart>();
						}
						ThingStackPart thingStackPart = curJob.placedThings.Find((ThingStackPart x) => x.thing == th);
						if (thingStackPart != null)
						{
							thingStackPart.Count += added;
						}
						else
						{
							curJob.placedThings.Add(new ThingStackPart(th, added));
						}
					};
				}
				Thing thing;
				if (!actor.carrier.TryDropCarriedThing(cell, ThingPlaceMode.Direct, out thing, placedAction))
				{
					if (storageMode)
					{
						IntVec3 vec;
						if (nextToilOnPlaceFailOrIncomplete != null && StoreUtility.TryFindBestBetterStoreCellFor(actor.carrier.CarriedThing, actor, StoragePriority.Unstored, actor.Faction, out vec, true))
						{
							actor.CurJob.SetTarget(cellInd, vec);
							actor.jobs.curDriver.SetNextToil(nextToilOnPlaceFailOrIncomplete);
							return;
						}
						Job job = HaulAIUtility.HaulAsideJobFor(actor, actor.carrier.CarriedThing);
						if (job != null)
						{
							curJob.targetA = job.targetA;
							curJob.targetB = job.targetB;
							curJob.targetC = job.targetC;
							curJob.maxNumToCarry = job.maxNumToCarry;
							curJob.haulOpportunisticDuplicates = job.haulOpportunisticDuplicates;
							curJob.haulMode = job.haulMode;
							actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
						}
						else
						{
							Log.Error(string.Concat(new object[]
							{
								"Incomplete haul for ",
								actor,
								": Could not find anywhere to put ",
								actor.carrier.CarriedThing,
								" near ",
								actor.Position,
								". Destroying. This should never happen!"
							}));
							actor.carrier.CarriedThing.Destroy(DestroyMode.Vanish);
						}
					}
					else if (nextToilOnPlaceFailOrIncomplete != null)
					{
						actor.jobs.curDriver.SetNextToil(nextToilOnPlaceFailOrIncomplete);
						return;
					}
				}
			};
			return toil;
		}

		public static Toil CarryHauledThingToContainer()
		{
			Toil gotoDest = new Toil();
			gotoDest.initAction = delegate
			{
				gotoDest.actor.pather.StartPath(gotoDest.actor.jobs.curJob.targetB.Thing, PathEndMode.Touch);
			};
			gotoDest.AddFailCondition(delegate
			{
				Thing thing = gotoDest.actor.jobs.curJob.targetB.Thing;
				if (thing.Destroyed || thing.IsForbidden(gotoDest.actor))
				{
					return true;
				}
				IThingContainerOwner thingContainerOwner = thing as IThingContainerOwner;
				if (thingContainerOwner != null)
				{
					ThingContainer container = thingContainerOwner.GetContainer();
					if (!container.CanAcceptAnyOf(gotoDest.actor.carrier.CarriedThing))
					{
						return true;
					}
				}
				return false;
			});
			gotoDest.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return gotoDest;
		}

		public static Toil DepositHauledThingInContainer(TargetIndex containerInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (actor.carrier.CarriedThing == null)
				{
					Log.Error(actor + " tried to place hauled thing in container but is not hauling anything.");
					return;
				}
				IThingContainerOwner thingContainerOwner = curJob.GetTarget(containerInd).Thing as IThingContainerOwner;
				if (thingContainerOwner != null)
				{
					int num = actor.carrier.CarriedThing.stackCount;
					if (thingContainerOwner is IConstructible)
					{
						int a = GenConstruct.AmountNeededByOf((IConstructible)thingContainerOwner, actor.carrier.CarriedThing.def);
						num = Mathf.Min(a, num);
					}
					actor.carrier.container.TransferToContainer(actor.carrier.CarriedThing, thingContainerOwner.GetContainer(), num);
				}
				else if (curJob.GetTarget(containerInd).Thing.def.Minifiable)
				{
					actor.carrier.container.Clear();
				}
				else
				{
					Log.Error("Could not deposit hauled thing in container: " + curJob.GetTarget(containerInd).Thing);
				}
			};
			return toil;
		}

		public static Toil JumpToCarryToNextContainerIfPossible(Toil carryToContainerToil)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (actor.carrier.CarriedThing == null)
				{
					return;
				}
				if (curJob.targetQueueB != null && curJob.targetQueueB.Count > 0)
				{
					Predicate<Thing> validator = (Thing th) => GenConstruct.CanConstruct(th, actor) && ((IConstructible)th).MaterialsNeeded().Any((ThingCount need) => need.thingDef == actor.carrier.CarriedThing.def);
					Thing nextTarget = GenClosest.ClosestThing_Global_Reachable(actor.Position, from targ in curJob.targetQueueB
					select targ.Thing, PathEndMode.Touch, TraverseParms.For(actor, Danger.Deadly, TraverseMode.ByPawn, false), 99999f, validator, null);
					if (nextTarget != null)
					{
						curJob.targetQueueB.RemoveAll((TargetInfo targ) => targ.Thing == nextTarget);
						curJob.targetB = nextTarget;
						actor.jobs.curDriver.JumpToToil(carryToContainerToil);
					}
				}
			};
			return toil;
		}

		public static Toil TakeToInventory(TargetIndex ind, int count)
		{
			return Toils_Haul.TakeToInventory(ind, () => count);
		}

		public static Toil TakeToInventory(TargetIndex ind, Func<int> countGetter)
		{
			Toil takeThing = new Toil();
			takeThing.initAction = delegate
			{
				Pawn actor = takeThing.actor;
				Thing thing = actor.CurJob.GetTarget(ind).Thing;
				if (Toils_Haul.ErrorCheckForCarry(thing, actor))
				{
					return;
				}
				int num = Mathf.Min(countGetter(), thing.stackCount);
				if (num <= 0)
				{
					actor.jobs.curDriver.ReadyForNextToil();
				}
				else
				{
					takeThing.actor.inventory.GetContainer().TryAdd(thing, num);
					if (thing.def.ingestible != null && thing.def.ingestible.preferability <= FoodPreferability.RawTasty)
					{
						actor.mindState.lastInventoryRawFoodUseTick = Find.TickManager.TicksGame;
					}
				}
			};
			return takeThing;
		}
	}
}
