using System;

namespace Verse.AI
{
	public static class AvoidGridTunings
	{
		public const byte MaxValue = 255;

		public const ushort PathCostFactor = 8;

		public const int CostForTurretLOS = 12;

		public const int CostForTrapCenter = 32;

		public const float TrapCostRadius = 2.9f;
	}
}
