using System;
using System.Collections.Generic;

namespace Verse
{
	public class FloodFiller
	{
		private Map map;

		private Queue<IntVec3> openSet = new Queue<IntVec3>();

		private BoolGrid queuedGrid;

		private CellGrid parentGrid;

		private List<int> visited = new List<int>();

		public FloodFiller(Map map)
		{
			this.map = map;
			this.queuedGrid = new BoolGrid(map);
		}

		public void FloodFill(IntVec3 root, Predicate<IntVec3> passCheck, Action<IntVec3> processor, bool rememberParents = false)
		{
			this.FloodFill(root, passCheck, delegate(IntVec3 x)
			{
				processor(x);
				return false;
			}, rememberParents);
		}

		public void FloodFill(IntVec3 root, Predicate<IntVec3> passCheck, Func<IntVec3, bool> processor, bool rememberParents = false)
		{
			ProfilerThreadCheck.BeginSample("FloodFill");
			this.ClearVisited();
			if (rememberParents && this.parentGrid == null)
			{
				this.parentGrid = new CellGrid(this.map);
			}
			if (!passCheck(root))
			{
				if (rememberParents)
				{
					this.parentGrid[root] = IntVec3.Invalid;
				}
				ProfilerThreadCheck.EndSample();
				return;
			}
			int area = this.map.Area;
			IntVec3[] cardinalDirectionsAround = GenAdj.CardinalDirectionsAround;
			int num = cardinalDirectionsAround.Length;
			CellIndices cellIndices = this.map.cellIndices;
			int num2 = cellIndices.CellToIndex(root);
			this.visited.Add(num2);
			this.queuedGrid.Set(num2, true);
			if (rememberParents)
			{
				this.parentGrid[num2] = root;
			}
			this.openSet.Clear();
			this.openSet.Enqueue(root);
			while (this.openSet.Count > 0)
			{
				IntVec3 intVec = this.openSet.Dequeue();
				if (processor(intVec))
				{
					break;
				}
				for (int i = 0; i < num; i++)
				{
					IntVec3 intVec2 = intVec + cardinalDirectionsAround[i];
					int num3 = cellIndices.CellToIndex(intVec2);
					if (intVec2.InBounds(this.map) && !this.queuedGrid[num3] && passCheck(intVec2))
					{
						this.visited.Add(num3);
						this.openSet.Enqueue(intVec2);
						this.queuedGrid.Set(num3, true);
						if (rememberParents)
						{
							this.parentGrid[num3] = intVec;
						}
					}
				}
				if (this.openSet.Count > area)
				{
					Log.Error("Overflow on flood fill (>" + area + " cells). Make sure we're not flooding over the same area after we check it.");
					ProfilerThreadCheck.EndSample();
					return;
				}
			}
			ProfilerThreadCheck.EndSample();
		}

		public void ReconstructLastFloodFillPath(IntVec3 dest, List<IntVec3> outPath)
		{
			outPath.Clear();
			if (this.parentGrid == null || !dest.InBounds(this.map) || !this.parentGrid[dest].IsValid)
			{
				return;
			}
			int num = 0;
			int num2 = this.map.Area + 1;
			IntVec3 intVec = dest;
			while (true)
			{
				num++;
				if (num > num2)
				{
					break;
				}
				if (!intVec.IsValid)
				{
					goto Block_4;
				}
				outPath.Add(intVec);
				if (this.parentGrid[intVec] == intVec)
				{
					goto Block_5;
				}
				intVec = this.parentGrid[intVec];
			}
			Log.Error("Too many iterations.");
			Block_4:
			Block_5:
			outPath.Reverse();
		}

		private void ClearVisited()
		{
			int i = 0;
			int count = this.visited.Count;
			while (i < count)
			{
				int index = this.visited[i];
				this.queuedGrid[index] = false;
				if (this.parentGrid != null)
				{
					this.parentGrid[index] = IntVec3.Invalid;
				}
				i++;
			}
			this.visited.Clear();
			this.openSet.Clear();
		}
	}
}
