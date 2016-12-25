using System;
using Verse;

namespace RimWorld
{
	public static class TradeSession
	{
		public static ITrader trader;

		public static Pawn playerNegotiator;

		public static TradeDeal deal;

		public static bool Active
		{
			get
			{
				return TradeSession.trader != null;
			}
		}

		public static void SetupWith(ITrader newTrader, Pawn newPlayerNegotiator)
		{
			if (!newTrader.CanTradeNow)
			{
				Log.Warning("Called SetupWith with a trader not willing to trade now.");
			}
			TradeSession.trader = newTrader;
			TradeSession.playerNegotiator = newPlayerNegotiator;
			TradeSession.deal = new TradeDeal();
			if (TradeSession.deal.cannotSellReasons.Count > 0)
			{
				Messages.Message("MessageCannotSellItemsReason".Translate() + GenText.ToCommaList(TradeSession.deal.cannotSellReasons, true), MessageSound.Negative);
			}
		}

		public static void Close()
		{
			TradeSession.trader = null;
		}
	}
}
