using System;
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
			return GenDate.DayPassedPercent * 24f;
		}
	}
}
