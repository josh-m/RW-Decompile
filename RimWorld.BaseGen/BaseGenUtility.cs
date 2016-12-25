using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public static class BaseGenUtility
	{
		public static ThingDef RandomCheapWallStuff(Faction faction, bool notVeryFlammable = false)
		{
			if (faction != null && faction.def.techLevel.IsNeolithicOrWorse())
			{
				return ThingDefOf.WoodLog;
			}
			return (from d in DefDatabase<ThingDef>.AllDefsListForReading
			where d.IsStuff && d.stuffProps.CanMake(ThingDefOf.Wall) && (!notVeryFlammable || d.BaseFlammability < 0.5f) && d.BaseMarketValue / d.VolumePerUnit < 5f
			select d).RandomElement<ThingDef>();
		}

		public static TerrainDef RandomBasicFloorDef()
		{
			float value = Rand.Value;
			if (value < 0.25f)
			{
				return TerrainDefOf.MetalTile;
			}
			if (value < 0.5f)
			{
				return TerrainDefOf.PavedTile;
			}
			if (value < 0.75f)
			{
				return TerrainDefOf.WoodPlankFloor;
			}
			return TerrainDefOf.TileSandstone;
		}

		public static TerrainDef CorrespondingTerrainDef(ThingDef stuffDef)
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

		public static bool AnyDoorAdjacentToCorner(CellRect rect, Map map, int corner)
		{
			IntVec3 cornerPos = BaseGenUtility.GetCornerPos(rect, corner);
			return BaseGenUtility.AnyDoorAdjacentTo(cornerPos, map);
		}

		public static bool AnyDoorAdjacentTo(IntVec3 cell, Map map)
		{
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = cell + GenAdj.CardinalDirections[i];
				if (c.InBounds(map))
				{
					if (c.GetThingList(map).Any((Thing x) => x is Building_Door))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static IntVec3 GetCornerPos(CellRect rect, int corner)
		{
			switch (corner)
			{
			case 0:
				return new IntVec3(rect.minX, 0, rect.minZ);
			case 1:
				return new IntVec3(rect.maxX, 0, rect.minZ);
			case 2:
				return new IntVec3(rect.minX, 0, rect.maxZ);
			case 3:
				return new IntVec3(rect.maxX, 0, rect.maxZ);
			default:
				throw new InvalidOperationException("corner");
			}
		}

		public static bool TryFindRandomNonDoorBlockingCorner(CellRect rect, Map map, out int corner, List<int> exceptThese = null)
		{
			return Rand.TryRangeInclusiveWhere(0, 3, (int x) => !BaseGenUtility.AnyDoorAdjacentToCorner(rect, map, x) && BaseGenUtility.GetCornerPos(rect, x).GetFirstBuilding(map) == null && (exceptThese == null || !exceptThese.Contains(x)), out corner);
		}
	}
}
