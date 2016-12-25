using System;
using Verse;

namespace RimWorld.Planet
{
	public static class GenWorldClosest
	{
		public static bool TryFindClosestTile(int rootTile, Predicate<int> predicate, out int foundTile, int maxTilesToScan = 2147483647, bool canSearchThroughImpassable = true)
		{
			int foundTileLocal = -1;
			WorldFloodFiller.FloodFill(rootTile, (int x) => canSearchThroughImpassable || !Find.World.Impassable(x), delegate(int t)
			{
				bool flag = predicate(t);
				if (flag)
				{
					foundTileLocal = t;
				}
				return flag;
			}, maxTilesToScan);
			foundTile = foundTileLocal;
			return foundTileLocal >= 0;
		}

		public static bool TryFindClosestPassableTile(int rootTile, out int foundTile)
		{
			return GenWorldClosest.TryFindClosestTile(rootTile, (int x) => !Find.World.Impassable(x), out foundTile, 2147483647, true);
		}
	}
}
