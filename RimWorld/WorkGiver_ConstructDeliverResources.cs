using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_ConstructDeliverResources : WorkGiver_Scanner
	{
		private const float NearbyConstructScanRadius = 8f;

		private static List<Thing> resourcesAvailable = new List<Thing>();

		private static string MissingMaterialsTranslated = null;

		public WorkGiver_ConstructDeliverResources()
		{
			if (WorkGiver_ConstructDeliverResources.MissingMaterialsTranslated == null)
			{
				WorkGiver_ConstructDeliverResources.MissingMaterialsTranslated = "MissingMaterials".Translate();
			}
		}

		private static bool ResourceValidator(Pawn pawn, ThingCountClass need, Thing th)
		{
			return th.def == need.thingDef && !th.IsForbidden(pawn) && pawn.CanReserve(th, 1);
		}

		protected Job ResourceDeliverJobFor(Pawn pawn, IConstructible c)
		{
			Blueprint_Install blueprint_Install = c as Blueprint_Install;
			if (blueprint_Install == null)
			{
				bool flag = false;
				List<ThingCountClass> list = c.MaterialsNeeded();
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					WorkGiver_ConstructDeliverResources.<ResourceDeliverJobFor>c__AnonStorey27D <ResourceDeliverJobFor>c__AnonStorey27D = new WorkGiver_ConstructDeliverResources.<ResourceDeliverJobFor>c__AnonStorey27D();
					<ResourceDeliverJobFor>c__AnonStorey27D.<>f__ref$636 = <ResourceDeliverJobFor>c__AnonStorey27C;
					<ResourceDeliverJobFor>c__AnonStorey27D.need = list[i];
					if (!pawn.Map.itemAvailability.ThingsAvailableAnywhere(<ResourceDeliverJobFor>c__AnonStorey27D.need, pawn))
					{
						flag = true;
						break;
					}
					WorkGiver_ConstructDeliverResources.<ResourceDeliverJobFor>c__AnonStorey27D arg_158_0 = <ResourceDeliverJobFor>c__AnonStorey27D;
					Predicate<Thing> validator = (Thing r) => WorkGiver_ConstructDeliverResources.ResourceValidator(<ResourceDeliverJobFor>c__AnonStorey27D.<>f__ref$636.pawn, <ResourceDeliverJobFor>c__AnonStorey27D.need, r);
					arg_158_0.foundRes = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(<ResourceDeliverJobFor>c__AnonStorey27D.need.thingDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
					if (<ResourceDeliverJobFor>c__AnonStorey27D.foundRes != null)
					{
						int num = Mathf.Min(<ResourceDeliverJobFor>c__AnonStorey27D.foundRes.def.stackLimit, pawn.carryTracker.MaxStackSpaceEver(<ResourceDeliverJobFor>c__AnonStorey27D.foundRes.def));
						int num2 = 0;
						WorkGiver_ConstructDeliverResources.resourcesAvailable.Clear();
						WorkGiver_ConstructDeliverResources.resourcesAvailable.Add(<ResourceDeliverJobFor>c__AnonStorey27D.foundRes);
						num2 += <ResourceDeliverJobFor>c__AnonStorey27D.foundRes.stackCount;
						if (num2 < num)
						{
							foreach (Thing current in GenRadial.RadialDistinctThingsAround(<ResourceDeliverJobFor>c__AnonStorey27D.foundRes.Position, <ResourceDeliverJobFor>c__AnonStorey27D.foundRes.Map, 5f, false))
							{
								if (num2 >= num)
								{
									break;
								}
								if (current.def == <ResourceDeliverJobFor>c__AnonStorey27D.foundRes.def)
								{
									if (GenAI.CanUseItemForWork(pawn, current))
									{
										WorkGiver_ConstructDeliverResources.resourcesAvailable.Add(current);
										num2 += current.stackCount;
									}
								}
							}
						}
						int num3 = <ResourceDeliverJobFor>c__AnonStorey27D.need.count;
						HashSet<Thing> hashSet = new HashSet<Thing>();
						Thing thing = (Thing)c;
						foreach (Thing current2 in GenRadial.RadialDistinctThingsAround(thing.Position, thing.Map, 8f, false))
						{
							if (num3 >= num2)
							{
								break;
							}
							IConstructible constructible = current2 as IConstructible;
							if (constructible != null)
							{
								if (constructible != c)
								{
									if (!(constructible is Blueprint_Install))
									{
										if (current2.Faction == pawn.Faction)
										{
											if (!current2.IsForbidden(pawn))
											{
												if (current2 != c && !hashSet.Contains(current2))
												{
													if (GenConstruct.CanConstruct(current2, pawn))
													{
														int num4 = GenConstruct.AmountNeededByOf(constructible, <ResourceDeliverJobFor>c__AnonStorey27D.foundRes.def);
														if (num4 != 0)
														{
															hashSet.Add(current2);
															num3 += num4;
														}
													}
												}
											}
										}
									}
								}
							}
						}
						hashSet.Add((Thing)c);
						Thing thing2 = hashSet.MinBy((Thing nee) => IntVec3Utility.ManhattanDistanceFlat(<ResourceDeliverJobFor>c__AnonStorey27D.foundRes.Position, nee.Position));
						hashSet.Remove(thing2);
						int num5 = 0;
						int j = 0;
						do
						{
							num5 += WorkGiver_ConstructDeliverResources.resourcesAvailable[j].stackCount;
							j++;
						}
						while (num5 < num3 && j < WorkGiver_ConstructDeliverResources.resourcesAvailable.Count);
						WorkGiver_ConstructDeliverResources.resourcesAvailable.RemoveRange(j, WorkGiver_ConstructDeliverResources.resourcesAvailable.Count - j);
						WorkGiver_ConstructDeliverResources.resourcesAvailable.Remove(<ResourceDeliverJobFor>c__AnonStorey27D.foundRes);
						Job job = new Job(JobDefOf.HaulToContainer);
						job.targetA = <ResourceDeliverJobFor>c__AnonStorey27D.foundRes;
						job.targetQueueA = new List<LocalTargetInfo>();
						for (j = 0; j < WorkGiver_ConstructDeliverResources.resourcesAvailable.Count; j++)
						{
							job.targetQueueA.Add(WorkGiver_ConstructDeliverResources.resourcesAvailable[j]);
						}
						job.targetB = thing2;
						if (hashSet.Count > 0)
						{
							job.targetQueueB = new List<LocalTargetInfo>();
							foreach (Thing current3 in hashSet)
							{
								job.targetQueueB.Add(current3);
							}
						}
						job.count = num3;
						job.haulMode = HaulMode.ToContainer;
						return job;
					}
					flag = true;
				}
				if (flag)
				{
					JobFailReason.Is(WorkGiver_ConstructDeliverResources.MissingMaterialsTranslated);
				}
				return null;
			}
			if (blueprint_Install.MiniToInstallOrBuildingToReinstall.IsForbidden(pawn) || !pawn.CanReserveAndReach(blueprint_Install.MiniToInstallOrBuildingToReinstall, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1))
			{
				return null;
			}
			return new Job(JobDefOf.HaulToContainer)
			{
				targetA = blueprint_Install.MiniToInstallOrBuildingToReinstall,
				targetB = blueprint_Install,
				count = 1,
				haulMode = HaulMode.ToContainer
			};
		}
	}
}
