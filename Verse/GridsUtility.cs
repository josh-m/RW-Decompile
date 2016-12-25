using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GridsUtility
	{
		public static float GetTemperature(this IntVec3 loc)
		{
			return GenTemperature.GetTemperatureForCell(loc);
		}

		public static Region GetRegion(this IntVec3 loc)
		{
			return Find.RegionGrid.GetValidRegionAt(loc);
		}

		public static Room GetRoom(this IntVec3 loc)
		{
			return RoomQuery.RoomAt(loc);
		}

		public static Room GetRoomOrAdjacent(this IntVec3 loc)
		{
			Room room = RoomQuery.RoomAt(loc);
			if (room != null)
			{
				return room;
			}
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = loc + GenAdj.AdjacentCells[i];
				room = RoomQuery.RoomAt(c);
				if (room != null)
				{
					return room;
				}
			}
			return room;
		}

		public static Room GetRoom(this Thing t)
		{
			return RoomQuery.RoomAt(t.Position);
		}

		public static List<Thing> GetThingList(this IntVec3 c)
		{
			return Find.ThingGrid.ThingsListAt(c);
		}

		public static float GetSnowDepth(this IntVec3 c)
		{
			return Find.SnowGrid.GetDepth(c);
		}

		public static bool Fogged(this IntVec3 c)
		{
			return Find.FogGrid.IsFogged(c);
		}

		public static RoofDef GetRoof(this IntVec3 c)
		{
			return Find.RoofGrid.RoofAt(c);
		}

		public static bool Roofed(this IntVec3 c)
		{
			return Find.RoofGrid.Roofed(c);
		}

		public static bool Filled(this IntVec3 c)
		{
			Building edifice = c.GetEdifice();
			return edifice != null && edifice.def.Fillage == FillCategory.Full;
		}

		public static TerrainDef GetTerrain(this IntVec3 c)
		{
			return Find.TerrainGrid.TerrainAt(c);
		}

		public static Zone GetZone(this IntVec3 c)
		{
			return Find.ZoneManager.ZoneAt(c);
		}

		public static Thing GetRegionBarrier(this IntVec3 c)
		{
			return c.GetThingList().Find((Thing x) => x.def.regionBarrier);
		}

		public static Plant GetPlant(this IntVec3 c)
		{
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.category == ThingCategory.Plant)
				{
					return (Plant)list[i];
				}
			}
			return null;
		}

		public static Thing GetFirstThing(this IntVec3 c, ThingDef def)
		{
			List<Thing> thingList = c.GetThingList();
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def == def)
				{
					return thingList[i];
				}
			}
			return null;
		}

		public static Thing GetFirstHaulable(this IntVec3 c)
		{
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.designateHaulable)
				{
					return list[i];
				}
			}
			return null;
		}

		public static Thing GetFirstItem(this IntVec3 c)
		{
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.category == ThingCategory.Item)
				{
					return list[i];
				}
			}
			return null;
		}

		public static Pawn GetFirstPawn(this IntVec3 c)
		{
			return (Pawn)c.GetThingList().Find((Thing x) => x is Pawn);
		}

		public static Building GetTransmitter(this IntVec3 c)
		{
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.EverTransmitsPower)
				{
					return (Building)list[i];
				}
			}
			return null;
		}

		public static Building GetEdifice(this IntVec3 c)
		{
			return Find.EdificeGrid[c];
		}

		public static Thing GetCover(this IntVec3 c)
		{
			return Find.CoverGrid[c];
		}

		public static bool IsInPrisonCell(this IntVec3 c)
		{
			Room room = RoomQuery.RoomAt(c);
			if (room != null)
			{
				return room.isPrisonCell;
			}
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				Room room2 = RoomQuery.RoomAt(c2);
				if (room2 != null)
				{
					return room2.isPrisonCell;
				}
			}
			Log.Error("Checking prison cell status of " + c + " which is not in or adjacent to a room.");
			return false;
		}

		public static bool UsesOutdoorTemperature(this IntVec3 c)
		{
			Room room = c.GetRoom();
			if (room != null)
			{
				return room.UsesOutdoorTemperature;
			}
			if (!c.Impassable())
			{
				return true;
			}
			Building edifice = c.GetEdifice();
			if (edifice != null)
			{
				IntVec3[] array = GenAdj.CellsAdjacent8Way(edifice).ToArray<IntVec3>();
				for (int i = 0; i < array.Count<IntVec3>(); i++)
				{
					if (array[i].InBounds() && !array[i].Impassable())
					{
						room = array[i].GetRoom();
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
