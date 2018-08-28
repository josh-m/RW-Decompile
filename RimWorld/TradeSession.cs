using System;
using Verse;

namespace RimWorld
{
	public static class TradeSession
	{
		public static ITrader trader;

		public static Pawn playerNegotiator;

		public static TradeDeal deal;

		public static bool giftMode;

		public static bool Active
		{
			get
			{
				return TradeSession.trader != null;
			}
		}

		public static void SetupWith(ITrader newTrader, Pawn newPlayerNegotiator, bool giftMode)
		{
			if (!newTrader.CanTradeNow)
			{
				Log.Warning("Called SetupWith with a trader not willing to trade now.", false);
			}
			TradeSession.trader = newTrader;
			TradeSession.playerNegotiator = newPlayerNegotiator;
			TradeSession.giftMode = giftMode;
			TradeSession.deal = new TradeDeal();
			if (!giftMode && TradeSession.deal.cannotSellReasons.Count > 0)
			{
				Messages.Message("MessageCannotSellItemsReason".Translate() + TradeSession.deal.cannotSellReasons.ToCommaList(true), MessageTypeDefOf.NegativeEvent, false);
			}
		}

		public static void Close()
		{
			TradeSession.trader = null;
		}
	}
}
