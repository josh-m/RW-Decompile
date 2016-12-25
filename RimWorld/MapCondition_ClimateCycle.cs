using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MapCondition_ClimateCycle : MapCondition
	{
		private const float PeriodYears = 4f;

		private const float MaxTempOffset = 20f;

		private int ticksOffset;

		public override void Init()
		{
			this.ticksOffset = ((Rand.Value >= 0.5f) ? 7200000 : 0);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.ticksOffset, "ticksOffset", 0, false);
		}

		public override float TemperatureOffset()
		{
			return Mathf.Sin((float)GenDate.YearsPassed / 4f * 3.14159274f * 2f) * 20f;
		}
	}
}
