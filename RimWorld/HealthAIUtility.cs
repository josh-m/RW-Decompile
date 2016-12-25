using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class HealthAIUtility
	{
		public static bool ShouldSeekMedicalRestUrgent(Pawn pawn)
		{
			return pawn.Downed || pawn.health.HasHediffsNeedingTend(false) || HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn);
		}

		public static bool ShouldSeekMedicalRest(Pawn pawn)
		{
			return HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn) || pawn.health.hediffSet.HasTendedAndHealingInjury() || HealthAIUtility.HasTendedImmunizableNonInjuryNonMissingPartHediff(pawn);
		}

		public static bool ShouldBeTendedNowUrgent(Pawn pawn)
		{
			return HealthAIUtility.ShouldBeTendedNow(pawn) && HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) < 15000;
		}

		public static bool ShouldBeTendedNow(Pawn pawn)
		{
			return pawn.playerSettings != null && HealthAIUtility.ShouldEverReceiveMedicalCare(pawn) && pawn.health.HasHediffsNeedingTendByColony(false);
		}

		public static bool ShouldEverReceiveMedicalCare(Pawn pawn)
		{
			return (pawn.playerSettings == null || pawn.playerSettings.medCare != MedicalCareCategory.NoCare) && (pawn.guest == null || pawn.guest.interactionMode != PrisonerInteractionMode.Execution) && pawn.Map.designationManager.DesignationOn(pawn, DesignationDefOf.Slaughter) == null;
		}

		public static bool ShouldHaveSurgeryDoneNow(Pawn pawn)
		{
			return pawn.health.surgeryBills.AnyShouldDoNow;
		}

		public static bool HasTendedImmunizableNonInjuryNonMissingPartHediff(Pawn pawn)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (!(hediffs[i] is Hediff_Injury))
				{
					if (!(hediffs[i] is Hediff_MissingPart))
					{
						if (hediffs[i].Visible)
						{
							if (hediffs[i].IsTended())
							{
								if (hediffs[i].def.PossibleToDevelopImmunity())
								{
									return true;
								}
							}
						}
					}
				}
			}
			return false;
		}

		public static Thing FindBestMedicine(Pawn healer, Pawn patient)
		{
			if (patient.playerSettings == null || patient.playerSettings.medCare <= MedicalCareCategory.NoMeds)
			{
				return null;
			}
			Predicate<Thing> predicate = (Thing m) => !m.IsForbidden(healer) && patient.playerSettings.medCare.AllowsMedicine(m.def) && healer.CanReserve(m, 1);
			Func<Thing, float> priorityGetter = (Thing t) => t.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThing_Global_Reachable(patient.Position, patient.Map, patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine), PathEndMode.ClosestTouch, TraverseParms.For(healer, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, priorityGetter);
		}
	}
}
