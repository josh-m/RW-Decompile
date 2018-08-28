using System;
using System.Collections.Generic;

namespace Verse
{
	public class MapCellsInRandomOrder
	{
		private Map map;

		private List<IntVec3> randomizedCells;

		public MapCellsInRandomOrder(Map map)
		{
			this.map = map;
		}

		public List<IntVec3> GetAll()
		{
			this.CreateListIfShould();
			return this.randomizedCells;
		}

		public IntVec3 Get(int index)
		{
			this.CreateListIfShould();
			return this.randomizedCells[index];
		}

		private void CreateListIfShould()
		{
			if (this.randomizedCells != null)
			{
				return;
			}
			this.randomizedCells = new List<IntVec3>(this.map.Area);
			foreach (IntVec3 current in this.map.AllCells)
			{
				this.randomizedCells.Add(current);
			}
			Rand.PushState();
			Rand.Seed = (Find.World.info.Seed ^ this.map.Tile);
			this.randomizedCells.Shuffle<IntVec3>();
			Rand.PopState();
		}
	}
}
