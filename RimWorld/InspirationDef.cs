using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class InspirationDef : Def
	{
		public Type inspirationClass = typeof(Inspiration);

		public Type workerClass = typeof(InspirationWorker);

		public float baseCommonality = 1f;

		public float baseDurationDays = 1f;

		public bool allowedOnAnimals;

		public bool allowedOnNonColonists;

		public List<StatDef> requiredNonDisabledStats;

		public List<SkillRequirement> requiredSkills;

		public List<WorkTypeDef> requiredNonDisabledWorkTypes;

		public List<PawnCapacityDef> requiredCapacities;

		public List<StatModifier> statOffsets;

		public List<StatModifier> statFactors;

		[MustTranslate]
		public string beginLetter;

		[MustTranslate]
		public string beginLetterLabel;

		public LetterDef beginLetterDef;

		[MustTranslate]
		public string endMessage;

		[MustTranslate]
		public string baseInspectLine;

		[Unsaved]
		private InspirationWorker workerInt;

		public InspirationWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (InspirationWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}
	}
}
