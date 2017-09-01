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
			if (this.parent.Map.Biome == BiomeDefOf.SeaIce)
			{
				this.parent.Map.terrainGrid.SetTerrain(c, TerrainDefOf.Ice);
			}
			else
			{
				this.parent.Map.terrainGrid.SetTerrain(c, terrain.driesTo);
			}
		}
	}
}
