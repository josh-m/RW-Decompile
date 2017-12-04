using System;

namespace RimWorld
{
	public class GameCondition_ColdSnap : GameCondition
	{
		private const int LerpTicks = 12000;

		private const float MaxTempOffset = -20f;

		public override float TemperatureOffset()
		{
			return GameConditionUtility.LerpInOutValue(this, 12000f, -20f);
		}
	}
}
