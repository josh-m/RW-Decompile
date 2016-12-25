using System;
using Verse;

namespace RimWorld
{
	public sealed class DifficultyDef : Def
	{
		public int difficulty = -1;

		public float threatScale = -1f;

		public float colonistMoodOffset = -1f;

		public float baseSellPriceFactor = -1f;

		public float cropYieldFactor = -1f;

		public float diseaseIntervalFactor = 1f;

		public float enemyReproductionRateFactor = 1f;
	}
}
