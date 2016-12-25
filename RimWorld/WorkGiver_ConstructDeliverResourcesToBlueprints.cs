using System;
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

		public override Job JobOnThing(Pawn pawn, Thing t)
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
					if (pawn.CanReserveAndReach(thing, PathEndMode.ClosestTouch, pawn.NormalMaxDanger(), 1))
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
			if (!GenConstruct.CanConstruct(blueprint, pawn))
			{
				return null;
			}
			Job job = this.DeconstructExistingEdificeJob(pawn, blueprint);
			if (job != null)
			{
				return job;
			}
			Job job2 = base.ResourceDeliverJobFor(pawn, blueprint);
			if (job2 != null)
			{
				return job2;
			}
			Job job3 = this.NoCostFrameMakeJobFor(pawn, blueprint);
			if (job3 != null)
			{
				return job3;
			}
			return null;
		}

		private Job DeconstructExistingEdificeJob(Pawn pawn, Blueprint blue)
		{
			if (!blue.def.entityDefToBuild.IsEdifice())
			{
				return null;
			}
			Thing thing = GenConstruct.MiniToInstallOrBuildingToReinstall(blue);
			Thing thing2 = null;
			CellRect cellRect = blue.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				int j = cellRect.minX;
				while (j <= cellRect.maxX)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					thing2 = c.GetEdifice(pawn.Map);
					if (thing2 == thing)
					{
						thing2 = null;
					}
					if (thing2 != null)
					{
						ThingDef thingDef = blue.def.entityDefToBuild as ThingDef;
						if (thingDef != null && thingDef.building.canPlaceOverWall && thing2.def == ThingDefOf.Wall)
						{
							return null;
						}
						break;
					}
					else
					{
						j++;
					}
				}
				if (thing2 != null)
				{
					break;
				}
			}
			if (thing2 == null || !pawn.CanReserve(thing2, 1))
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
