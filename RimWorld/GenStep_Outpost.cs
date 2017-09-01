using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_Outpost : GenStep
	{
		private const int Size = 16;

		private static List<CellRect> possibleRects = new List<CellRect>();

		public override void Generate(Map map)
		{
			CellRect rectToDefend;
			if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out rectToDefend))
			{
				rectToDefend = CellRect.SingleCell(map.Center);
			}
			Faction faction;
			if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
			{
				faction = Find.FactionManager.RandomEnemyFaction(false, false, true);
			}
			else
			{
				faction = map.ParentFaction;
			}
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = this.GetOutpostRect(rectToDefend, map);
			resolveParams.faction = faction;
			resolveParams.edgeDefenseWidth = new int?(2);
			resolveParams.edgeDefenseTurretsCount = new int?(Rand.RangeInclusive(0, 1));
			resolveParams.edgeDefenseMortarsCount = new int?(0);
			resolveParams.factionBasePawnGroupPointsFactor = new float?(0.4f);
			BaseGen.globalSettings.map = map;
			BaseGen.globalSettings.minBuildings = 1;
			BaseGen.globalSettings.minBarracks = 1;
			BaseGen.symbolStack.Push("factionBase", resolveParams);
			BaseGen.Generate();
		}

		private CellRect GetOutpostRect(CellRect rectToDefend, Map map)
		{
			GenStep_Outpost.possibleRects.Add(new CellRect(rectToDefend.minX - 1 - 16, rectToDefend.CenterCell.z - 8, 16, 16));
			GenStep_Outpost.possibleRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - 8, 16, 16));
			GenStep_Outpost.possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - 8, rectToDefend.minZ - 1 - 16, 16, 16));
			GenStep_Outpost.possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - 8, rectToDefend.maxZ + 1, 16, 16));
			CellRect mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
			GenStep_Outpost.possibleRects.RemoveAll((CellRect x) => !x.FullyContainedWithin(mapRect));
			if (GenStep_Outpost.possibleRects.Any<CellRect>())
			{
				return GenStep_Outpost.possibleRects.RandomElement<CellRect>();
			}
			return rectToDefend;
		}
	}
}
