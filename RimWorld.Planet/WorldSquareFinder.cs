using System;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldSquareFinder
	{
		public static IntVec2 RandomStartingWorldSquare()
		{
			IntVec2 zero = IntVec2.Zero;
			for (int i = 0; i < 5000; i++)
			{
				zero = new IntVec2(Rand.Range(10, Find.World.Size.x - 10), Rand.Range(10, Find.World.Size.z - 10));
				WorldSquare worldSquare = Find.World.grid.Get(zero);
				if (worldSquare.biome.canBuildBase && worldSquare.biome.implemented)
				{
					return zero;
				}
			}
			Log.Error("Found no starting world square.");
			return zero;
		}
	}
}
