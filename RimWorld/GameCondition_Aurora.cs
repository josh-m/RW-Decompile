using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition_Aurora : GameCondition
	{
		private int curColorIndex = -1;

		private int prevColorIndex = -1;

		private float curColorTransition;

		private SkyColorSet AuroraSkyColors = new SkyColorSet(Color.white, new Color(0.92f, 0.92f, 0.92f), Color.white, 1f);

		private const int LerpTicks = 200;

		public const float MaxSunGlow = 0.5f;

		private const float Glow = 0.25f;

		private const float SkyColorStrength = 0.075f;

		private const float OverlayColorStrength = 0.025f;

		private const float BaseBrightness = 0.73f;

		private const int TransitionDurationTicks_NotPermanent = 280;

		private const int TransitionDurationTicks_Permanent = 3750;

		private static readonly Color[] Colors = new Color[]
		{
			new Color(0f, 1f, 0f),
			new Color(0.3f, 1f, 0f),
			new Color(0f, 1f, 0.7f),
			new Color(0.3f, 1f, 0.7f),
			new Color(0f, 0.5f, 1f),
			new Color(0f, 0f, 1f),
			new Color(0.87f, 0f, 1f),
			new Color(0.75f, 0f, 1f)
		};

		public override float SkyGazeChanceFactor
		{
			get
			{
				return 5f;
			}
		}

		public override float SkyGazeJoyGainFactor
		{
			get
			{
				return 3f;
			}
		}

		public Color CurrentColor
		{
			get
			{
				return Color.Lerp(GameCondition_Aurora.Colors[this.prevColorIndex], GameCondition_Aurora.Colors[this.curColorIndex], this.curColorTransition);
			}
		}

		private int TransitionDurationTicks
		{
			get
			{
				return (!base.Permanent) ? 280 : 3750;
			}
		}

		private float Brightness
		{
			get
			{
				return (!base.Permanent) ? 0.73f : Mathf.Lerp(0.73f, 1f, GenCelestial.CurCelestialSunGlow(base.Map));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.curColorIndex, "curColorIndex", 0, false);
			Scribe_Values.Look<int>(ref this.prevColorIndex, "prevColorIndex", 0, false);
			Scribe_Values.Look<float>(ref this.curColorTransition, "curColorTransition", 0f, false);
		}

		public override void Init()
		{
			base.Init();
			this.curColorIndex = Rand.Range(0, GameCondition_Aurora.Colors.Length);
			this.prevColorIndex = this.curColorIndex;
			this.curColorTransition = 1f;
		}

		public override float SkyTargetLerpFactor()
		{
			return GameConditionUtility.LerpInOutValue(this, 200f, 1f);
		}

		public override SkyTarget? SkyTarget()
		{
			Color currentColor = this.CurrentColor;
			this.AuroraSkyColors.sky = Color.Lerp(Color.white, currentColor, 0.075f) * this.Brightness;
			this.AuroraSkyColors.overlay = Color.Lerp(Color.white, currentColor, 0.025f) * this.Brightness;
			float glow = Mathf.Max(GenCelestial.CurCelestialSunGlow(base.Map), 0.25f);
			return new SkyTarget?(new SkyTarget(glow, this.AuroraSkyColors, 1f, 1f));
		}

		public override void GameConditionTick()
		{
			this.curColorTransition += 1f / (float)this.TransitionDurationTicks;
			if (this.curColorTransition >= 1f)
			{
				this.prevColorIndex = this.curColorIndex;
				this.curColorIndex = this.GetNewColorIndex();
				this.curColorTransition = 0f;
			}
			if (!base.Permanent && base.TicksLeft > 200 && GenCelestial.CurCelestialSunGlow(base.Map) > 0.5f)
			{
				base.TicksLeft = 200;
			}
		}

		private int GetNewColorIndex()
		{
			return (from x in Enumerable.Range(0, GameCondition_Aurora.Colors.Length)
			where x != this.curColorIndex
			select x).RandomElement<int>();
		}
	}
}
