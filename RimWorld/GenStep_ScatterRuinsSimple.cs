using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterRuinsSimple : GenStep_Scatterer
	{
		public IntRange shedSizeRange = new IntRange(3, 10);

		public IntRange wallLengthRange = new IntRange(4, 14);

		protected override bool CanScatterAt(IntVec3 loc)
		{
			return base.CanScatterAt(loc) && loc.SupportsStructureType(TerrainAffordance.Heavy);
		}

		protected ThingDef RandomWallStuff()
		{
			return (from d in DefDatabase<ThingDef>.AllDefs
			where d.IsStuff && d.stuffProps.CanMake(ThingDefOf.Wall) && d.BaseFlammability < 0.5f && d.BaseMarketValue / d.VolumePerUnit < 15f
			select d).RandomElement<ThingDef>();
		}

		protected TerrainDef CorrespondingTileDef(ThingDef stuffDef)
		{
			TerrainDef terrainDef = null;
			List<TerrainDef> allDefsListForReading = DefDatabase<TerrainDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].costList != null)
				{
					for (int j = 0; j < allDefsListForReading[i].costList.Count; j++)
					{
						if (allDefsListForReading[i].costList[j].thingDef == stuffDef)
						{
							terrainDef = allDefsListForReading[i];
							break;
						}
					}
				}
				if (terrainDef != null)
				{
					break;
				}
			}
			if (terrainDef == null)
			{
				terrainDef = TerrainDefOf.Concrete;
			}
			return terrainDef;
		}

		protected override void ScatterAt(IntVec3 loc, int stackCount = 1)
		{
			ThingDef stuffDef = this.RandomWallStuff();
			if (Rand.Value < 0.5f)
			{
				bool horizontal = Rand.Value < 0.5f;
				this.MakeLongWall(loc, this.wallLengthRange.RandomInRange, horizontal, stuffDef);
			}
			else
			{
				IntVec3 intVec = loc;
				CellRect mapRect = new CellRect(intVec.x, intVec.z, this.shedSizeRange.RandomInRange, this.shedSizeRange.RandomInRange);
				this.MakeShed(mapRect, stuffDef, true);
			}
		}

		private void TrySetCellAsWall(IntVec3 c, ThingDef stuffDef)
		{
			List<Thing> thingList = c.GetThingList();
			for (int i = 0; i < thingList.Count; i++)
			{
				if (!thingList[i].def.destroyable)
				{
					return;
				}
			}
			for (int j = thingList.Count - 1; j >= 0; j--)
			{
				thingList[j].Destroy(DestroyMode.Vanish);
			}
			Find.TerrainGrid.SetTerrain(c, this.CorrespondingTileDef(stuffDef));
			ThingDef wall = ThingDefOf.Wall;
			Thing newThing = ThingMaker.MakeThing(wall, stuffDef);
			GenSpawn.Spawn(newThing, c);
		}

		private void MakeLongWall(IntVec3 start, int extendDist, bool horizontal, ThingDef stuffDef)
		{
			TerrainDef newTerr = this.CorrespondingTileDef(stuffDef);
			IntVec3 intVec = start;
			for (int i = 0; i < extendDist; i++)
			{
				if (!intVec.InBounds())
				{
					return;
				}
				this.TrySetCellAsWall(intVec, stuffDef);
				if (Rand.Value < 0.4f)
				{
					for (int j = 0; j < 9; j++)
					{
						IntVec3 c = intVec + GenAdj.AdjacentCellsAndInside[j];
						if (c.InBounds())
						{
							if (Rand.Value < 0.5f)
							{
								Find.TerrainGrid.SetTerrain(c, newTerr);
							}
						}
					}
				}
				if (horizontal)
				{
					intVec.x++;
				}
				else
				{
					intVec.z++;
				}
			}
		}

		public void MakeShed(CellRect mapRect, ThingDef stuffDef, bool leaveDoorGaps = true)
		{
			mapRect.ClipInsideMap();
			foreach (IntVec3 current in mapRect)
			{
				if (current.x == mapRect.minX || current.x == mapRect.maxX || current.z == mapRect.minZ || current.z == mapRect.maxZ)
				{
					if (!leaveDoorGaps || Rand.Value >= 0.1f)
					{
						this.TrySetCellAsWall(current, stuffDef);
					}
				}
				else
				{
					Building edifice = current.GetEdifice();
					if (edifice != null)
					{
						edifice.Destroy(DestroyMode.Vanish);
					}
					Find.TerrainGrid.SetTerrain(current, this.CorrespondingTileDef(stuffDef));
				}
			}
		}
	}
}
