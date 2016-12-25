using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class BiomeWorker_SeaIce : BiomeWorker
	{
		public override float GetScore(Tile tile)
		{
			if (!tile.WaterCovered)
			{
				return -100f;
			}
			return BiomeWorker_IceSheet.PermaIceScore(tile) - 23f;
		}
	}
}
