using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterRuinsSimple : GenStep_Scatterer
	{
		public IntRange ShedSizeRange = new IntRange(3, 10);

		public IntRange WallLengthRange = new IntRange(4, 14);

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			return base.CanScatterAt(c, map) && c.SupportsStructureType(map, TerrainAffordance.Heavy);
		}

		protected override void ScatterAt(IntVec3 c, Map map, int stackCount = 1)
		{
			ThingDef stuffDef = BaseGenUtility.RandomCheapWallStuff(null, true);
			if (Rand.Bool)
			{
				bool @bool = Rand.Bool;
				this.MakeLongWall(c, map, this.WallLengthRange.RandomInRange, @bool, stuffDef);
			}
			else
			{
				CellRect cellRect = new CellRect(c.x, c.z, this.ShedSizeRange.RandomInRange, this.ShedSizeRange.RandomInRange);
				CellRect rect = cellRect.ClipInsideMap(map);
				BaseGen.globalSettings.map = map;
				BaseGen.symbolStack.Push("ancientRuins", rect);
				BaseGen.Generate();
			}
		}

		private void TrySetCellAsWall(IntVec3 c, Map map, ThingDef stuffDef)
		{
			List<Thing> thingList = c.GetThingList(map);
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
			map.terrainGrid.SetTerrain(c, BaseGenUtility.CorrespondingTerrainDef(stuffDef));
			Thing newThing = ThingMaker.MakeThing(ThingDefOf.Wall, stuffDef);
			GenSpawn.Spawn(newThing, c, map);
		}

		private void MakeLongWall(IntVec3 start, Map map, int extendDist, bool horizontal, ThingDef stuffDef)
		{
			TerrainDef newTerr = BaseGenUtility.CorrespondingTerrainDef(stuffDef);
			IntVec3 intVec = start;
			for (int i = 0; i < extendDist; i++)
			{
				if (!intVec.InBounds(map))
				{
					return;
				}
				this.TrySetCellAsWall(intVec, map, stuffDef);
				if (Rand.Chance(0.4f))
				{
					for (int j = 0; j < 9; j++)
					{
						IntVec3 c = intVec + GenAdj.AdjacentCellsAndInside[j];
						if (c.InBounds(map))
						{
							if (Rand.Bool)
							{
								map.terrainGrid.SetTerrain(c, newTerr);
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
	}
}
