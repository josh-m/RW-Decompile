using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public interface ITrader
	{
		TraderKindDef TraderKind
		{
			get;
		}

		IEnumerable<Thing> Goods
		{
			get;
		}

		int RandomPriceFactorSeed
		{
			get;
		}

		string TraderName
		{
			get;
		}

		bool CanTradeNow
		{
			get;
		}

		float TradePriceImprovementOffsetForPlayer
		{
			get;
		}

		IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator);

		void AddToStock(Thing thing, Pawn playerNegotiator);

		void GiveSoldThingToPlayer(Thing toGive, Thing originalThingFromStock, Pawn playerNegotiator);
	}
}
