using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class BiomeWorker_IceSheet : BiomeWorker
	{
		public override float GetScore(Tile tile)
		{
			if (tile.WaterCovered)
			{
				return -100f;
			}
			return BiomeWorker_IceSheet.PermaIceScore(tile);
		}

		public static float PermaIceScore(Tile tile)
		{
			return -20f + -tile.temperature * 2f;
		}
	}
}
