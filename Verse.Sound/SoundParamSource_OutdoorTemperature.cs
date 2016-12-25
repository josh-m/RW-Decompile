using System;

namespace Verse.Sound
{
	public class SoundParamSource_OutdoorTemperature : SoundParamSource
	{
		public override string Label
		{
			get
			{
				return "Outdoor temperature";
			}
		}

		public override float ValueFor(Sample samp)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return 0f;
			}
			if (Find.VisibleMap == null)
			{
				return 0f;
			}
			return Find.VisibleMap.mapTemperature.OutdoorTemp;
		}
	}
}
