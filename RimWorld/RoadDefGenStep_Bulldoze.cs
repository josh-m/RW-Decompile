using System;
using Verse;

namespace RimWorld
{
	public class RoadDefGenStep_Bulldoze : RoadDefGenStep
	{
		public override void Place(Map map, IntVec3 tile, TerrainDef rockDef, IntVec3 origin, GenStep_Roads.DistanceElement[,] distance)
		{
			while (tile.Impassable(map))
			{
				foreach (Thing current in tile.GetThingList(map))
				{
					if (current.def.passability == Traversability.Impassable)
					{
						current.Destroy(DestroyMode.Vanish);
						break;
					}
				}
			}
		}
	}
}
