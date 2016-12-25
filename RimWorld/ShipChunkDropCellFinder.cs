using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class ShipChunkDropCellFinder
	{
		public static bool TryFindShipChunkDropCell(IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos)
		{
			ThingDef chunkDef = ThingDefOf.ShipChunk;
			return CellFinder.TryFindRandomCellNear(nearLoc, map, maxDist, delegate(IntVec3 x)
			{
				foreach (IntVec3 current in GenAdj.OccupiedRect(x, Rot4.North, chunkDef.size))
				{
					if (!current.InBounds(map) || current.Fogged(map) || !current.Standable(map) || (current.Roofed(map) && current.GetRoof(map).isThickRoof))
					{
						bool result = false;
						return result;
					}
					if (!current.SupportsStructureType(map, chunkDef.terrainAffordanceNeeded))
					{
						bool result = false;
						return result;
					}
					List<Thing> thingList = current.GetThingList(map);
					for (int i = 0; i < thingList.Count; i++)
					{
						Thing thing = thingList[i];
						if (thing.def.category != ThingCategory.Plant && GenSpawn.SpawningWipes(chunkDef, thing.def))
						{
							bool result = false;
							return result;
						}
					}
				}
				return true;
			}, out pos);
		}
	}
}
