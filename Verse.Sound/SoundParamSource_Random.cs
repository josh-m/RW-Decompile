using System;

namespace Verse.Sound
{
	public class SoundParamSource_Random : SoundParamSource
	{
		public override string Label
		{
			get
			{
				return "Random";
			}
		}

		public override float ValueFor(Sample samp)
		{
			return Rand.Value;
		}
	}
}
