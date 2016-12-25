using System;
using Verse;

namespace RimWorld
{
	public class MapCondition_PsychicEmanation : MapCondition
	{
		public Gender gender = Gender.Male;

		public override string Label
		{
			get
			{
				return this.def.label + " (" + this.gender.ToString().Translate().ToLower() + ")";
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<Gender>(ref this.gender, "gender", Gender.None, false);
		}
	}
}
