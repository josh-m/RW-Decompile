using System;
using UnityEngine;

namespace Verse.Sound
{
	public class SoundParamTarget_PropertyHighPass : SoundParamTarget
	{
		private HighPassFilterProperty filterProperty;

		public override string Label
		{
			get
			{
				return "HighPassFilter-" + this.filterProperty;
			}
		}

		public override Type NeededFilterType
		{
			get
			{
				return typeof(SoundFilterHighPass);
			}
		}

		public override void SetOn(Sample sample, float value)
		{
			AudioHighPassFilter audioHighPassFilter = sample.source.GetComponent<AudioHighPassFilter>();
			if (audioHighPassFilter == null)
			{
				audioHighPassFilter = sample.source.gameObject.AddComponent<AudioHighPassFilter>();
			}
			if (this.filterProperty == HighPassFilterProperty.Cutoff)
			{
				audioHighPassFilter.cutoffFrequency = value;
			}
			if (this.filterProperty == HighPassFilterProperty.Resonance)
			{
				audioHighPassFilter.highpassResonanceQ = value;
			}
		}
	}
}
