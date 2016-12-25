using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterShrines : GenStep_ScatterRuinsSimple
	{
		private const int MarginCells = 1;

		private static readonly IntRange ShrinesCountX = new IntRange(1, 4);

		private static readonly IntRange ShrinesCountZ = new IntRange(1, 4);

		private static readonly IntRange ExtraHeightRange = new IntRange(0, 8);

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice.def.building.isNaturalRock;
		}

		protected override void ScatterAt(IntVec3 loc, Map map, int stackCount = 1)
		{
			int randomInRange = GenStep_ScatterShrines.ShrinesCountX.RandomInRange;
			int randomInRange2 = GenStep_ScatterShrines.ShrinesCountZ.RandomInRange;
			int randomInRange3 = GenStep_ScatterShrines.ExtraHeightRange.RandomInRange;
			IntVec2 standardAncientShrineSize = SymbolResolver_AncientShrinesGroup.StandardAncientShrineSize;
			int num = 1;
			int num2 = randomInRange * standardAncientShrineSize.x + (randomInRange - 1) * num;
			int num3 = randomInRange2 * standardAncientShrineSize.z + (randomInRange2 - 1) * num;
			int num4 = num2 + 2;
			int num5 = num3 + 2 + randomInRange3;
			CellRect rect = new CellRect(loc.x, loc.z, num4, num5);
			rect.ClipInsideMap(map);
			if (rect.Width != num4 || rect.Height != num5)
			{
				return;
			}
			foreach (IntVec3 current in rect.Cells)
			{
				List<Thing> list = map.thingGrid.ThingsListAt(current);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].def == ThingDefOf.AncientCryptosleepCasket)
					{
						return;
					}
				}
			}
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = rect;
			resolveParams.disableSinglePawn = new bool?(true);
			resolveParams.disableHives = new bool?(true);
			resolveParams.ancientTempleEntranceHeight = new int?(randomInRange3);
			BaseGen.globalSettings.map = map;
			BaseGen.symbolStack.Push("ancientTemple", resolveParams);
			BaseGen.Generate();
			RectTrigger rectTrigger = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger, null);
			rectTrigger.Rect = rect.ExpandedBy(1).ClipInsideMap(map);
			rectTrigger.letter = new Letter("LetterLabelAncientShrineWarning".Translate(), "AncientShrineWarning".Translate(), LetterType.BadNonUrgent, new TargetInfo(rect.CenterCell, map, false));
			rectTrigger.destroyIfUnfogged = true;
			GenSpawn.Spawn(rectTrigger, rect.CenterCell, map);
		}
	}
}
