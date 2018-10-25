using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_ConstructDeliverResources : WorkGiver_Scanner
	{
		private static List<Thing> resourcesAvailable = new List<Thing>();

		private const float MultiPickupRadius = 5f;

		private const float NearbyConstructScanRadius = 8f;

		private static string MissingMaterialsTranslated;

		private static string ForbiddenLowerTranslated;

		private static string NoPathTranslated;

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public static void ResetStaticData()
		{
			WorkGiver_ConstructDeliverResources.MissingMaterialsTranslated = "MissingMaterials".Translate();
			WorkGiver_ConstructDeliverResources.ForbiddenLowerTranslated = "ForbiddenLower".Translate();
			WorkGiver_ConstructDeliverResources.NoPathTranslated = "NoPath".Translate();
		}

		private static bool ResourceValidator(Pawn pawn, ThingDefCountClass need, Thing th)
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
			ThingDefCountClass thingDefCountClass = null;
			List<ThingDefCountClass> list = c.MaterialsNeeded();
			int count = list.Count;
			int i = 0;
			while (i < count)
			{
				ThingDefCountClass need = list[i];
				if (!pawn.Map.itemAvailability.ThingsAvailableAnywhere(need, pawn))
				{
					flag = true;
					thingDefCountClass = need;
					break;
				}
				Thing foundRes = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(need.thingDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, (Thing r) => WorkGiver_ConstructDeliverResources.ResourceValidator(pawn, need, r), null, 0, -1, false, RegionType.Set_Passable, false);
				if (foundRes != null)
				{
					int resTotalAvailable;
					this.FindAvailableNearbyResources(foundRes, pawn, out resTotalAvailable);
					int num;
					Job job;
					HashSet<Thing> hashSet = this.FindNearbyNeeders(pawn, need, c, resTotalAvailable, canRemoveExistingFloorUnderNearbyNeeders, out num, out job);
					if (job != null)
					{
						return job;
					}
					hashSet.Add((Thing)c);
					Thing thing = hashSet.MinBy((Thing nee) => IntVec3Utility.ManhattanDistanceFlat(foundRes.Position, nee.Position));
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
					WorkGiver_ConstructDeliverResources.resourcesAvailable.Remove(foundRes);
					Job job2 = new Job(JobDefOf.HaulToContainer);
					job2.targetA = foundRes;
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
					thingDefCountClass = need;
					i++;
				}
			}
			if (flag)
			{
				JobFailReason.Is(string.Format("{0}: {1}", WorkGiver_ConstructDeliverResources.MissingMaterialsTranslated, thingDefCountClass.thingDef.label), null);
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

		private HashSet<Thing> FindNearbyNeeders(Pawn pawn, ThingDefCountClass need, IConstructible c, int resTotalAvailable, bool canRemoveExistingFloorUnderNearbyNeeders, out int neededTotal, out Job jobToMakeNeederAvailable)
		{
			neededTotal = need.count;
			HashSet<Thing> hashSet = new HashSet<Thing>();
			Thing thing = (Thing)c;
			foreach (Thing current in GenRadial.RadialDistinctThingsAround(thing.Position, thing.Map, 8f, true))
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
			return t is IConstructible && t != constructible && !(t is Blueprint_Install) && t.Faction == pawn.Faction && !t.IsForbidden(pawn) && !nearbyNeeders.Contains(t) && GenConstruct.CanConstruct(t, pawn, false, false);
		}

		protected static bool ShouldRemoveExistingFloorFirst(Pawn pawn, Blueprint blue)
		{
			return blue.def.entityDefToBuild is TerrainDef && pawn.Map.terrainGrid.CanRemoveTopLayerAt(blue.Position);
		}

		protected Job RemoveExistingFloorJob(Pawn pawn, Blueprint blue)
		{
			if (!WorkGiver_ConstructDeliverResources.ShouldRemoveExistingFloorFirst(pawn, blue))
			{
				return null;
			}
			LocalTargetInfo target = blue.Position;
			ReservationLayerDef floor = ReservationLayerDefOf.Floor;
			if (!pawn.CanReserve(target, 1, -1, floor, false))
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
			if (miniToInstallOrBuildingToReinstall.IsForbidden(pawn))
			{
				JobFailReason.Is(WorkGiver_ConstructDeliverResources.ForbiddenLowerTranslated, null);
				return null;
			}
			if (!pawn.CanReach(miniToInstallOrBuildingToReinstall, PathEndMode.ClosestTouch, pawn.NormalMaxDanger(), false, TraverseMode.ByPawn))
			{
				JobFailReason.Is(WorkGiver_ConstructDeliverResources.NoPathTranslated, null);
				return null;
			}
			if (!pawn.CanReserve(miniToInstallOrBuildingToReinstall, 1, -1, null, false))
			{
				Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(miniToInstallOrBuildingToReinstall, pawn);
				if (pawn2 != null)
				{
					JobFailReason.Is("ReservedBy".Translate(pawn2.LabelShort, pawn2), null);
				}
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
