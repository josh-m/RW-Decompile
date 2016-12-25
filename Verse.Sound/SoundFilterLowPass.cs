using System;
using UnityEngine;

namespace Verse.Sound
{
	public class SoundFilterLowPass : SoundFilter
	{
		[Description("This filter will attenuate frequencies above this cutoff frequency."), EditSliderRange(50f, 20000f)]
		private float cutoffFrequency = 10000f;

		[Description("The resonance Q value."), EditSliderRange(1f, 10f)]
		private float lowpassResonaceQ = 1f;

		public override void SetupOn(AudioSource source)
		{
			AudioLowPassFilter orMakeFilterOn = SoundFilter.GetOrMakeFilterOn<AudioLowPassFilter>(source);
			orMakeFilterOn.cutoffFrequency = this.cutoffFrequency;
			orMakeFilterOn.lowpassResonanceQ = this.lowpassResonaceQ;
		}
	}
}
