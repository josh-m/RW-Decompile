using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class SoundParamSource_TimeOfDay : SoundParamSource
	{
		public override string Label
		{
			get
			{
				return "Time of day (hour)";
			}
		}

		public override float ValueFor(Sample samp)
		{
			if (Find.VisibleMap == null)
			{
				return 0f;
			}
			return GenLocalDate.DayPercent(Find.VisibleMap) * 24f;
		}
	}
}
