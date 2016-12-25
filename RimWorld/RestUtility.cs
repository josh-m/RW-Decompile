using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class RestUtility
	{
		private static List<ThingDef> bedDefsBestToWorst_RestEffectiveness;

		private static List<ThingDef> bedDefsBestToWorst_Medical;

		public static List<ThingDef> AllBedDefBestToWorst
		{
			get
			{
				return RestUtility.bedDefsBestToWorst_RestEffectiveness;
			}
		}

		public static void Reset()
		{
			RestUtility.bedDefsBestToWorst_RestEffectiveness = (from d in DefDatabase<ThingDef>.AllDefs
			where d.IsBed
			orderby d.building.bed_maxBodySize, d.GetStatValueAbstract(StatDefOf.BedRestEffectiveness, null) descending
			select d).ToList<ThingDef>();
			RestUtility.bedDefsBestToWorst_Medical = (from d in DefDatabase<ThingDef>.AllDefs
			where d.IsBed
			orderby d.building.bed_maxBodySize, d.GetStatValueAbstract(StatDefOf.MedicalTendQualityOffset, null) descending, d.GetStatValueAbstract(StatDefOf.BedRestEffectiveness, null) descending
			select d).ToList<ThingDef>();
		}

		public static Building_Bed FindBedFor(Pawn p)
		{
			return RestUtility.FindBedFor(p, p, p.IsPrisonerOfColony, true, false);
		}

		public static Building_Bed FindBedFor(Pawn sleeper, Pawn traveler, bool sleeperWillBePrisoner, bool checkSocialProperness, bool forceCheckMedBed = false)
		{
			Predicate<Thing> bedValidator = delegate(Thing t)
			{
				Building_Bed building_Bed3 = (Building_Bed)t;
				if (!traveler.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Some, building_Bed3.SleepingSlotsCount))
				{
					return false;
				}
				if (!RestUtility.CanUseBedEver(sleeper, building_Bed3.def))
				{
					return false;
				}
				if (!building_Bed3.AnyUnoccupiedSleepingSlot && (!sleeper.InBed() || sleeper.CurrentBed() != building_Bed3))
				{
					return false;
				}
				if (sleeperWillBePrisoner)
				{
					if (!building_Bed3.ForPrisoners)
					{
						return false;
					}
					if (!building_Bed3.Position.IsInPrisonCell(building_Bed3.Map))
					{
						return false;
					}
				}
				else
				{
					if (building_Bed3.Faction != traveler.Faction)
					{
						return false;
					}
					if (building_Bed3.ForPrisoners)
					{
						return false;
					}
				}
				if (building_Bed3.Medical)
				{
					if (!HealthAIUtility.ShouldEverReceiveMedicalCare(sleeper))
					{
						return false;
					}
					if (!HealthAIUtility.ShouldSeekMedicalRest(sleeper))
					{
						return false;
					}
					if (!building_Bed3.AnyUnoccupiedSleepingSlot && (!sleeper.InBed() || sleeper.CurrentBed() != building_Bed3))
					{
						return false;
					}
				}
				else if (building_Bed3.owners.Any<Pawn>() && !building_Bed3.owners.Contains(sleeper))
				{
					if (building_Bed3.owners.Find((Pawn x) => LovePartnerRelationUtility.LovePartnerRelationExists(sleeper, x)) == null || sleeper.IsPrisoner || sleeperWillBePrisoner)
					{
						return false;
					}
					if (!building_Bed3.AnyUnownedSleepingSlot)
					{
						return false;
					}
				}
				return (!checkSocialProperness || building_Bed3.IsSociallyProper(sleeper, sleeperWillBePrisoner, false)) && !building_Bed3.IsForbidden(traveler) && !building_Bed3.IsBurning();
			};
			if (forceCheckMedBed || HealthAIUtility.ShouldSeekMedicalRest(sleeper))
			{
				if (sleeper.InBed() && sleeper.CurrentBed().Medical && bedValidator(sleeper.CurrentBed()))
				{
					return sleeper.CurrentBed();
				}
				for (int i = 0; i < RestUtility.bedDefsBestToWorst_Medical.Count; i++)
				{
					Predicate<Thing> validator = (Thing b) => bedValidator(b) && ((Building_Bed)b).Medical;
					Building_Bed building_Bed = (Building_Bed)GenClosest.ClosestThingReachable(sleeper.Position, sleeper.Map, ThingRequest.ForDef(RestUtility.bedDefsBestToWorst_Medical[i]), PathEndMode.OnCell, TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
					if (building_Bed != null)
					{
						return building_Bed;
					}
				}
			}
			if (sleeper.ownership != null && sleeper.ownership.OwnedBed != null && bedValidator(sleeper.ownership.OwnedBed))
			{
				return sleeper.ownership.OwnedBed;
			}
			DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(sleeper, false);
			if (directPawnRelation != null)
			{
				Building_Bed ownedBed = directPawnRelation.otherPawn.ownership.OwnedBed;
				if (ownedBed != null && bedValidator(ownedBed))
				{
					return ownedBed;
				}
			}
			for (int j = 0; j < RestUtility.bedDefsBestToWorst_RestEffectiveness.Count; j++)
			{
				ThingDef thingDef = RestUtility.bedDefsBestToWorst_RestEffectiveness[j];
				if (RestUtility.CanUseBedEver(sleeper, thingDef))
				{
					Predicate<Thing> validator = (Thing b) => bedValidator(b) && !((Building_Bed)b).Medical;
					Building_Bed building_Bed2 = (Building_Bed)GenClosest.ClosestThingReachable(sleeper.Position, sleeper.Map, ThingRequest.ForDef(thingDef), PathEndMode.OnCell, TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
					if (building_Bed2 != null)
					{
						if (sleeper.ownership != null)
						{
							sleeper.ownership.UnclaimBed();
						}
						return building_Bed2;
					}
				}
			}
			return null;
		}

		public static Building_Bed FindPatientBedFor(Pawn pawn)
		{
			Predicate<Thing> predicate = delegate(Thing t)
			{
				Building_Bed building_Bed2 = t as Building_Bed;
				return building_Bed2 != null && RestUtility.CanUseBedEver(pawn, building_Bed2.def) && (building_Bed2.Medical || !building_Bed2.def.building.bed_humanlike) && (building_Bed2.ForPrisoners == pawn.IsPrisoner && (building_Bed2.AnyUnoccupiedSleepingSlot || (pawn.InBed() && pawn.CurrentBed() == building_Bed2))) && !building_Bed2.IsBurning() && !building_Bed2.IsForbidden(pawn) && pawn.CanReserveAndReach(building_Bed2, PathEndMode.OnCell, Danger.Some, building_Bed2.SleepingSlotsCount);
			};
			if (pawn.InBed() && predicate(pawn.CurrentBed()))
			{
				return pawn.CurrentBed();
			}
			Predicate<Thing> validator = predicate;
			Building_Bed building_Bed = (Building_Bed)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
			if (building_Bed != null)
			{
				return building_Bed;
			}
			return RestUtility.FindBedFor(pawn);
		}

		public static IntVec3 GetBedSleepingSlotPosFor(Pawn pawn, Building_Bed bed)
		{
			for (int i = 0; i < bed.owners.Count; i++)
			{
				if (bed.owners[i] == pawn)
				{
					return bed.GetSleepingSlotPos(i);
				}
			}
			for (int j = 0; j < bed.SleepingSlotsCount; j++)
			{
				Pawn curOccupant = bed.GetCurOccupant(j);
				if ((j >= bed.owners.Count || bed.owners[j] == null) && curOccupant == pawn)
				{
					return bed.GetSleepingSlotPos(j);
				}
			}
			for (int k = 0; k < bed.SleepingSlotsCount; k++)
			{
				Pawn curOccupant2 = bed.GetCurOccupant(k);
				if ((k >= bed.owners.Count || bed.owners[k] == null) && curOccupant2 == null)
				{
					return bed.GetSleepingSlotPos(k);
				}
			}
			Log.Error("Could not find good sleeping slot position for " + pawn + ". Perhaps AnyUnoccupiedSleepingSlot check is missing somewhere.");
			return bed.GetSleepingSlotPos(0);
		}

		public static bool CanUseBedEver(Pawn p, ThingDef bedDef)
		{
			return p.BodySize <= bedDef.building.bed_maxBodySize && p.RaceProps.Humanlike == bedDef.building.bed_humanlike;
		}

		public static bool TimetablePreventsLayDown(Pawn pawn)
		{
			return pawn.timetable != null && !pawn.timetable.CurrentAssignment.allowRest && pawn.needs.rest.CurLevel >= 0.2f;
		}

		public static bool DisturbancePreventsLyingDown(Pawn pawn)
		{
			return Find.TickManager.TicksGame - pawn.mindState.lastDisturbanceTick < 400;
		}

		public static float PawnHealthRestEffectivenessFactor(Pawn pawn)
		{
			return pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.BloodPumping) * pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Metabolism) * pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Breathing);
		}

		public static bool Awake(this Pawn p)
		{
			return p.health.capacities.CanBeAwake && (!p.Spawned || p.CurJob == null || !p.jobs.curDriver.asleep);
		}

		public static Building_Bed CurrentBed(this Pawn p)
		{
			if (p.CurJob == null || !p.jobs.curDriver.layingDown)
			{
				return null;
			}
			Building_Bed layingDownBed = p.jobs.curDriver.layingDownBed;
			if (layingDownBed == null)
			{
				return null;
			}
			for (int i = 0; i < layingDownBed.SleepingSlotsCount; i++)
			{
				if (layingDownBed.GetCurOccupant(i) == p)
				{
					return layingDownBed;
				}
			}
			return null;
		}

		public static bool InBed(this Pawn p)
		{
			return p.CurrentBed() != null;
		}
	}
}
