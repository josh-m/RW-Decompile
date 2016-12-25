using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public static class FactionBaseDefeatUtility
	{
		public static void CheckDefeated(FactionBase factionBase)
		{
			if (factionBase.Faction == Faction.OfPlayer)
			{
				return;
			}
			Map map = factionBase.Map;
			if (map == null || !FactionBaseDefeatUtility.IsDefeated(map, factionBase.Faction))
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("LetterFactionBaseDefeated".Translate(new object[]
			{
				factionBase.Label,
				24
			}));
			if (!FactionBaseDefeatUtility.HasAnyOtherBase(factionBase))
			{
				factionBase.Faction.defeated = true;
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("LetterFactionBaseDefeated_FactionDestroyed".Translate(new object[]
				{
					factionBase.Faction.Name
				}));
			}
			Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), stringBuilder.ToString(), LetterType.Good, new GlobalTargetInfo(factionBase.Tile), null);
			DestroyedFactionBase destroyedFactionBase = (DestroyedFactionBase)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedFactionBase);
			destroyedFactionBase.Tile = factionBase.Tile;
			Find.WorldObjects.Add(destroyedFactionBase);
			map.info.parent = destroyedFactionBase;
			Find.WorldObjects.Remove(factionBase);
			destroyedFactionBase.StartForceExitAndRemoveMapCountdown();
		}

		private static bool IsDefeated(Map map, Faction faction)
		{
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i];
				if (pawn.RaceProps.Humanlike && !PawnUtility.ThreatDisabledOrFleeing(pawn))
				{
					return false;
				}
			}
			return true;
		}

		private static bool HasAnyOtherBase(FactionBase defeatedFactionBase)
		{
			List<FactionBase> factionBases = Find.WorldObjects.FactionBases;
			for (int i = 0; i < factionBases.Count; i++)
			{
				FactionBase factionBase = factionBases[i];
				if (factionBase.Faction == defeatedFactionBase.Faction && factionBase != defeatedFactionBase)
				{
					return true;
				}
			}
			return false;
		}
	}
}
