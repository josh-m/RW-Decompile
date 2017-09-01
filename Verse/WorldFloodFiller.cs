using RimWorld.Planet;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class WorldFloodFiller
	{
		private Queue<int> openSet = new Queue<int>();

		private List<int> traversalDistance = new List<int>();

		private List<int> visited = new List<int>();

		public void FloodFill(int rootTile, Predicate<int> passCheck, Action<int> processor, int maxTilesToProcess = 2147483647)
		{
			this.FloodFill(rootTile, passCheck, delegate(int tile, int traversalDistance)
			{
				processor(tile);
				return false;
			}, maxTilesToProcess);
		}

		public void FloodFill(int rootTile, Predicate<int> passCheck, Action<int, int> processor, int maxTilesToProcess = 2147483647)
		{
			this.FloodFill(rootTile, passCheck, delegate(int tile, int traversalDistance)
			{
				processor(tile, traversalDistance);
				return false;
			}, maxTilesToProcess);
		}

		public void FloodFill(int rootTile, Predicate<int> passCheck, Predicate<int> processor, int maxTilesToProcess = 2147483647)
		{
			this.FloodFill(rootTile, passCheck, (int tile, int traversalDistance) => processor(tile), maxTilesToProcess);
		}

		public void FloodFill(int rootTile, Predicate<int> passCheck, Func<int, int, bool> processor, int maxTilesToProcess = 2147483647)
		{
			if (rootTile < 0)
			{
				Log.Error("Flood fill with rootTile=" + rootTile);
				return;
			}
			ProfilerThreadCheck.BeginSample("WorldFloodFill");
			this.ClearVisited();
			if (!passCheck(rootTile))
			{
				ProfilerThreadCheck.EndSample();
				return;
			}
			int tilesCount = Find.WorldGrid.TilesCount;
			int num = tilesCount;
			if (this.traversalDistance.Count != tilesCount)
			{
				this.traversalDistance.Clear();
				for (int i = 0; i < tilesCount; i++)
				{
					this.traversalDistance.Add(-1);
				}
			}
			WorldGrid worldGrid = Find.WorldGrid;
			List<int> tileIDToNeighbors_offsets = worldGrid.tileIDToNeighbors_offsets;
			List<int> tileIDToNeighbors_values = worldGrid.tileIDToNeighbors_values;
			int num2 = 0;
			this.visited.Add(rootTile);
			this.traversalDistance[rootTile] = 0;
			this.openSet.Clear();
			this.openSet.Enqueue(rootTile);
			while (this.openSet.Count > 0)
			{
				int num3 = this.openSet.Dequeue();
				int num4 = this.traversalDistance[num3];
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
				for (int j = tileIDToNeighbors_offsets[num3]; j < num5; j++)
				{
					int num6 = tileIDToNeighbors_values[j];
					if (this.traversalDistance[num6] == -1 && passCheck(num6))
					{
						this.visited.Add(num6);
						this.openSet.Enqueue(num6);
						this.traversalDistance[num6] = num4 + 1;
					}
				}
				if (this.openSet.Count > num)
				{
					Log.Error("Overflow on world flood fill (>" + num + " cells). Make sure we're not flooding over the same area after we check it.");
					ProfilerThreadCheck.EndSample();
					return;
				}
			}
			ProfilerThreadCheck.EndSample();
		}

		private void ClearVisited()
		{
			int i = 0;
			int count = this.visited.Count;
			while (i < count)
			{
				this.traversalDistance[this.visited[i]] = -1;
				i++;
			}
			this.visited.Clear();
			this.openSet.Clear();
		}
	}
}
