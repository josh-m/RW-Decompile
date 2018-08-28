using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TraitDegreeData
	{
		[MustTranslate]
		public string label;

		[TranslationHandle, Unsaved]
		public string untranslatedLabel;

		[MustTranslate]
		public string description;

		public int degree;

		public float commonality = 1f;

		public List<StatModifier> statOffsets;

		public List<StatModifier> statFactors;

		public ThinkTreeDef thinkTree;

		public MentalStateDef randomMentalState;

		public SimpleCurve randomMentalStateMtbDaysMoodCurve;

		public List<MentalStateDef> disallowedMentalStates;

		public List<InspirationDef> disallowedInspirations;

		public List<MentalBreakDef> theOnlyAllowedMentalBreaks;

		public Dictionary<SkillDef, int> skillGains = new Dictionary<SkillDef, int>();

		public float socialFightChanceFactor = 1f;

		public float marketValueFactorOffset;

		public float randomDiseaseMtbDays;

		public Type mentalStateGiverClass = typeof(TraitMentalStateGiver);

		[Unsaved]
		private TraitMentalStateGiver mentalStateGiverInt;

		public TraitMentalStateGiver MentalStateGiver
		{
			get
			{
				if (this.mentalStateGiverInt == null)
				{
					this.mentalStateGiverInt = (TraitMentalStateGiver)Activator.CreateInstance(this.mentalStateGiverClass);
					this.mentalStateGiverInt.traitDegreeData = this;
				}
				return this.mentalStateGiverInt;
			}
		}

		public void PostLoad()
		{
			this.untranslatedLabel = this.label;
		}
	}
}
