using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class SickPawnVisitUtility
	{
		public static Pawn FindRandomSickPawn(Pawn pawn, JoyCategory maxPatientJoy)
		{
			IEnumerable<Pawn> source = from x in pawn.Map.mapPawns.FreeColonistsSpawned
			where SickPawnVisitUtility.CanVisit(pawn, x, maxPatientJoy)
			select x;
			Pawn result;
			if (!source.TryRandomElementByWeight((Pawn x) => SickPawnVisitUtility.VisitChanceScore(pawn, x), out result))
			{
				return null;
			}
			return result;
		}

		public static bool CanVisit(Pawn pawn, Pawn sick, JoyCategory maxPatientJoy)
		{
			return sick.IsColonist && !sick.Dead && pawn != sick && sick.InBed() && sick.Awake() && !sick.IsForbidden(pawn) && sick.needs.joy != null && sick.needs.joy.CurCategory <= maxPatientJoy && InteractionUtility.CanReceiveInteraction(sick) && !sick.needs.food.Starving && sick.needs.rest.CurLevel > 0.33f && pawn.CanReserveAndReach(sick, PathEndMode.InteractionCell, Danger.None, 1) && !SickPawnVisitUtility.AboutToRecover(sick);
		}

		public static Thing FindChair(Pawn forPawn, Pawn nearPawn)
		{
			Predicate<Thing> validator = delegate(Thing x)
			{
				if (!x.def.building.isSittable)
				{
					return false;
				}
				if (x.IsForbidden(forPawn))
				{
					return false;
				}
				if (!GenSight.LineOfSight(x.Position, nearPawn.Position, nearPawn.Map, false))
				{
					return false;
				}
				if (!forPawn.CanReserve(x, 1))
				{
					return false;
				}
				if (x.def.rotatable)
				{
					float num = GenGeo.AngleDifferenceBetween(x.Rotation.AsAngle, (nearPawn.Position - x.Position).AngleFlat);
					if (num > 95f)
					{
						return false;
					}
				}
				return true;
			};
			return GenClosest.ClosestThingReachable(nearPawn.Position, nearPawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(forPawn, Danger.Deadly, TraverseMode.ByPawn, false), 2.2f, validator, null, 5, false);
		}

		private static bool AboutToRecover(Pawn pawn)
		{
			if (pawn.Downed)
			{
				return false;
			}
			if (!HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn) && !HealthAIUtility.ShouldSeekMedicalRest(pawn))
			{
				return true;
			}
			if (HealthAIUtility.HasTendedImmunizableNonInjuryNonMissingPartHediff(pawn))
			{
				return false;
			}
			float num = 0f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Injury hediff_Injury = hediffs[i] as Hediff_Injury;
				if (hediff_Injury != null && (hediff_Injury.CanHealFromTending() || hediff_Injury.CanHealNaturally() || hediff_Injury.BleedRate > 0.0001f))
				{
					num += hediff_Injury.Severity;
				}
			}
			return num < 8f * pawn.RaceProps.baseHealthScale;
		}

		private static float VisitChanceScore(Pawn pawn, Pawn sick)
		{
			float num = GenMath.LerpDouble(-100f, 100f, 0.05f, 2f, (float)pawn.relations.OpinionOf(sick));
			float lengthHorizontal = (pawn.Position - sick.Position).LengthHorizontal;
			float num2 = Mathf.Clamp(GenMath.LerpDouble(0f, 150f, 1f, 0.2f, lengthHorizontal), 0.2f, 1f);
			return num * num2;
		}
	}
}
