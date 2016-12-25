using System;

namespace RimWorld
{
	public class MapCondition_ColdSnap : MapCondition
	{
		private const int LerpTicks = 12000;

		private const float MaxTempOffset = -20f;

		public override float TemperatureOffset()
		{
			return MapConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, 12000f, -20f);
		}
	}
}
