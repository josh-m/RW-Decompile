using System;
using Verse;

namespace RimWorld
{
	public sealed class DifficultyDef : Def
	{
		public int difficulty = -1;

		public float threatScale;

		public bool allowBigThreats = true;

		public bool allowIntroThreats = true;

		public bool allowCaveHives = true;

		public bool peacefulTemples;

		public float colonistMoodOffset;

		public float tradePriceFactorLoss;

		public float cropYieldFactor = 1f;

		public float diseaseIntervalFactor = 1f;

		public float enemyReproductionRateFactor = 1f;

		public float playerPawnInfectionChanceFactor = 1f;

		public float manhunterChanceOnDamageFactor = 1f;
	}
}
