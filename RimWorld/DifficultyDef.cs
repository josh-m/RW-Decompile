using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class DifficultyDef : Def
	{
		public Color drawColor = Color.white;

		public bool isExtreme;

		public int difficulty = -1;

		public float threatScale = 1f;

		public bool allowBigThreats = true;

		public bool allowIntroThreats = true;

		public bool allowCaveHives = true;

		public bool peacefulTemples;

		public bool predatorsHuntHumanlikes = true;

		public float colonistMoodOffset;

		public float tradePriceFactorLoss;

		public float cropYieldFactor = 1f;

		public float mineYieldFactor = 1f;

		public float researchSpeedFactor = 1f;

		public float diseaseIntervalFactor = 1f;

		public float enemyReproductionRateFactor = 1f;

		public float playerPawnInfectionChanceFactor = 1f;

		public float manhunterChanceOnDamageFactor = 1f;

		public float deepDrillInfestationChanceFactor = 1f;

		public float foodPoisonChanceFactor = 1f;

		public float raidBeaconThreatCountFactor = 1f;

		public float maintenanceCostFactor = 1f;

		public float enemyDeathOnDownedChanceFactor = 1f;

		public float adaptationGrowthRateFactorOverZero = 1f;

		public float adaptationEffectFactor = 1f;
	}
}
