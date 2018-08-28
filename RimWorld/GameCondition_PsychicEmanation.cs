using System;
using Verse;

namespace RimWorld
{
	public class GameCondition_PsychicEmanation : GameCondition
	{
		public Gender gender = Gender.Male;

		public PsychicDroneLevel level = PsychicDroneLevel.BadMedium;

		public override string Label
		{
			get
			{
				return this.def.label + " (" + this.gender.ToString().Translate().ToLower() + ")";
			}
		}

		public override void PostMake()
		{
			base.PostMake();
			this.level = this.def.defaultDroneLevel;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<Gender>(ref this.gender, "gender", Gender.None, false);
			Scribe_Values.Look<PsychicDroneLevel>(ref this.level, "level", PsychicDroneLevel.None, false);
		}
	}
}
