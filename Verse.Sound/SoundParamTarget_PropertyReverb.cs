using System;
using UnityEngine;

namespace Verse.Sound
{
	public class SoundParamTarget_PropertyReverb : SoundParamTarget
	{
		[Description("The base setup for the reverb.\n\nOnly used if no parameters are touching this filter.")]
		private ReverbSetup baseSetup = new ReverbSetup();

		[Description("The interpolation target setup for this filter.\n\nWhen the interpolant parameter is at 1, these settings will be active.")]
		private ReverbSetup targetSetup = new ReverbSetup();

		public override string Label
		{
			get
			{
				return "ReverbFilter-interpolant";
			}
		}

		public override Type NeededFilterType
		{
			get
			{
				return typeof(SoundFilterReverb);
			}
		}

		public override void SetOn(Sample sample, float value)
		{
			AudioReverbFilter audioReverbFilter = sample.source.GetComponent<AudioReverbFilter>();
			if (audioReverbFilter == null)
			{
				audioReverbFilter = sample.source.gameObject.AddComponent<AudioReverbFilter>();
			}
			ReverbSetup reverbSetup;
			if (value < 0.001f)
			{
				reverbSetup = this.baseSetup;
			}
			if (value > 0.999f)
			{
				reverbSetup = this.targetSetup;
			}
			else
			{
				reverbSetup = ReverbSetup.Lerp(this.baseSetup, this.targetSetup, value);
			}
			reverbSetup.ApplyTo(audioReverbFilter);
		}
	}
}
