using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class BiomeWorker_Tundra : BiomeWorker
	{
		public override float GetScore(WorldSquare square)
		{
			if (square.elevation <= 0f)
			{
				return -100f;
			}
			return -square.temperature;
		}
	}
}
