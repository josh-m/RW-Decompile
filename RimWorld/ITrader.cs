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

		IEnumerable<Thing> ColonyThingsWillingToBuy
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

		void AddToStock(Thing thing);

		void GiveSoldThingToBuyer(Thing toGive, Thing originalThingFromStock);
	}
}
