using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Repair : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return ListerBuildingsRepairable.RepairableBuildings(pawn.Faction);
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			return ListerBuildingsRepairable.RepairableBuildings(pawn.Faction).Count == 0;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			if (pawn.Faction == Faction.OfPlayer && !Find.AreaHome[t.Position])
			{
				return false;
			}
			if (!t.def.useHitPoints || t.HitPoints == t.MaxHitPoints)
			{
				return false;
			}
			Building building = t as Building;
			return building != null && building.def.building.repairable && pawn.CanReserve(building, 1) && Find.DesignationManager.DesignationOn(building, DesignationDefOf.Deconstruct) == null && !building.IsBurning();
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			return new Job(JobDefOf.Repair, t);
		}
	}
}
