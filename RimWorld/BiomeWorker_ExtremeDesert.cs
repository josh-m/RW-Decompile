using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class BiomeWorker_ExtremeDesert : BiomeWorker
	{
		public override float GetScore(WorldSquare square)
		{
			if (square.elevation <= 0f)
			{
				return -100f;
			}
			if (square.rainfall >= 340f)
			{
				return 0f;
			}
			return square.temperature * 2.7f - 13f - square.rainfall * 0.14f;
		}
	}
}
