using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class BiomeWorker_AridShrubland : BiomeWorker
	{
		public override float GetScore(WorldSquare square)
		{
			if (square.elevation <= 0f)
			{
				return -100f;
			}
			if (square.temperature < -10f)
			{
				return 0f;
			}
			if (square.rainfall < 600f || square.rainfall >= 2000f)
			{
				return 0f;
			}
			return 22.5f + (square.temperature - 20f) * 2.2f + (square.rainfall - 600f) / 100f;
		}
	}
}
