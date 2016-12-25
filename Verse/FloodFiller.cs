using System;
using System.Collections.Generic;

namespace Verse
{
	public class FloodFiller
	{
		private Map map;

		private Queue<IntVec3> openSet = new Queue<IntVec3>();

		private BoolGrid queuedGrid;

		private List<IntVec3> visited = new List<IntVec3>();

		public FloodFiller(Map map)
		{
			this.map = map;
			this.queuedGrid = new BoolGrid(map);
		}

		public void FloodFill(IntVec3 root, Predicate<IntVec3> passCheck, Action<IntVec3> processor)
		{
			ProfilerThreadCheck.BeginSample("FloodFill");
			if (!passCheck(root))
			{
				ProfilerThreadCheck.EndSample();
				return;
			}
			int area = this.map.Area;
			if (this.visited.Any<IntVec3>())
			{
				this.queuedGrid.Clear();
				this.visited.Clear();
			}
			IntVec3[] cardinalDirectionsAround = GenAdj.CardinalDirectionsAround;
			int num = cardinalDirectionsAround.Length;
			this.queuedGrid.Set(root, true);
			this.visited.Add(root);
			this.openSet.Clear();
			this.openSet.Enqueue(root);
			while (this.openSet.Count > 0)
			{
				IntVec3 intVec = this.openSet.Dequeue();
				processor(intVec);
				for (int i = 0; i < num; i++)
				{
					IntVec3 intVec2 = intVec + cardinalDirectionsAround[i];
					if (intVec2.InBounds(this.map) && !this.queuedGrid[intVec2] && passCheck(intVec2))
					{
						this.openSet.Enqueue(intVec2);
						this.queuedGrid.Set(intVec2, true);
						this.visited.Add(intVec2);
					}
				}
				if (this.openSet.Count > area)
				{
					Log.Error("Overflow on flood fill (>" + area + " cells). Make sure we're not flooding over the same area after we check it.");
					this.ClearVisited();
					ProfilerThreadCheck.EndSample();
					return;
				}
			}
			this.ClearVisited();
			ProfilerThreadCheck.EndSample();
		}

		private void ClearVisited()
		{
			int i = 0;
			int count = this.visited.Count;
			while (i < count)
			{
				this.queuedGrid[this.visited[i]] = false;
				i++;
			}
			this.visited.Clear();
		}
	}
}
