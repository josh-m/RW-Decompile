using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class BiomeWorker_Tundra : BiomeWorker
	{
		public override float GetScore(Tile tile)
		{
			if (tile.WaterCovered)
			{
				return -100f;
			}
			return -tile.temperature;
		}
	}
}
