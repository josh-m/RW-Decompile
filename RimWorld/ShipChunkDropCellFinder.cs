using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class ShipChunkDropCellFinder
	{
		public static bool TryFindShipChunkDropCell(out IntVec3 pos, IntVec3 nearLoc, int maxDist)
		{
			ThingDef chunkDef = ThingDefOf.ShipChunk;
			return CellFinder.TryFindRandomCellNear(nearLoc, maxDist, delegate(IntVec3 x)
			{
				foreach (IntVec3 current in GenAdj.OccupiedRect(x, Rot4.North, chunkDef.size))
				{
					if (!current.InBounds() || current.Fogged() || !current.Standable() || (current.Roofed() && current.GetRoof().isThickRoof))
					{
						bool result = false;
						return result;
					}
					if (!current.SupportsStructureType(chunkDef.terrainAffordanceNeeded))
					{
						bool result = false;
						return result;
					}
					List<Thing> thingList = current.GetThingList();
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
