using System;
using UnityEngine;

namespace Verse.Sound
{
	public class SoundParamTarget_PropertyEcho : SoundParamTarget
	{
		private EchoFilterProperty filterProperty;

		public override string Label
		{
			get
			{
				return "EchoFilter-" + this.filterProperty;
			}
		}

		public override Type NeededFilterType
		{
			get
			{
				return typeof(SoundFilterEcho);
			}
		}

		public override void SetOn(Sample sample, float value)
		{
			AudioEchoFilter audioEchoFilter = sample.source.GetComponent<AudioEchoFilter>();
			if (audioEchoFilter == null)
			{
				audioEchoFilter = sample.source.gameObject.AddComponent<AudioEchoFilter>();
			}
			if (this.filterProperty == EchoFilterProperty.Delay)
			{
				audioEchoFilter.delay = value;
			}
			if (this.filterProperty == EchoFilterProperty.DecayRatio)
			{
				audioEchoFilter.decayRatio = value;
			}
			if (this.filterProperty == EchoFilterProperty.WetMix)
			{
				audioEchoFilter.wetMix = value;
			}
			if (this.filterProperty == EchoFilterProperty.DryMix)
			{
				audioEchoFilter.dryMix = value;
			}
		}
	}
}
