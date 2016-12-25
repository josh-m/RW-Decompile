using System;
using UnityEngine;

namespace Verse.Sound
{
	public class SoundFilterEcho : SoundFilter
	{
		[Description("Echo delay in ms. 10 to 5000. Default = 500."), EditSliderRange(10f, 5000f)]
		private float delay = 500f;

		[Description("Echo decay per delay. 0 to 1. 1.0 = No decay, 0.0 = total decay (ie simple 1 line delay)."), EditSliderRange(0f, 1f)]
		private float decayRatio = 0.5f;

		[Description("The volume of the echo signal to pass to output."), EditSliderRange(0f, 1f)]
		private float wetMix = 1f;

		[Description("The volume of the original signal to pass to output."), EditSliderRange(0f, 1f)]
		private float dryMix = 1f;

		public override void SetupOn(AudioSource source)
		{
			AudioEchoFilter orMakeFilterOn = SoundFilter.GetOrMakeFilterOn<AudioEchoFilter>(source);
			orMakeFilterOn.delay = this.delay;
			orMakeFilterOn.decayRatio = this.decayRatio;
			orMakeFilterOn.wetMix = this.wetMix;
			orMakeFilterOn.dryMix = this.dryMix;
		}
	}
}
