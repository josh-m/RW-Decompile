using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class GenPlanetMorphology
	{
		private static HashSet<int> tmpOutput = new HashSet<int>();

		private static HashSet<int> tilesSet = new HashSet<int>();

		private static List<int> tmpNeighbors = new List<int>();

		private static List<int> tmpEdgeTiles = new List<int>();

		public static void Erode(List<int> tiles, int count, Predicate<int> extraPredicate = null)
		{
			if (count <= 0)
			{
				return;
			}
			WorldGrid worldGrid = Find.WorldGrid;
			GenPlanetMorphology.tilesSet.Clear();
			GenPlanetMorphology.tilesSet.AddRange(tiles);
			GenPlanetMorphology.tmpEdgeTiles.Clear();
			for (int i = 0; i < tiles.Count; i++)
			{
				worldGrid.GetTileNeighbors(tiles[i], GenPlanetMorphology.tmpNeighbors);
				for (int j = 0; j < GenPlanetMorphology.tmpNeighbors.Count; j++)
				{
					if (!GenPlanetMorphology.tilesSet.Contains(GenPlanetMorphology.tmpNeighbors[j]))
					{
						GenPlanetMorphology.tmpEdgeTiles.Add(tiles[i]);
						break;
					}
				}
			}
			if (!GenPlanetMorphology.tmpEdgeTiles.Any<int>())
			{
				return;
			}
			GenPlanetMorphology.tmpOutput.Clear();
			Predicate<int> predicate;
			if (extraPredicate != null)
			{
				predicate = ((int x) => GenPlanetMorphology.tilesSet.Contains(x) && extraPredicate(x));
			}
			else
			{
				predicate = ((int x) => GenPlanetMorphology.tilesSet.Contains(x));
			}
			WorldFloodFiller arg_13F_0 = Find.WorldFloodFiller;
			int rootTile = -1;
			Predicate<int> passCheck = predicate;
			Func<int, int, bool> processor = delegate(int tile, int traversalDist)
			{
				if (traversalDist >= count)
				{
					GenPlanetMorphology.tmpOutput.Add(tile);
				}
				return false;
			};
			List<int> extraRootTiles = GenPlanetMorphology.tmpEdgeTiles;
			arg_13F_0.FloodFill(rootTile, passCheck, processor, 2147483647, extraRootTiles);
			tiles.Clear();
			tiles.AddRange(GenPlanetMorphology.tmpOutput);
		}

		public static void Dilate(List<int> tiles, int count, Predicate<int> extraPredicate = null)
		{
			if (count <= 0)
			{
				return;
			}
			WorldFloodFiller arg_6D_0 = Find.WorldFloodFiller;
			int rootTile = -1;
			Predicate<int> arg_4D_0 = extraPredicate;
			if (extraPredicate == null)
			{
				arg_4D_0 = ((int x) => true);
			}
			Predicate<int> passCheck = arg_4D_0;
			Func<int, int, bool> processor = delegate(int tile, int traversalDist)
			{
				if (traversalDist > count)
				{
					return true;
				}
				if (traversalDist != 0)
				{
					tiles.Add(tile);
				}
				return false;
			};
			List<int> tiles2 = tiles;
			arg_6D_0.FloodFill(rootTile, passCheck, processor, 2147483647, tiles2);
		}

		public static void Open(List<int> tiles, int count)
		{
			GenPlanetMorphology.Erode(tiles, count, null);
			GenPlanetMorphology.Dilate(tiles, count, null);
		}

		public static void Close(List<int> tiles, int count)
		{
			GenPlanetMorphology.Dilate(tiles, count, null);
			GenPlanetMorphology.Erode(tiles, count, null);
		}
	}
}
