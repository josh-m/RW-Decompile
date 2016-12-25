using System;
using System.Collections.Generic;

namespace Verse
{
	public static class FloodFiller
	{
		private static Queue<IntVec3> openSet = new Queue<IntVec3>();

		private static BoolGrid queuedGrid = new BoolGrid();

		private static List<IntVec3> visited = new List<IntVec3>();

		public static void FloodFill(IntVec3 root, Predicate<IntVec3> passCheck, Action<IntVec3> processor)
		{
			ProfilerThreadCheck.BeginSample("FloodFill");
			if (!passCheck(root))
			{
				ProfilerThreadCheck.EndSample();
				return;
			}
			int area = Find.Map.Area;
			if (FloodFiller.queuedGrid.InnerArray.Length != Find.Map.Area)
			{
				FloodFiller.queuedGrid = new BoolGrid();
			}
			if (FloodFiller.visited.Any<IntVec3>())
			{
				FloodFiller.queuedGrid.Clear();
				FloodFiller.visited.Clear();
			}
			IntVec3[] cardinalDirectionsAround = GenAdj.CardinalDirectionsAround;
			int num = cardinalDirectionsAround.Length;
			FloodFiller.queuedGrid.Set(root, true);
			FloodFiller.visited.Add(root);
			FloodFiller.openSet.Clear();
			FloodFiller.openSet.Enqueue(root);
			while (FloodFiller.openSet.Count > 0)
			{
				IntVec3 intVec = FloodFiller.openSet.Dequeue();
				processor(intVec);
				for (int i = 0; i < num; i++)
				{
					IntVec3 intVec2 = intVec + cardinalDirectionsAround[i];
					if (intVec2.InBounds() && !FloodFiller.queuedGrid[intVec2] && passCheck(intVec2))
					{
						FloodFiller.openSet.Enqueue(intVec2);
						FloodFiller.queuedGrid.Set(intVec2, true);
						FloodFiller.visited.Add(intVec2);
					}
				}
				if (FloodFiller.openSet.Count > area)
				{
					Log.Error("Overflow on flood fill (>" + area + " cells). Make sure we're not flooding over the same area after we check it.");
					FloodFiller.ClearVisited();
					ProfilerThreadCheck.EndSample();
					return;
				}
			}
			FloodFiller.ClearVisited();
			ProfilerThreadCheck.EndSample();
		}

		private static void ClearVisited()
		{
			int i = 0;
			int count = FloodFiller.visited.Count;
			while (i < count)
			{
				FloodFiller.queuedGrid[FloodFiller.visited[i]] = false;
				i++;
			}
			FloodFiller.visited.Clear();
		}
	}
}
