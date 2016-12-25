using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class BiomeWorker_Ocean : BiomeWorker
	{
		public override float GetScore(Tile tile)
		{
			if (!tile.WaterCovered)
			{
				return -100f;
			}
			return 0f;
		}
	}
}
