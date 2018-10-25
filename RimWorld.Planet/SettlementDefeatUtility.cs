using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public static class SettlementDefeatUtility
	{
		public static void CheckDefeated(Settlement factionBase)
		{
			if (factionBase.Faction == Faction.OfPlayer)
			{
				return;
			}
			Map map = factionBase.Map;
			if (map == null || !SettlementDefeatUtility.IsDefeated(map, factionBase.Faction))
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("LetterFactionBaseDefeated".Translate(factionBase.Label, TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(60000)));
			if (!SettlementDefeatUtility.HasAnyOtherBase(factionBase))
			{
				factionBase.Faction.defeated = true;
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("LetterFactionBaseDefeated_FactionDestroyed".Translate(factionBase.Faction.Name));
			}
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				if (!current.def.hidden && !current.IsPlayer && current != factionBase.Faction && current.HostileTo(factionBase.Faction))
				{
					FactionRelationKind playerRelationKind = current.PlayerRelationKind;
					if (current.TryAffectGoodwillWith(Faction.OfPlayer, 20, false, false, null, null))
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine();
						stringBuilder.Append("RelationsWith".Translate(current.Name) + ": " + 20.ToStringWithSign());
						current.TryAppendRelationKindChangedInfo(stringBuilder, playerRelationKind, current.PlayerRelationKind, null);
					}
				}
			}
			Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), stringBuilder.ToString(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(factionBase.Tile), factionBase.Faction, null);
			DestroyedSettlement destroyedSettlement = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement);
			destroyedSettlement.Tile = factionBase.Tile;
			Find.WorldObjects.Add(destroyedSettlement);
			map.info.parent = destroyedSettlement;
			Find.WorldObjects.Remove(factionBase);
			destroyedSettlement.GetComponent<TimedForcedExit>().StartForceExitAndRemoveMapCountdown();
			TaleRecorder.RecordTale(TaleDefOf.CaravanAssaultSuccessful, new object[]
			{
				map.mapPawns.FreeColonists.RandomElement<Pawn>()
			});
		}

		private static bool IsDefeated(Map map, Faction faction)
		{
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i];
				if (pawn.RaceProps.Humanlike && GenHostility.IsActiveThreatToPlayer(pawn))
				{
					return false;
				}
			}
			return true;
		}

		private static bool HasAnyOtherBase(Settlement defeatedFactionBase)
		{
			List<Settlement> settlements = Find.WorldObjects.Settlements;
			for (int i = 0; i < settlements.Count; i++)
			{
				Settlement settlement = settlements[i];
				if (settlement.Faction == defeatedFactionBase.Faction && settlement != defeatedFactionBase)
				{
					return true;
				}
			}
			return false;
		}
	}
}
