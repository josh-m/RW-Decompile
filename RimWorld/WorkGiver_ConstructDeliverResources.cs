using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_ConstructDeliverResources : WorkGiver_Scanner
	{
		private const float MultiPickupRadius = 5f;

		private const float NearbyConstructScanRadius = 8f;

		private static List<Thing> resourcesAvailable = new List<Thing>();

		private static string MissingMaterialsTranslated;

		public WorkGiver_ConstructDeliverResources()
		{
			if (WorkGiver_ConstructDeliverResources.MissingMaterialsTranslated == null)
			{
				WorkGiver_ConstructDeliverResources.MissingMaterialsTranslated = "MissingMaterials".Translate();
			}
		}

		private static bool ResourceValidator(Pawn pawn, ThingCountClass need, Thing th)
		{
			return th.def == need.thingDef && !th.IsForbidden(pawn) && pawn.CanReserve(th, 1, -1, null, false);
		}

		protected Job ResourceDeliverJobFor(Pawn pawn, IConstructible c, bool canRemoveExistingFloorUnderNearbyNeeders = true)
		{
			Blueprint_Install blueprint_Install = c as Blueprint_Install;
			if (blueprint_Install != null)
			{
				return this.InstallJob(pawn, blueprint_Install);
			}
			bool flag = false;
			List<ThingCountClass> list = c.MaterialsNeeded();
			int count = list.Count;
			int i = 0;
			while (i < count)
			{
				WorkGiver_ConstructDeliverResources.<ResourceDeliverJobFor>c__AnonStorey2AF <ResourceDeliverJobFor>c__AnonStorey2AF = new WorkGiver_ConstructDeliverResources.<ResourceDeliverJobFor>c__AnonStorey2AF();
				<ResourceDeliverJobFor>c__AnonStorey2AF.<>f__ref$686 = <ResourceDeliverJobFor>c__AnonStorey2AE;
				<ResourceDeliverJobFor>c__AnonStorey2AF.need = list[i];
				if (!pawn.Map.itemAvailability.ThingsAvailableAnywhere(<ResourceDeliverJobFor>c__AnonStorey2AF.need, pawn))
				{
					flag = true;
					break;
				}
				WorkGiver_ConstructDeliverResources.<ResourceDeliverJobFor>c__AnonStorey2AF arg_EE_0 = <ResourceDeliverJobFor>c__AnonStorey2AF;
				Predicate<Thing> validator = (Thing r) => WorkGiver_ConstructDeliverResources.ResourceValidator(<ResourceDeliverJobFor>c__AnonStorey2AF.<>f__ref$686.pawn, <ResourceDeliverJobFor>c__AnonStorey2AF.need, r);
				arg_EE_0.foundRes = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(<ResourceDeliverJobFor>c__AnonStorey2AF.need.thingDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				if (<ResourceDeliverJobFor>c__AnonStorey2AF.foundRes != null)
				{
					int resTotalAvailable;
					this.FindAvailableNearbyResources(<ResourceDeliverJobFor>c__AnonStorey2AF.foundRes, pawn, out resTotalAvailable);
					int num;
					Job job;
					HashSet<Thing> hashSet = this.FindNearbyNeeders(pawn, <ResourceDeliverJobFor>c__AnonStorey2AF.need, c, resTotalAvailable, canRemoveExistingFloorUnderNearbyNeeders, out num, out job);
					if (job != null)
					{
						return job;
					}
					hashSet.Add((Thing)c);
					Thing thing = hashSet.MinBy((Thing nee) => IntVec3Utility.ManhattanDistanceFlat(<ResourceDeliverJobFor>c__AnonStorey2AF.foundRes.Position, nee.Position));
					hashSet.Remove(thing);
					int num2 = 0;
					int j = 0;
					do
					{
						num2 += WorkGiver_ConstructDeliverResources.resourcesAvailable[j].stackCount;
						j++;
					}
					while (num2 < num && j < WorkGiver_ConstructDeliverResources.resourcesAvailable.Count);
					WorkGiver_ConstructDeliverResources.resourcesAvailable.RemoveRange(j, WorkGiver_ConstructDeliverResources.resourcesAvailable.Count - j);
					WorkGiver_ConstructDeliverResources.resourcesAvailable.Remove(<ResourceDeliverJobFor>c__AnonStorey2AF.foundRes);
					Job job2 = new Job(JobDefOf.HaulToContainer);
					job2.targetA = <ResourceDeliverJobFor>c__AnonStorey2AF.foundRes;
					job2.targetQueueA = new List<LocalTargetInfo>();
					for (j = 0; j < WorkGiver_ConstructDeliverResources.resourcesAvailable.Count; j++)
					{
						job2.targetQueueA.Add(WorkGiver_ConstructDeliverResources.resourcesAvailable[j]);
					}
					job2.targetB = thing;
					if (hashSet.Count > 0)
					{
						job2.targetQueueB = new List<LocalTargetInfo>();
						foreach (Thing current in hashSet)
						{
							job2.targetQueueB.Add(current);
						}
					}
					job2.targetC = (Thing)c;
					job2.count = num;
					job2.haulMode = HaulMode.ToContainer;
					return job2;
				}
				else
				{
					flag = true;
					i++;
				}
			}
			if (flag)
			{
				JobFailReason.Is(WorkGiver_ConstructDeliverResources.MissingMaterialsTranslated);
			}
			return null;
		}

		private void FindAvailableNearbyResources(Thing firstFoundResource, Pawn pawn, out int resTotalAvailable)
		{
			int num = Mathf.Min(firstFoundResource.def.stackLimit, pawn.carryTracker.MaxStackSpaceEver(firstFoundResource.def));
			resTotalAvailable = 0;
			WorkGiver_ConstructDeliverResources.resourcesAvailable.Clear();
			WorkGiver_ConstructDeliverResources.resourcesAvailable.Add(firstFoundResource);
			resTotalAvailable += firstFoundResource.stackCount;
			if (resTotalAvailable < num)
			{
				foreach (Thing current in GenRadial.RadialDistinctThingsAround(firstFoundResource.Position, firstFoundResource.Map, 5f, false))
				{
					if (resTotalAvailable >= num)
					{
						break;
					}
					if (current.def == firstFoundResource.def)
					{
						if (GenAI.CanUseItemForWork(pawn, current))
						{
							WorkGiver_ConstructDeliverResources.resourcesAvailable.Add(current);
							resTotalAvailable += current.stackCount;
						}
					}
				}
			}
		}

		private HashSet<Thing> FindNearbyNeeders(Pawn pawn, ThingCountClass need, IConstructible c, int resTotalAvailable, bool canRemoveExistingFloorUnderNearbyNeeders, out int neededTotal, out Job jobToMakeNeederAvailable)
		{
			neededTotal = need.count;
			HashSet<Thing> hashSet = new HashSet<Thing>();
			Thing thing = (Thing)c;
			foreach (Thing current in GenRadial.RadialDistinctThingsAround(thing.Position, thing.Map, 8f, false))
			{
				if (neededTotal >= resTotalAvailable)
				{
					break;
				}
				if (this.IsNewValidNearbyNeeder(current, hashSet, c, pawn))
				{
					Blueprint blueprint = current as Blueprint;
					if (blueprint == null || !WorkGiver_ConstructDeliverResources.ShouldRemoveExistingFloorFirst(pawn, blueprint))
					{
						int num = GenConstruct.AmountNeededByOf((IConstructible)current, need.thingDef);
						if (num > 0)
						{
							hashSet.Add(current);
							neededTotal += num;
						}
					}
				}
			}
			Blueprint blueprint2 = c as Blueprint;
			if (blueprint2 != null && blueprint2.def.entityDefToBuild is TerrainDef && canRemoveExistingFloorUnderNearbyNeeders && neededTotal < resTotalAvailable)
			{
				foreach (Thing current2 in GenRadial.RadialDistinctThingsAround(thing.Position, thing.Map, 3f, false))
				{
					if (this.IsNewValidNearbyNeeder(current2, hashSet, c, pawn))
					{
						Blueprint blueprint3 = current2 as Blueprint;
						if (blueprint3 != null)
						{
							Job job = this.RemoveExistingFloorJob(pawn, blueprint3);
							if (job != null)
							{
								jobToMakeNeederAvailable = job;
								return hashSet;
							}
						}
					}
				}
			}
			jobToMakeNeederAvailable = null;
			return hashSet;
		}

		private bool IsNewValidNearbyNeeder(Thing t, HashSet<Thing> nearbyNeeders, IConstructible constructible, Pawn pawn)
		{
			return t is IConstructible && t != constructible && !(t is Blueprint_Install) && t.Faction == pawn.Faction && !t.IsForbidden(pawn) && !nearbyNeeders.Contains(t) && GenConstruct.CanConstruct(t, pawn, false);
		}

		private static bool ShouldRemoveExistingFloorFirst(Pawn pawn, Blueprint blue)
		{
			return blue.def.entityDefToBuild is TerrainDef && pawn.Map.terrainGrid.CanRemoveTopLayerAt(blue.Position);
		}

		protected Job RemoveExistingFloorJob(Pawn pawn, Blueprint blue)
		{
			if (!WorkGiver_ConstructDeliverResources.ShouldRemoveExistingFloorFirst(pawn, blue))
			{
				return null;
			}
			ReservationLayerDef floor = ReservationLayerDefOf.Floor;
			if (!pawn.CanReserve(blue.Position, 1, -1, floor, false))
			{
				return null;
			}
			return new Job(JobDefOf.RemoveFloor, blue.Position)
			{
				ignoreDesignations = true
			};
		}

		private Job InstallJob(Pawn pawn, Blueprint_Install install)
		{
			Thing miniToInstallOrBuildingToReinstall = install.MiniToInstallOrBuildingToReinstall;
			if (miniToInstallOrBuildingToReinstall.IsForbidden(pawn) || !pawn.CanReserveAndReach(miniToInstallOrBuildingToReinstall, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1, -1, null, false))
			{
				return null;
			}
			return new Job(JobDefOf.HaulToContainer)
			{
				targetA = miniToInstallOrBuildingToReinstall,
				targetB = install,
				count = 1,
				haulMode = HaulMode.ToContainer
			};
		}
	}
}
