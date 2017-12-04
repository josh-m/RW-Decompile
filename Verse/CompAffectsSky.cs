using System;
using UnityEngine;

namespace Verse
{
	public class CompAffectsSky : ThingComp
	{
		private int autoAnimationStartTick;

		private int fadeInDuration;

		private int holdDuration;

		private int fadeOutDuration;

		private float autoAnimationTarget;

		public CompProperties_AffectsSky Props
		{
			get
			{
				return (CompProperties_AffectsSky)this.props;
			}
		}

		public virtual float LerpFactor
		{
			get
			{
				if (this.HasAutoAnimation)
				{
					int ticksGame = Find.TickManager.TicksGame;
					float num;
					if (ticksGame < this.autoAnimationStartTick + this.fadeInDuration)
					{
						num = (float)(ticksGame - this.autoAnimationStartTick) / (float)this.fadeInDuration;
					}
					else if (ticksGame < this.autoAnimationStartTick + this.fadeInDuration + this.holdDuration)
					{
						num = 1f;
					}
					else
					{
						num = 1f - (float)(ticksGame - this.autoAnimationStartTick - this.fadeInDuration - this.holdDuration) / (float)this.fadeOutDuration;
					}
					return Mathf.Clamp01(num * this.autoAnimationTarget);
				}
				return 0f;
			}
		}

		public bool HasAutoAnimation
		{
			get
			{
				return Find.TickManager.TicksGame < this.autoAnimationStartTick + this.fadeInDuration + this.holdDuration + this.fadeOutDuration;
			}
		}

		public virtual SkyTarget SkyTarget
		{
			get
			{
				return new SkyTarget(this.Props.glow, this.Props.skyColors, this.Props.lightsourceShineSize, this.Props.lightsourceShineIntensity);
			}
		}

		public virtual Vector2? OverrideShadowVector
		{
			get
			{
				return null;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.autoAnimationStartTick, "autoAnimationStartTick", 0, false);
			Scribe_Values.Look<int>(ref this.fadeInDuration, "fadeInDuration", 0, false);
			Scribe_Values.Look<int>(ref this.holdDuration, "holdDuration", 0, false);
			Scribe_Values.Look<int>(ref this.fadeOutDuration, "fadeOutDuration", 0, false);
			Scribe_Values.Look<float>(ref this.autoAnimationTarget, "autoAnimationTarget", 0f, false);
		}

		public void StartFadeInHoldFadeOut(int fadeInDuration, int holdDuration, int fadeOutDuration, float target = 1f)
		{
			this.autoAnimationStartTick = Find.TickManager.TicksGame;
			this.fadeInDuration = fadeInDuration;
			this.holdDuration = holdDuration;
			this.fadeOutDuration = fadeOutDuration;
			this.autoAnimationTarget = target;
		}
	}
}
