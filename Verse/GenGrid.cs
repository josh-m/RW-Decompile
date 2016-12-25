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

		public static bool InNoBuildEdgeArea(this IntVec3 c)
		{
			return c.CloseToEdge(10);
		}

		public static bool InNoZoneEdgeArea(this IntVec3 c)
		{
			return c.CloseToEdge(5);
		}

		public static bool CloseToEdge(this IntVec3 c, int edgeDist)
		{
			return c.x < edgeDist || c.z < edgeDist || c.x >= Find.Map.Size.x - edgeDist || c.z >= Find.Map.Size.z - edgeDist;
		}

		public static bool OnEdge(this IntVec3 c)
		{
			return c.x == 0 || c.x == Find.Map.Size.x - 1 || c.z == 0 || c.z == Find.Map.Size.z - 1;
		}

		public static bool InBounds(this IntVec3 c)
		{
			return c.x >= 0 && c.z >= 0 && c.x < Find.Map.Size.x && c.z < Find.Map.Size.z;
		}

		public static bool InBounds(this Vector3 v)
		{
			return v.x >= 0f && v.z >= 0f && v.x < (float)Find.Map.Size.x && v.z < (float)Find.Map.Size.z;
		}

		public static bool Walkable(this IntVec3 c)
		{
			return Find.PathGrid.Walkable(c);
		}

		public static bool Standable(this IntVec3 c)
		{
			if (!Find.PathGrid.Walkable(c))
			{
				return false;
			}
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.passability != Traversability.Standable)
				{
					return false;
				}
			}
			return true;
		}

		public static bool Impassable(this IntVec3 c)
		{
			List<Thing> list = Find.ThingGrid.ThingsListAtFast(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.passability == Traversability.Impassable)
				{
					return true;
				}
			}
			return false;
		}

		public static bool SupportsStructureType(this IntVec3 c, TerrainAffordance surfaceType)
		{
			return c.GetTerrain().affordances.Contains(surfaceType);
		}

		public static bool CanBeSeenOver(this IntVec3 c)
		{
			if (!c.InBounds())
			{
				return false;
			}
			Building edifice = c.GetEdifice();
			if (edifice != null && edifice.def.Fillage == FillCategory.Full)
			{
				Building_Door building_Door = edifice as Building_Door;
				return building_Door != null && building_Door.Open;
			}
			return true;
		}

		public static bool CanBeSeenOverFast(this IntVec3 c)
		{
			Building edifice = c.GetEdifice();
			if (edifice != null && edifice.def.Fillage == FillCategory.Full)
			{
				Building_Door building_Door = edifice as Building_Door;
				return building_Door != null && building_Door.Open;
			}
			return true;
		}

		public static bool HasEatSurface(this IntVec3 c)
		{
			if (!c.InBounds())
			{
				return false;
			}
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
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
