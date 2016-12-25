using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class BiomeWorker_Desert : BiomeWorker
	{
		public override float GetScore(WorldSquare square)
		{
			if (square.elevation <= 0f)
			{
				return -100f;
			}
			if (square.rainfall >= 600f)
			{
				return 0f;
			}
			return square.temperature + 0.0001f;
		}
	}
}
