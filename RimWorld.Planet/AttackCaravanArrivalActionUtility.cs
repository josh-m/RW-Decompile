using System;
using Verse;

namespace RimWorld.Planet
{
	public static class AttackCaravanArrivalActionUtility
	{
		public static Map GenerateFactionBaseMap(FactionBase factionBase)
		{
			MapGeneratorDef factionBaseMapGenerator = MapGeneratorDefOf.FactionBaseMapGenerator;
			return MapGenerator.GenerateMap(Find.World.info.initialMapSize, factionBase.Tile, factionBase, null, factionBaseMapGenerator);
		}
	}
}
