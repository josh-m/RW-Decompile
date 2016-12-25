using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class TraderKindDef : Def
	{
		public List<StockGenerator> stockGenerators = new List<StockGenerator>();

		public bool orbital;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			foreach (StockGenerator current in this.stockGenerators)
			{
				current.ResolveReferences(this);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string err in base.ConfigErrors())
			{
				yield return err;
			}
			foreach (StockGenerator stock in this.stockGenerators)
			{
				foreach (string err2 in stock.ConfigErrors(this))
				{
					yield return err2;
				}
			}
		}

		public bool WillTrade(ThingDef td)
		{
			for (int i = 0; i < this.stockGenerators.Count; i++)
			{
				if (this.stockGenerators[i].HandlesThingDef(td))
				{
					return true;
				}
			}
			return false;
		}

		public PriceType PriceTypeFor(ThingDef thingDef, TradeAction action)
		{
			if (thingDef == ThingDefOf.Silver)
			{
				return PriceType.Undefined;
			}
			if (action == TradeAction.PlayerBuys)
			{
				for (int i = 0; i < this.stockGenerators.Count; i++)
				{
					PriceType result;
					if (this.stockGenerators[i].TryGetPriceType(thingDef, action, out result))
					{
						return result;
					}
				}
			}
			return PriceType.Normal;
		}
	}
}
