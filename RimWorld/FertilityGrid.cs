using System;
using Verse;

namespace RimWorld
{
	public sealed class FertilityGrid
	{
		public float FertilityAt(IntVec3 loc)
		{
			return this.CalculateFertilityAt(loc);
		}

		private float CalculateFertilityAt(IntVec3 loc)
		{
			Thing edifice = loc.GetEdifice();
			if (edifice != null && edifice.def.fertility >= 0f)
			{
				return edifice.def.fertility;
			}
			return Find.TerrainGrid.TerrainAt(loc).fertility;
		}
	}
}
