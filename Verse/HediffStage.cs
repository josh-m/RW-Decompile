using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class HediffStage
	{
		public float minSeverity;

		public string label;

		public bool everVisible = true;

		public bool lifeThreatening;

		public float vomitMtbDays = -1f;

		public float deathMtbDays = -1f;

		public float painFactor = 1f;

		public float painOffset;

		public float partEfficiencyFactor = 1f;

		public float forgetMemoryThoughtMtbDays = -1f;

		public float pctConditionalThoughtsNullified;

		public float opinionOfOthersFactor = 1f;

		public float hungerRateFactor = 1f;

		public float restFallFactor = 1f;

		public float socialFightChanceFactor = 1f;

		public float setMinPartEfficiency = -1f;

		public bool destroyPart;

		public List<HediffDef> makeImmuneTo;

		public TaleDef tale;

		public List<PawnCapacityModifier> capMods = new List<PawnCapacityModifier>();

		public List<HediffGiver> hediffGivers;

		public List<MentalStateGiver> mentalStateGivers;

		public List<StatModifier> statOffsets;

		public bool AffectsMemory
		{
			get
			{
				return this.forgetMemoryThoughtMtbDays > 0f || this.pctConditionalThoughtsNullified > 0f;
			}
		}

		public bool AffectsSocialInteractions
		{
			get
			{
				return this.opinionOfOthersFactor != 1f;
			}
		}

		public IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			return HediffStatsUtility.SpecialDisplayStats(this, null);
		}
	}
}
