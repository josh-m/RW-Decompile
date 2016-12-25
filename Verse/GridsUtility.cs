using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GridsUtility
	{
		public static float GetTemperature(this IntVec3 loc, Map map)
		{
			return GenTemperature.GetTemperatureForCell(loc, map);
		}

		public static Region GetRegion(this IntVec3 loc, Map map)
		{
			return map.regionGrid.GetValidRegionAt(loc);
		}

		public static Region GetRegion(this Thing thing)
		{
			if (!thing.Spawned)
			{
				return null;
			}
			return thing.Position.GetRegion(thing.Map);
		}

		public static Room GetRoom(this IntVec3 loc, Map map)
		{
			return RoomQuery.RoomAt(loc, map);
		}

		public static Room GetRoom(this Thing t)
		{
			return RoomQuery.RoomAt(t);
		}

		public static Room GetRoomOrAdjacent(this IntVec3 loc, Map map)
		{
			Room room = RoomQuery.RoomAt(loc, map);
			if (room != null)
			{
				return room;
			}
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = loc + GenAdj.AdjacentCells[i];
				room = RoomQuery.RoomAt(c, map);
				if (room != null)
				{
					return room;
				}
			}
			return room;
		}

		public static List<Thing> GetThingList(this IntVec3 c, Map map)
		{
			return map.thingGrid.ThingsListAt(c);
		}

		public static float GetSnowDepth(this IntVec3 c, Map map)
		{
			return map.snowGrid.GetDepth(c);
		}

		public static bool Fogged(this IntVec3 c, Map map)
		{
			return map.fogGrid.IsFogged(c);
		}

		public static RoofDef GetRoof(this IntVec3 c, Map map)
		{
			return map.roofGrid.RoofAt(c);
		}

		public static bool Roofed(this IntVec3 c, Map map)
		{
			return map.roofGrid.Roofed(c);
		}

		public static bool Filled(this IntVec3 c, Map map)
		{
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice.def.Fillage == FillCategory.Full;
		}

		public static TerrainDef GetTerrain(this IntVec3 c, Map map)
		{
			return map.terrainGrid.TerrainAt(c);
		}

		public static Zone GetZone(this IntVec3 c, Map map)
		{
			return map.zoneManager.ZoneAt(c);
		}

		public static Thing GetRegionBarrier(this IntVec3 c, Map map)
		{
			return c.GetThingList(map).Find((Thing x) => x.def.regionBarrier);
		}

		public static Plant GetPlant(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.category == ThingCategory.Plant)
				{
					return (Plant)list[i];
				}
			}
			return null;
		}

		public static Thing GetFirstThing(this IntVec3 c, Map map, ThingDef def)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def == def)
				{
					return thingList[i];
				}
			}
			return null;
		}

		public static Thing GetFirstHaulable(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.designateHaulable)
				{
					return list[i];
				}
			}
			return null;
		}

		public static Thing GetFirstItem(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.category == ThingCategory.Item)
				{
					return list[i];
				}
			}
			return null;
		}

		public static Building GetFirstBuilding(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Building building = list[i] as Building;
				if (building != null)
				{
					return building;
				}
			}
			return null;
		}

		public static Pawn GetFirstPawn(this IntVec3 c, Map map)
		{
			return (Pawn)c.GetThingList(map).Find((Thing x) => x is Pawn);
		}

		public static Building GetTransmitter(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.EverTransmitsPower)
				{
					return (Building)list[i];
				}
			}
			return null;
		}

		public static Building_Door GetDoor(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Building_Door building_Door = list[i] as Building_Door;
				if (building_Door != null)
				{
					return building_Door;
				}
			}
			return null;
		}

		public static Building GetEdifice(this IntVec3 c, Map map)
		{
			return map.edificeGrid[c];
		}

		public static Thing GetCover(this IntVec3 c, Map map)
		{
			return map.coverGrid[c];
		}

		public static bool IsInPrisonCell(this IntVec3 c, Map map)
		{
			Room room = RoomQuery.RoomAt(c, map);
			if (room != null)
			{
				return room.isPrisonCell;
			}
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				Room room2 = RoomQuery.RoomAt(c2, map);
				if (room2 != null)
				{
					return room2.isPrisonCell;
				}
			}
			Log.Error("Checking prison cell status of " + c + " which is not in or adjacent to a room.");
			return false;
		}

		public static bool UsesOutdoorTemperature(this IntVec3 c, Map map)
		{
			Room room = c.GetRoom(map);
			if (room != null)
			{
				return room.UsesOutdoorTemperature;
			}
			if (!c.Impassable(map))
			{
				return true;
			}
			Building edifice = c.GetEdifice(map);
			if (edifice != null)
			{
				IntVec3[] array = GenAdj.CellsAdjacent8Way(edifice).ToArray<IntVec3>();
				for (int i = 0; i < array.Count<IntVec3>(); i++)
				{
					if (array[i].InBounds(map) && !array[i].Impassable(map))
					{
						room = array[i].GetRoom(map);
						if (room == null || room.UsesOutdoorTemperature)
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}
	}
}
