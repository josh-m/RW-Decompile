using System;

namespace RimWorld
{
	public class GameCondition_HeatWave : GameCondition
	{
		private const int LerpTicks = 12000;

		private const float MaxTempOffset = 17f;

		public override float TemperatureOffset()
		{
			return GameConditionUtility.LerpInOutValue(this, 12000f, 17f);
		}
	}
}
