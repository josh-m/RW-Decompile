using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class RecipeMakerProperties
	{
		public int productCount = 1;

		public int workAmount = -1;

		public StatDef workSpeedStat;

		public StatDef efficiencyStat;

		public ThingDef unfinishedThingDef;

		public ThingFilter defaultIngredientFilter;

		public List<SkillRequirement> skillRequirements;

		public SkillDef workSkill;

		public float workSkillLearnPerTick = 1f;

		public EffecterDef effectWorking;

		public SoundDef soundWorking;

		public List<ThingDef> recipeUsers;

		public ResearchProjectDef researchPrerequisite;
	}
}
