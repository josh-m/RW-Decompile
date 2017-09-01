using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class TendUtility
	{
		public const float NoMedicinePotency = 0.3f;

		public const float NoDoctorTendQuality = 0.75f;

		public const float SelfTendQualityFactor = 0.7f;

		private const float ChanceToDevelopBondRelationOnTended = 0.004f;

		private static List<Hediff> tmpHediffsToTend = new List<Hediff>();

		private static List<Hediff> tmpHediffs = new List<Hediff>();

		private static List<Pair<Hediff, float>> tmpHediffsWithTendPriority = new List<Pair<Hediff, float>>();

		public static void DoTend(Pawn doctor, Pawn patient, Medicine medicine)
		{
			if (!patient.health.HasHediffsNeedingTend(false))
			{
				return;
			}
			if (medicine != null && medicine.Destroyed)
			{
				Log.Warning("Tried to use destroyed medicine.");
				medicine = null;
			}
			float num = (medicine == null) ? 0.3f : medicine.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
			float num2;
			if (doctor != null)
			{
				num2 = doctor.GetStatValue(StatDefOf.MedicalTendQuality, true);
			}
			else
			{
				num2 = 0.75f;
			}
			num2 *= num;
			Building_Bed building_Bed = patient.CurrentBed();
			if (building_Bed != null)
			{
				num2 += building_Bed.GetStatValue(StatDefOf.MedicalTendQualityOffset, true);
			}
			if (doctor == patient)
			{
				num2 *= 0.7f;
			}
			num2 = Mathf.Clamp01(num2);
			TendUtility.GetOptimalHediffsToTendWithSingleTreatment(patient, medicine != null, TendUtility.tmpHediffsToTend, null);
			for (int i = 0; i < TendUtility.tmpHediffsToTend.Count; i++)
			{
				TendUtility.tmpHediffsToTend[i].Tended(num2, i);
			}
			if (doctor != null && doctor.Faction != null && patient.HostFaction == null && patient.Faction != null && patient.Faction != doctor.Faction)
			{
				patient.Faction.AffectGoodwillWith(doctor.Faction, 0.3f);
			}
			if (doctor != null && doctor.RaceProps.Humanlike && patient.RaceProps.Animal && RelationsUtility.TryDevelopBondRelation(doctor, patient, 0.004f) && doctor.Faction != null && doctor.Faction != patient.Faction)
			{
				InteractionWorker_RecruitAttempt.DoRecruit(doctor, patient, 1f, false);
			}
			patient.records.Increment(RecordDefOf.TimesTendedTo);
			if (doctor != null)
			{
				doctor.records.Increment(RecordDefOf.TimesTendedOther);
			}
			if (medicine != null)
			{
				if ((patient.Spawned || (doctor != null && doctor.Spawned)) && num > ThingDefOf.Medicine.GetStatValueAbstract(StatDefOf.MedicalPotency, null))
				{
					SoundDefOf.TechMedicineUsed.PlayOneShot(new TargetInfo(patient.Position, patient.Map, false));
				}
				if (medicine.stackCount > 1)
				{
					medicine.stackCount--;
				}
				else if (!medicine.Destroyed)
				{
					medicine.Destroy(DestroyMode.Vanish);
				}
			}
		}

		public static void GetOptimalHediffsToTendWithSingleTreatment(Pawn patient, bool usingMedicine, List<Hediff> outHediffsToTend, List<Hediff> tendableHediffsInTendPriorityOrder = null)
		{
			outHediffsToTend.Clear();
			TendUtility.tmpHediffs.Clear();
			if (tendableHediffsInTendPriorityOrder != null)
			{
				TendUtility.tmpHediffs.AddRange(tendableHediffsInTendPriorityOrder);
			}
			else
			{
				List<Hediff> hediffs = patient.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					if (hediffs[i].TendableNow)
					{
						TendUtility.tmpHediffs.Add(hediffs[i]);
					}
				}
				TendUtility.SortByTendPriority(TendUtility.tmpHediffs);
			}
			if (!TendUtility.tmpHediffs.Any<Hediff>())
			{
				return;
			}
			Hediff hediff = TendUtility.tmpHediffs[0];
			outHediffsToTend.Add(hediff);
			HediffCompProperties_TendDuration hediffCompProperties_TendDuration = hediff.def.CompProps<HediffCompProperties_TendDuration>();
			if (hediffCompProperties_TendDuration != null && hediffCompProperties_TendDuration.tendAllAtOnce)
			{
				for (int j = 0; j < TendUtility.tmpHediffs.Count; j++)
				{
					if (TendUtility.tmpHediffs[j] != hediff && TendUtility.tmpHediffs[j].def == hediff.def)
					{
						outHediffsToTend.Add(TendUtility.tmpHediffs[j]);
					}
				}
			}
			else if (hediff is Hediff_Injury && usingMedicine)
			{
				float num = hediff.Severity;
				for (int k = 0; k < TendUtility.tmpHediffs.Count; k++)
				{
					if (TendUtility.tmpHediffs[k] != hediff)
					{
						Hediff_Injury hediff_Injury = TendUtility.tmpHediffs[k] as Hediff_Injury;
						if (hediff_Injury != null)
						{
							float severity = hediff_Injury.Severity;
							if (num + severity <= 20f)
							{
								num += severity;
								outHediffsToTend.Add(hediff_Injury);
							}
						}
					}
				}
			}
			TendUtility.tmpHediffs.Clear();
		}

		public static void SortByTendPriority(List<Hediff> hediffs)
		{
			if (hediffs.Count <= 1)
			{
				return;
			}
			TendUtility.tmpHediffsWithTendPriority.Clear();
			for (int i = 0; i < hediffs.Count; i++)
			{
				TendUtility.tmpHediffsWithTendPriority.Add(new Pair<Hediff, float>(hediffs[i], hediffs[i].TendPriority));
			}
			TendUtility.tmpHediffsWithTendPriority.SortByDescending((Pair<Hediff, float> x) => x.Second, (Pair<Hediff, float> x) => x.First.Severity);
			hediffs.Clear();
			for (int j = 0; j < TendUtility.tmpHediffsWithTendPriority.Count; j++)
			{
				hediffs.Add(TendUtility.tmpHediffsWithTendPriority[j].First);
			}
			TendUtility.tmpHediffsWithTendPriority.Clear();
		}
	}
}
