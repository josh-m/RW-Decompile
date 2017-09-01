using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition_VolcanicWinter : GameCondition
	{
		private const float AnimalDensityImpact = 0.5f;

		private const float SkyGlow = 0.55f;

		private const float MaxSkyLerpFactor = 0.3f;

		private int LerpTicks = 50000;

		private float MaxTempOffset = -7f;

		private SkyColorSet VolcanicWinterColors;

		public GameCondition_VolcanicWinter()
		{
			ColorInt colorInt = new ColorInt(0, 0, 0);
			this.VolcanicWinterColors = new SkyColorSet(colorInt.ToColor, Color.white, new Color(0.6f, 0.6f, 0.6f), 0.65f);
			base..ctor();
		}

		public override float SkyTargetLerpFactor()
		{
			return GameConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, (float)this.LerpTicks, 0.3f);
		}

		public override SkyTarget? SkyTarget()
		{
			return new SkyTarget?(new SkyTarget(0.55f, this.VolcanicWinterColors, 1f, 1f));
		}

		public override float TemperatureOffset()
		{
			return GameConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, (float)this.LerpTicks, this.MaxTempOffset);
		}

		public override float AnimalDensityFactor()
		{
			return 1f - GameConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, (float)this.LerpTicks, 0.5f);
		}

		public override bool AllowEnjoyableOutsideNow()
		{
			return false;
		}
	}
}
