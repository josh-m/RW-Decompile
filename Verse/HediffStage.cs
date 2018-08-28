using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class HediffStage
	{
		public float minSeverity;

		[MustTranslate]
		public string label;

		[TranslationHandle, Unsaved]
		public string untranslatedLabel;

		public bool becomeVisible = true;

		public bool lifeThreatening;

		public TaleDef tale;

		public float vomitMtbDays = -1f;

		public float deathMtbDays = -1f;

		public float painFactor = 1f;

		public float painOffset;

		public float forgetMemoryThoughtMtbDays = -1f;

		public float pctConditionalThoughtsNullified;

		public float opinionOfOthersFactor = 1f;

		public float hungerRateFactor = 1f;

		public float hungerRateFactorOffset;

		public float restFallFactor = 1f;

		public float restFallFactorOffset;

		public float socialFightChanceFactor = 1f;

		public float mentalBreakMtbDays = -1f;

		public List<HediffDef> makeImmuneTo;

		public List<PawnCapacityModifier> capMods = new List<PawnCapacityModifier>();

		public List<HediffGiver> hediffGivers;

		public List<MentalStateGiver> mentalStateGivers;

		public List<StatModifier> statOffsets;

		public float partEfficiencyOffset;

		public bool partIgnoreMissingHP;

		public bool destroyPart;

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

		public void PostLoad()
		{
			this.untranslatedLabel = this.label;
		}

		public IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			return HediffStatsUtility.SpecialDisplayStats(this, null);
		}
	}
}
