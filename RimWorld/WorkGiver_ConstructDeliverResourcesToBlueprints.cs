using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ConstructDeliverResourcesToBlueprints : WorkGiver_ConstructDeliverResources
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Blueprint);
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.Faction != pawn.Faction)
			{
				return null;
			}
			Blueprint blueprint = t as Blueprint;
			if (blueprint == null)
			{
				return null;
			}
			Thing thingToIgnore = GenConstruct.MiniToInstallOrBuildingToReinstall(blueprint);
			Thing thing = blueprint.FirstBlockingThing(pawn, thingToIgnore, false);
			if (thing != null)
			{
				if (thing.def.category == ThingCategory.Plant)
				{
					if (pawn.CanReserveAndReach(thing, PathEndMode.ClosestTouch, pawn.NormalMaxDanger(), 1, -1, null, forced))
					{
						return new Job(JobDefOf.CutPlant, thing);
					}
				}
				else if (thing.def.category == ThingCategory.Item)
				{
					if (thing.def.EverHaulable)
					{
						return HaulAIUtility.HaulAsideJobFor(pawn, thing);
					}
					Log.ErrorOnce(string.Concat(new object[]
					{
						"Never haulable ",
						thing,
						" blocking ",
						t,
						" at ",
						t.Position
					}), 6429262);
				}
				return null;
			}
			if (!GenConstruct.CanConstruct(blueprint, pawn, forced))
			{
				return null;
			}
			Job job = this.DeconstructExistingBuildingJob(pawn, blueprint);
			if (job != null)
			{
				return job;
			}
			Job job2 = base.RemoveExistingFloorJob(pawn, blueprint);
			if (job2 != null)
			{
				return job2;
			}
			Job job3 = base.ResourceDeliverJobFor(pawn, blueprint, true);
			if (job3 != null)
			{
				return job3;
			}
			Job job4 = this.NoCostFrameMakeJobFor(pawn, blueprint);
			if (job4 != null)
			{
				return job4;
			}
			return null;
		}

		private Job DeconstructExistingBuildingJob(Pawn pawn, Blueprint blue)
		{
			Thing thing = GenConstruct.MiniToInstallOrBuildingToReinstall(blue);
			Thing thing2 = null;
			CellRect cellRect = blue.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					List<Thing> thingList = c.GetThingList(pawn.Map);
					for (int k = 0; k < thingList.Count; k++)
					{
						if (thingList[k].def.category == ThingCategory.Building && thingList[k] != thing && GenSpawn.SpawningWipes(blue.def.entityDefToBuild, thingList[k].def))
						{
							thing2 = thingList[k];
							break;
						}
					}
					if (thing2 != null)
					{
						break;
					}
				}
				if (thing2 != null)
				{
					break;
				}
			}
			if (thing2 == null || !pawn.CanReserve(thing2, 1, -1, null, false))
			{
				return null;
			}
			return new Job(JobDefOf.Deconstruct, thing2)
			{
				ignoreDesignations = true
			};
		}

		private Job NoCostFrameMakeJobFor(Pawn pawn, IConstructible c)
		{
			if (c is Blueprint_Install)
			{
				return null;
			}
			if (c is Blueprint && c.MaterialsNeeded().Count == 0)
			{
				return new Job(JobDefOf.PlaceNoCostFrame)
				{
					targetA = (Thing)c
				};
			}
			return null;
		}
	}
}
