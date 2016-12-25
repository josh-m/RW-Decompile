using System;
using Verse;

namespace RimWorld
{
	public class CompTerrainPumpDry : CompTerrainPump
	{
		protected override void AffectCell(IntVec3 c)
		{
			TerrainDef terrain = c.GetTerrain(this.parent.Map);
			if (terrain.driesTo == null)
			{
				return;
			}
			this.parent.Map.terrainGrid.SetTerrain(c, terrain.driesTo);
		}
	}
}
