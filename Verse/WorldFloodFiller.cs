using RimWorld.Planet;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class WorldFloodFiller
	{
		private static Queue<int> openSet = new Queue<int>();

		private static List<int> traversalDistance = new List<int>();

		private static List<int> visited = new List<int>();

		public static void FloodFill(int rootTile, Predicate<int> passCheck, Action<int> processor, int maxTilesToProcess = 2147483647)
		{
			WorldFloodFiller.FloodFill(rootTile, passCheck, delegate(int tile, int traversalDistance)
			{
				processor(tile);
				return false;
			}, maxTilesToProcess);
		}

		public static void FloodFill(int rootTile, Predicate<int> passCheck, Action<int, int> processor, int maxTilesToProcess = 2147483647)
		{
			WorldFloodFiller.FloodFill(rootTile, passCheck, delegate(int tile, int traversalDistance)
			{
				processor(tile, traversalDistance);
				return false;
			}, maxTilesToProcess);
		}

		public static void FloodFill(int rootTile, Predicate<int> passCheck, Predicate<int> processor, int maxTilesToProcess = 2147483647)
		{
			WorldFloodFiller.FloodFill(rootTile, passCheck, (int tile, int traversalDistance) => processor(tile), maxTilesToProcess);
		}

		public static void FloodFill(int rootTile, Predicate<int> passCheck, Func<int, int, bool> processor, int maxTilesToProcess = 2147483647)
		{
			ProfilerThreadCheck.BeginSample("WorldFloodFill");
			if (!passCheck(rootTile))
			{
				ProfilerThreadCheck.EndSample();
				return;
			}
			int tilesCount = Find.WorldGrid.TilesCount;
			int num = tilesCount;
			if (WorldFloodFiller.traversalDistance.Count != tilesCount)
			{
				WorldFloodFiller.traversalDistance.Clear();
				for (int i = 0; i < tilesCount; i++)
				{
					WorldFloodFiller.traversalDistance.Add(-1);
				}
			}
			if (WorldFloodFiller.visited.Any<int>())
			{
				for (int j = 0; j < tilesCount; j++)
				{
					WorldFloodFiller.traversalDistance[j] = -1;
				}
				WorldFloodFiller.visited.Clear();
			}
			WorldGrid worldGrid = Find.WorldGrid;
			List<int> tileIDToNeighbors_offsets = worldGrid.tileIDToNeighbors_offsets;
			List<int> tileIDToNeighbors_values = worldGrid.tileIDToNeighbors_values;
			int num2 = 0;
			WorldFloodFiller.traversalDistance[rootTile] = 0;
			WorldFloodFiller.visited.Add(rootTile);
			WorldFloodFiller.openSet.Clear();
			WorldFloodFiller.openSet.Enqueue(rootTile);
			while (WorldFloodFiller.openSet.Count > 0)
			{
				int num3 = WorldFloodFiller.openSet.Dequeue();
				int num4 = WorldFloodFiller.traversalDistance[num3];
				if (processor(num3, num4))
				{
					break;
				}
				num2++;
				if (num2 == maxTilesToProcess)
				{
					break;
				}
				int num5 = (num3 + 1 >= tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_values.Count : tileIDToNeighbors_offsets[num3 + 1];
				for (int k = tileIDToNeighbors_offsets[num3]; k < num5; k++)
				{
					int num6 = tileIDToNeighbors_values[k];
					if (WorldFloodFiller.traversalDistance[num6] == -1 && passCheck(num6))
					{
						WorldFloodFiller.openSet.Enqueue(num6);
						WorldFloodFiller.traversalDistance[num6] = num4 + 1;
						WorldFloodFiller.visited.Add(num6);
					}
				}
				if (WorldFloodFiller.openSet.Count > num)
				{
					Log.Error("Overflow on world flood fill (>" + num + " cells). Make sure we're not flooding over the same area after we check it.");
					WorldFloodFiller.ClearVisited();
					ProfilerThreadCheck.EndSample();
					return;
				}
			}
			WorldFloodFiller.ClearVisited();
			ProfilerThreadCheck.EndSample();
		}

		private static void ClearVisited()
		{
			int i = 0;
			int count = WorldFloodFiller.visited.Count;
			while (i < count)
			{
				WorldFloodFiller.traversalDistance[WorldFloodFiller.visited[i]] = -1;
				i++;
			}
			WorldFloodFiller.visited.Clear();
		}
	}
}
