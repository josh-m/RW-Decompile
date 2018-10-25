using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition_ClimateCycle : GameCondition
	{
		private int ticksOffset;

		private const float PeriodYears = 4f;

		private const float MaxTempOffset = 20f;

		public override void Init()
		{
			this.ticksOffset = ((Rand.Value >= 0.5f) ? 7200000 : 0);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksOffset, "ticksOffset", 0, false);
		}

		public override float TemperatureOffset()
		{
			return Mathf.Sin((GenDate.YearsPassedFloat + (float)this.ticksOffset / 3600000f) / 4f * 3.14159274f * 2f) * 20f;
		}
	}
}
