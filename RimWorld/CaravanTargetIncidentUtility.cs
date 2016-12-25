using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public static class CaravanTargetIncidentUtility
	{
		public static Map GenerateOrGetMapForIncident(int width, int height, Caravan caravan, CaravanEnterMode enterMode, WorldObjectDef mapParentDef, MapGeneratorDef mapGenerator = null, bool draftColonists = false)
		{
			Map map = Current.Game.FindMap(caravan.Tile);
			if (map == null)
			{
				MapParent mapParent = null;
				if (mapParentDef != null)
				{
					mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(mapParentDef);
					mapParent.Tile = caravan.Tile;
					Find.WorldObjects.Add(mapParent);
				}
				map = MapGenerator.GenerateMap(new IntVec3(width, 1, height), caravan.Tile, mapParent, null, mapGenerator);
			}
			if (enterMode != CaravanEnterMode.None)
			{
				CaravanEnterMapUtility.Enter(caravan, map, enterMode, CaravanDropInventoryMode.DoNotDrop, draftColonists, null);
			}
			return map;
		}
	}
}
