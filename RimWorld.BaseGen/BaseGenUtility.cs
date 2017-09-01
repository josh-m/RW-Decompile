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
			TechLevel techLevel = (faction != null) ? faction.def.techLevel : TechLevel.Spacer;
			return BaseGenUtility.RandomCheapWallStuff(techLevel, notVeryFlammable);
		}

		public static ThingDef RandomCheapWallStuff(TechLevel techLevel, bool notVeryFlammable = false)
		{
			if (techLevel.IsNeolithicOrWorse())
			{
				return ThingDefOf.WoodLog;
			}
			return (from d in DefDatabase<ThingDef>.AllDefsListForReading
			where BaseGenUtility.IsCheapWallStuff(d) && (!notVeryFlammable || d.BaseFlammability < 0.5f)
			select d).RandomElement<ThingDef>();
		}

		public static bool IsCheapWallStuff(ThingDef d)
		{
			return d.IsStuff && d.stuffProps.CanMake(ThingDefOf.Wall) && d.BaseMarketValue / d.VolumePerUnit < 5f;
		}

		public static TerrainDef RandomBasicFloorDef(Faction faction, bool allowCarpet = false)
		{
			if (allowCarpet && (faction == null || !faction.def.techLevel.IsNeolithicOrWorse()) && Rand.Chance(0.1f))
			{
				return (from x in DefDatabase<TerrainDef>.AllDefsListForReading
				where x.IsCarpet
				select x).RandomElement<TerrainDef>();
			}
			return Rand.Element<TerrainDef>(TerrainDefOf.MetalTile, TerrainDefOf.PavedTile, TerrainDefOf.WoodPlankFloor, TerrainDefOf.TileSandstone);
		}

		public static TerrainDef CorrespondingTerrainDef(ThingDef stuffDef, bool beautiful)
		{
			TerrainDef terrainDef = null;
			List<TerrainDef> allDefsListForReading = DefDatabase<TerrainDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].costList != null)
				{
					for (int j = 0; j < allDefsListForReading[i].costList.Count; j++)
					{
						if (allDefsListForReading[i].costList[j].thingDef == stuffDef && (terrainDef == null || ((!beautiful) ? (terrainDef.statBases.GetStatOffsetFromList(StatDefOf.Beauty) > allDefsListForReading[i].statBases.GetStatOffsetFromList(StatDefOf.Beauty)) : (terrainDef.statBases.GetStatOffsetFromList(StatDefOf.Beauty) < allDefsListForReading[i].statBases.GetStatOffsetFromList(StatDefOf.Beauty)))))
						{
							terrainDef = allDefsListForReading[i];
						}
					}
				}
			}
			if (terrainDef == null)
			{
				terrainDef = TerrainDefOf.Concrete;
			}
			return terrainDef;
		}

		public static TerrainDef RegionalRockTerrainDef(int tile, bool beautiful)
		{
			ThingDef thingDef = Find.World.NaturalRockTypesIn(tile).RandomElementWithFallback(null);
			ThingDef thingDef2 = (thingDef == null) ? null : thingDef.building.mineableThing;
			ThingDef stuffDef = (thingDef2 == null || thingDef2.butcherProducts == null || thingDef2.butcherProducts.Count <= 0) ? null : thingDef2.butcherProducts[0].thingDef;
			return BaseGenUtility.CorrespondingTerrainDef(stuffDef, beautiful);
		}

		public static bool AnyDoorCardinalAdjacentTo(IntVec3 cell, Map map)
		{
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = cell + GenAdj.CardinalDirections[i];
				if (c.InBounds(map) && c.GetDoor(map) != null)
				{
					return true;
				}
			}
			return false;
		}

		public static bool AnyDoorCardinalAdjacentTo(CellRect rect, Map map)
		{
			foreach (IntVec3 current in rect.ExpandedBy(1).EdgeCells)
			{
				if (!rect.IsCorner(current) && current.InBounds(map))
				{
					if (current.GetDoor(map) != null)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
