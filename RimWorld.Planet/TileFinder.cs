using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class TileFinder
	{
		private static List<Pair<int, int>> tmpTiles = new List<Pair<int, int>>();

		public static int RandomStartingTile()
		{
			for (int i = 0; i < 5000; i++)
			{
				int num = Rand.Range(0, Find.WorldGrid.TilesCount);
				Tile tile = Find.WorldGrid[num];
				if (tile.biome.canBuildBase && tile.biome.implemented && Find.FactionManager.FactionAtTile(num) == null && tile.hilliness != Hilliness.Impassable)
				{
					return num;
				}
			}
			Log.Error("Found no starting world tile.");
			return 0;
		}

		public static int RandomFactionBaseTileFor(Faction faction)
		{
			for (int i = 0; i < 150; i++)
			{
				int num;
				if (Find.WorldGrid.TileIndices.TryRandomElementByWeight(delegate(int x)
				{
					Tile tile = Find.WorldGrid[x];
					if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
					{
						return 0f;
					}
					return tile.biome.factionBaseSelectionWeight;
				}, out num))
				{
					if (Find.FactionManager.FactionAtTile(num) == null)
					{
						return num;
					}
				}
			}
			Log.Error("Failed to find faction base tile for " + faction);
			return 0;
		}

		public static bool TryFindPassableTileWithTraversalDistance(int rootTile, int minDist, int maxDist, out int result, Predicate<int> validator = null, bool ignoreFirstTilePassability = false)
		{
			TileFinder.tmpTiles.Clear();
			WorldFloodFiller.FloodFill(rootTile, (int x) => !Find.World.Impassable(x) || (x == rootTile && ignoreFirstTilePassability), delegate(int tile, int traversalDistance)
			{
				if (traversalDistance > maxDist)
				{
					return true;
				}
				if (traversalDistance >= minDist && (validator == null || validator(tile)))
				{
					TileFinder.tmpTiles.Add(new Pair<int, int>(tile, traversalDistance));
				}
				return false;
			}, 2147483647);
			Pair<int, int> pair;
			if (TileFinder.tmpTiles.TryRandomElementByWeight((Pair<int, int> x) => 1f - (float)(x.Second - minDist) / ((float)(maxDist - minDist) + 0.01f), out pair))
			{
				result = pair.First;
				return true;
			}
			result = -1;
			return false;
		}
	}
}
