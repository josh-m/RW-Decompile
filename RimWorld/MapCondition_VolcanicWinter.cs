using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MapCondition_VolcanicWinter : MapCondition
	{
		private const float AnimalDensityImpact = 0.5f;

		private const float SkyGlow = 0.55f;

		private const float MaxSkyLerpFactor = 0.3f;

		private int LerpTicks = 50000;

		private float MaxTempOffset = -7f;

		private SkyColorSet VolcanicWinterColors;

		public MapCondition_VolcanicWinter()
		{
			ColorInt colorInt = new ColorInt(0, 0, 0);
			this.VolcanicWinterColors = new SkyColorSet(colorInt.ToColor, Color.white, new Color(0.6f, 0.6f, 0.6f), 0.65f);
			base..ctor();
		}

		public override float SkyTargetLerpFactor()
		{
			return MapConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, (float)this.LerpTicks, 0.3f);
		}

		public override SkyTarget? SkyTarget()
		{
			return new SkyTarget?(new SkyTarget(this.VolcanicWinterColors)
			{
				glow = 0.55f
			});
		}

		public override float TemperatureOffset()
		{
			return MapConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, (float)this.LerpTicks, this.MaxTempOffset);
		}

		public override float AnimalDensityFactor()
		{
			return 1f - MapConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, (float)this.LerpTicks, 0.5f);
		}

		public override bool AllowEnjoyableOutsideNow()
		{
			return false;
		}
	}
}
