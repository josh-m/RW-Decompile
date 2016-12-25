using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class StockGenerator
	{
		[Unsaved]
		public TraderKindDef trader;

		public IntRange countRange = IntRange.zero;

		public FloatRange totalPriceRange = FloatRange.Zero;

		public TechLevel maxTechLevelGenerate = TechLevel.Transcendent;

		public TechLevel maxTechLevelBuy = TechLevel.Transcendent;

		public PriceType price;

		public virtual void PostLoad()
		{
			if (this.price == PriceType.Undefined)
			{
				this.price = PriceType.Normal;
			}
		}

		public virtual void ResolveReferences(TraderKindDef trader)
		{
			this.trader = trader;
		}

		[DebuggerHidden]
		public virtual IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
		{
		}

		public abstract IEnumerable<Thing> GenerateThings();

		public abstract bool HandlesThingDef(ThingDef thingDef);

		public bool TryGetPriceType(ThingDef thingDef, TradeAction action, out PriceType priceType)
		{
			if (!this.HandlesThingDef(thingDef))
			{
				priceType = PriceType.Normal;
				return false;
			}
			if (action == TradeAction.PlayerBuys)
			{
				priceType = this.price;
			}
			else
			{
				priceType = (PriceType)(this.price - PriceType.VeryCheap);
			}
			return true;
		}

		protected int RandomCountOf(ThingDef def)
		{
			if (this.countRange.max > 0 && this.totalPriceRange.max <= 0f)
			{
				return this.countRange.RandomInRange;
			}
			if (this.countRange.max <= 0 && this.totalPriceRange.max > 0f)
			{
				return Mathf.Max(1, Mathf.RoundToInt(this.totalPriceRange.RandomInRange / def.BaseMarketValue));
			}
			int num = 0;
			int randomInRange;
			do
			{
				randomInRange = this.countRange.RandomInRange;
				num++;
				if (num > 100)
				{
					break;
				}
			}
			while (!this.totalPriceRange.Includes((float)randomInRange * def.BaseMarketValue));
			return randomInRange;
		}
	}
}
