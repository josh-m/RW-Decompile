using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class CaravanVisitUtility
	{
		private static readonly Texture2D TradeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/Trade", true);

		public static Pawn BestNegotiator(Caravan caravan)
		{
			Pawn pawn = null;
			float num = -1f;
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Pawn pawn2 = pawnsListForReading[i];
				if (!pawn2.Downed && !pawn2.InMentalState && caravan.IsOwner(pawn2))
				{
					float statValue = pawn2.GetStatValue(StatDefOf.TradePriceImprovement, true);
					if (pawn == null || statValue > num)
					{
						pawn = pawn2;
						num = statValue;
					}
				}
			}
			return pawn;
		}

		public static FactionBase FactionBaseVisitedNow(Caravan caravan)
		{
			if (!caravan.Spawned || caravan.pather.Moving)
			{
				return null;
			}
			List<FactionBase> factionBases = Find.WorldObjects.FactionBases;
			for (int i = 0; i < factionBases.Count; i++)
			{
				FactionBase factionBase = factionBases[i];
				if (factionBase.Tile == caravan.Tile && factionBase.Faction != caravan.Faction && !factionBase.Faction.HostileTo(caravan.Faction))
				{
					return factionBase;
				}
			}
			return null;
		}

		public static Command TradeCommand(Caravan caravan)
		{
			Pawn bestNegotiator = CaravanVisitUtility.BestNegotiator(caravan);
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandTrade".Translate();
			command_Action.defaultDesc = "CommandTradeDesc".Translate();
			command_Action.icon = CaravanVisitUtility.TradeCommandTex;
			command_Action.action = delegate
			{
				FactionBase factionBase = CaravanVisitUtility.FactionBaseVisitedNow(caravan);
				if (factionBase != null && factionBase.CanTradeNow)
				{
					Find.WindowStack.Add(new Dialog_Trade(bestNegotiator, factionBase));
					string empty = string.Empty;
					string empty2 = string.Empty;
					PawnRelationUtility.Notify_PawnsSeenByPlayer(factionBase.Goods.OfType<Pawn>(), ref empty, ref empty2, "LetterRelatedPawnsTradingWithFactionBase".Translate(), false);
					if (!empty2.NullOrEmpty())
					{
						Find.LetterStack.ReceiveLetter(empty, empty2, LetterType.Good, factionBase, null);
					}
				}
			};
			if (bestNegotiator == null)
			{
				command_Action.Disable("CommandTradeFailNoNegotiator".Translate());
			}
			return command_Action;
		}
	}
}
