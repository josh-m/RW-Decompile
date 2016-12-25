using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Alert_Critical : Alert
	{
		private const float PulseFreq = 0.5f;

		private const float PulseAmpCritical = 0.6f;

		private const float PulseAmpTutorial = 0.2f;

		private int lastActiveFrame = -1;

		public override AlertPriority Priority
		{
			get
			{
				return AlertPriority.Critical;
			}
		}

		protected override Color BGColor
		{
			get
			{
				float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
				return new Color(num, num, num) * Color.red;
			}
		}

		public override void AlertActiveUpdate()
		{
			if (this.lastActiveFrame < Time.frameCount - 1)
			{
				Messages.Message("MessageCriticalAlert".Translate(new object[]
				{
					this.FullLabel
				}), this.Report.culprit, MessageSound.SeriousAlert);
			}
			this.lastActiveFrame = Time.frameCount;
		}
	}
}
