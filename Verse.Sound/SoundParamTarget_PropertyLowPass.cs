using System;
using UnityEngine;

namespace Verse.Sound
{
	public class SoundParamTarget_PropertyLowPass : SoundParamTarget
	{
		private LowPassFilterProperty filterProperty;

		public override string Label
		{
			get
			{
				return "LowPassFilter-" + this.filterProperty;
			}
		}

		public override Type NeededFilterType
		{
			get
			{
				return typeof(SoundFilterLowPass);
			}
		}

		public override void SetOn(Sample sample, float value)
		{
			AudioLowPassFilter audioLowPassFilter = sample.source.GetComponent<AudioLowPassFilter>();
			if (audioLowPassFilter == null)
			{
				audioLowPassFilter = sample.source.gameObject.AddComponent<AudioLowPassFilter>();
			}
			if (this.filterProperty == LowPassFilterProperty.Cutoff)
			{
				audioLowPassFilter.cutoffFrequency = value;
			}
			if (this.filterProperty == LowPassFilterProperty.Resonance)
			{
				audioLowPassFilter.lowpassResonanceQ = value;
			}
		}
	}
}
