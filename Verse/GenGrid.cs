using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class GenGrid
	{
		public const int NoBuildEdgeWidth = 10;

		public const int NoZoneEdgeWidth = 5;

		public static bool InNoBuildEdgeArea(this IntVec3 c, Map map)
		{
			return c.CloseToEdge(map, 10);
		}

		public static bool InNoZoneEdgeArea(this IntVec3 c, Map map)
		{
			return c.CloseToEdge(map, 5);
		}

		public static bool CloseToEdge(this IntVec3 c, Map map, int edgeDist)
		{
			return c.x < edgeDist || c.z < edgeDist || c.x >= map.Size.x - edgeDist || c.z >= map.Size.z - edgeDist;
		}

		public static bool OnEdge(this IntVec3 c, Map map)
		{
			return c.x == 0 || c.x == map.Size.x - 1 || c.z == 0 || c.z == map.Size.z - 1;
		}

		public static bool InBounds(this IntVec3 c, Map map)
		{
			return c.x >= 0 && c.z >= 0 && c.x < map.Size.x && c.z < map.Size.z;
		}

		public static bool InBounds(this Vector3 v, Map map)
		{
			return v.x >= 0f && v.z >= 0f && v.x < (float)map.Size.x && v.z < (float)map.Size.z;
		}

		public static bool Walkable(this IntVec3 c, Map map)
		{
			return map.pathGrid.Walkable(c);
		}

		public static bool Standable(this IntVec3 c, Map map)
		{
			if (!map.pathGrid.Walkable(c))
			{
				return false;
			}
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.passability != Traversability.Standable)
				{
					return false;
				}
			}
			return true;
		}

		public static bool Impassable(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAtFast(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.passability == Traversability.Impassable)
				{
					return true;
				}
			}
			return false;
		}

		public static bool SupportsStructureType(this IntVec3 c, Map map, TerrainAffordance surfaceType)
		{
			return c.GetTerrain(map).affordances.Contains(surfaceType);
		}

		public static bool CanBeSeenOver(this IntVec3 c, Map map)
		{
			if (!c.InBounds(map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(map);
			return edifice == null || edifice.CanBeSeenOver();
		}

		public static bool CanBeSeenOverFast(this IntVec3 c, Map map)
		{
			Building edifice = c.GetEdifice(map);
			return edifice == null || edifice.CanBeSeenOver();
		}

		public static bool CanBeSeenOver(this Building b)
		{
			if (b.def.Fillage == FillCategory.Full)
			{
				Building_Door building_Door = b as Building_Door;
				return building_Door != null && building_Door.Open;
			}
			return true;
		}

		public static bool HasEatSurface(this IntVec3 c, Map map)
		{
			if (!c.InBounds(map))
			{
				return false;
			}
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.surfaceType == SurfaceType.Eat)
				{
					return true;
				}
			}
			return false;
		}
	}
}
