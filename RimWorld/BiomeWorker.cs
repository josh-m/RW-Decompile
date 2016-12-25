using RimWorld.Planet;
using System;

namespace RimWorld
{
	public abstract class BiomeWorker
	{
		public abstract float GetScore(Tile tile);
	}
}
