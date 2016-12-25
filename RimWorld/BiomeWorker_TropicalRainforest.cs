using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class BiomeWorker_TropicalRainforest : BiomeWorker
	{
		public override float GetScore(WorldSquare square)
		{
			if (square.elevation <= 0f)
			{
				return -100f;
			}
			if (square.temperature < 15f)
			{
				return 0f;
			}
			if (square.rainfall < 2000f)
			{
				return 0f;
			}
			return 28f + (square.temperature - 20f) * 1.5f + (square.rainfall - 600f) / 165f;
		}
	}
}
