using System;

namespace Verse.Sound
{
	public class SoundParamTarget_Volume : SoundParamTarget
	{
		public override string Label
		{
			get
			{
				return "Volume";
			}
		}

		public override void SetOn(Sample sample, float value)
		{
			sample.SignalMappedVolume(value, this);
		}
	}
}
