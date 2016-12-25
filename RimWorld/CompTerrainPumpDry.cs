using System;
using Verse;

namespace RimWorld
{
	public class CompTerrainPumpDry : CompTerrainPump
	{
		protected override void AffectCell(IntVec3 c)
		{
			TerrainDef terrain = c.GetTerrain();
			if (terrain.driesTo == null)
			{
				return;
			}
			Find.TerrainGrid.SetTerrain(c, terrain.driesTo);
		}
	}
}
