using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition_Eclipse : GameCondition
	{
		private const int LerpTicks = 200;

		private SkyColorSet EclipseSkyColors = new SkyColorSet(new Color(0.482f, 0.603f, 0.682f), Color.white, new Color(0.6f, 0.6f, 0.6f), 1f);

		public override float SkyTargetLerpFactor()
		{
			return GameConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, 200f, 1f);
		}

		public override SkyTarget? SkyTarget()
		{
			return new SkyTarget?(new SkyTarget(0f, this.EclipseSkyColors, 1f, 0f));
		}
	}
}
