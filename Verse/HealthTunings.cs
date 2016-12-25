using System;

namespace Verse
{
	public class HealthTunings
	{
		public const int MinDamageSeverityForOld = 7;

		public const float MinDamagePartPctForOld = 0.25f;

		public const float MinDamagePartPctForInfection = 0.2f;

		public const float DelicateBodyPartOldInjuryChanceThreshold = 0.8f;

		public const float AnimalsInfectionChanceFactor = 0.125f;

		public const float DeathOnDownedChance_NonColonyHumanlike = 0.67f;

		public const float DeathOnDownedChance_NonColonyAnimal = 0.47f;

		public const float SmallPawnFragmentedDamageHealthScaleThreshold = 0.5f;

		public const int SmallPawnFragmentedDamageMinimumDamageAmount = 10;

		public const float ImpossibleToFallSickIfAboveThisImmunityLevel = 0.6f;

		public const int SeverityTendedPerMedicine = 20;

		public const int HealInterval_NoBed = 5000;

		public const int HealInterval_InjuriesTended = 650;

		public const int InjuryHealAmount_TendedPoorly = 1;

		public const int InjuryHealAmount_TendedWell = 2;

		public const float MaxDamageToHealUntended = 2f;

		public const float MinBleedingRateToBleed = 0.1f;

		public const float BleedRateGlobalFactor = 0.0142857144f;

		public const float BleedSeverityRecoveryPerInterval = 0.02f;

		public const float BloodFilthDropChanceFactorStanding = 0.008f;

		public const float BloodFilthDropChanceFactorLaying = 0.0008f;

		public const int BaseTicksAfterInjuryNoLongerBleeding = 80000;

		public const int TicksAfterMissingBodyPartNoLongerFresh = 70000;

		public const int BurnDamageCheckInterval = 400;

		public const float HypothermiaGrowthPerDegreeUnder = 6.45E-05f;

		public const float HeatstrokeGrowthPerDegreeOver = 6.45E-05f;

		public const float MinHeatstrokeProgress = 0.000375f;

		public const float MinHypothermiaProgress = 0.00075f;

		public const float HarmfulTemperatureOffset = 10f;

		public const float BurnTemperatureOffset = 150f;

		public const int VomitCheckInterval = 600;

		public const int DeathCheckInterval = 200;

		public const int ForgetRandomMemoryThoughtCheckInterval = 400;

		public const float GoodWillGainPerTend = 0.3f;

		public const float PawnBaseHealthForPercent = 75f;

		public static readonly IntRange InfectionDelayRange = new IntRange(25000, 45000);

		public static float ChanceToAdditionallyDamageInnerSolidPart = 0.15f;
	}
}
