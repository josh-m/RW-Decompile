using RimWorld.BaseGen;
using System;
using Verse;

namespace RimWorld
{
	public class GenStep_FactionBase : GenStep_Scatterer
	{
		private static readonly IntRange FactionBaseSizeRange = new IntRange(22, 23);

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			return base.CanScatterAt(c, map) && c.Standable(map) && !c.Roofed(map) && map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false));
		}

		protected override void ScatterAt(IntVec3 c, Map map, int stackCount = 1)
		{
			int randomInRange = GenStep_FactionBase.FactionBaseSizeRange.RandomInRange;
			int randomInRange2 = GenStep_FactionBase.FactionBaseSizeRange.RandomInRange;
			CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
			Faction faction;
			if (map.info.parent == null || map.info.parent.Faction == null || map.info.parent.Faction == Faction.OfPlayer)
			{
				faction = Find.FactionManager.RandomEnemyFaction(false, false);
			}
			else
			{
				faction = map.info.parent.Faction;
			}
			if (FactionBaseSymbolResolverUtility.ShouldUseSandbags(faction))
			{
				rect = rect.ExpandedBy(4);
			}
			rect.ClipInsideMap(map);
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = rect;
			resolveParams.faction = faction;
			BaseGen.globalSettings.map = map;
			BaseGen.symbolStack.Push("factionBase", resolveParams);
			BaseGen.Generate();
		}
	}
}
