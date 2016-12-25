using System;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGrid
	{
		private WorldSquare[,] grid;

		public WorldGrid()
		{
			this.grid = new WorldSquare[Find.World.Size.x, Find.World.Size.z];
		}

		public void Set(IntVec2 sq, WorldSquare sqDef)
		{
			if (this.grid[sq.x, sq.z] == sqDef)
			{
				return;
			}
			this.grid[sq.x, sq.z] = sqDef;
		}

		public WorldSquare Get(IntVec2 sq)
		{
			return this.grid[sq.x, sq.z];
		}
	}
}
