using System;

namespace Verse
{
	public class HealthTunings
	{
		public const float SmallPawnFragmentedDamageHealthScaleThreshold = 0.5f;

		public const int SmallPawnFragmentedDamageMinimumDamageAmount = 10;

		public const float MinBleedingRateToBleed = 0.1f;

		public const float BleedSeverityRecoveryPerInterval = 0.02f;

		public const float BloodFilthDropChanceFactorStanding = 0.008f;

		public const float BloodFilthDropChanceFactorLaying = 0.0008f;

		public const int BaseTicksAfterInjuryToStopBleeding = 90000;

		public const int TicksAfterMissingBodyPartToStopBeingFresh = 90000;

		public const int InjuryHealInterval = 600;

		public const float InjuryHealPerDay_Untended = 8f;

		public const float InjuryHealPerDay_Tended = 22f;

		public const int InjurySeverityTendedPerMedicine = 20;

		public const int MinDamageSeverityForOld = 7;

		public const float MinDamagePartPctForOld = 0.25f;

		public const float DelicateBodyPartOldInjuryChanceThreshold = 0.8f;

		public const float MinDamagePartPctForInfection = 0.2f;

		public const float AnimalsInfectionChanceFactor = 0.15f;

		public const float HypothermiaGrowthPerDegreeUnder = 6.45E-05f;

		public const float HeatstrokeGrowthPerDegreeOver = 6.45E-05f;

		public const float MinHeatstrokeProgressPerInterval = 0.000375f;

		public const float MinHypothermiaProgress = 0.00075f;

		public const float HarmfulTemperatureOffset = 10f;

		public const float MinTempOverComfyMaxForBurn = 150f;

		public const float BurnDamagePerTempOverage = 0.06f;

		public const int MinBurnDamage = 3;

		public const float ImpossibleToFallSickIfAboveThisImmunityLevel = 0.6f;

		public const int HediffGiverUpdateInterval = 60;

		public const int VomitCheckInterval = 600;

		public const int DeathCheckInterval = 200;

		public const int ForgetRandomMemoryThoughtCheckInterval = 400;

		public const float GoodWillGainPerTend = 0.3f;

		public const float PawnBaseHealthForSummary = 75f;

		public const float BaseBecomeOldChance = 0.2f;

		public const float DeathOnDownedChance_NonColonyHumanlike = 0.67f;

		public const float DeathOnDownedChance_NonColonyAnimal = 0.47f;

		public static float ChanceToAdditionallyDamageInnerSolidPart = 0.2f;

		public static readonly IntRange InfectionDelayRange = new IntRange(30000, 60000);
	}
}
