using System;
using Verse;

namespace RimWorld
{
	public sealed class FertilityGrid
	{
		private Map map;

		public FertilityGrid(Map map)
		{
			this.map = map;
		}

		public float FertilityAt(IntVec3 loc)
		{
			return this.CalculateFertilityAt(loc);
		}

		private float CalculateFertilityAt(IntVec3 loc)
		{
			Thing edifice = loc.GetEdifice(this.map);
			if (edifice != null && edifice.def.fertility >= 0f)
			{
				return edifice.def.fertility;
			}
			return this.map.terrainGrid.TerrainAt(loc).fertility;
		}
	}
}
