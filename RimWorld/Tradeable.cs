using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Tradeable : Transferable
	{
		public List<Thing> thingsColony = new List<Thing>();

		public List<Thing> thingsTrader = new List<Thing>();

		public string editBuffer;

		private const float MinimumBuyPrice = 0.5f;

		private const float MinimumSellPrice = 0.01f;

		public const float PriceFactorBuy_Global = 1.5f;

		public const float PriceFactorSell_Global = 0.5f;

		private float pricePlayerBuy = -1f;

		private float pricePlayerSell = -1f;

		private float priceFactorBuy_TraderPriceType;

		private float priceFactorSell_TraderPriceType;

		private float priceFactorSell_ItemSellPriceFactor;

		private float priceGain_PlayerNegotiator;

		private float priceGain_FactionBase;

		public Thing FirstThingColony
		{
			get
			{
				if (this.thingsColony.Count == 0)
				{
					return null;
				}
				return this.thingsColony[0];
			}
		}

		public Thing FirstThingTrader
		{
			get
			{
				if (this.thingsTrader.Count == 0)
				{
					return null;
				}
				return this.thingsTrader[0];
			}
		}

		public override string Label
		{
			get
			{
				return this.AnyThing.LabelCapNoCount;
			}
		}

		public virtual float BaseMarketValue
		{
			get
			{
				return this.AnyThing.MarketValue;
			}
		}

		public override bool Interactive
		{
			get
			{
				return !this.IsCurrency;
			}
		}

		public bool TraderWillTrade
		{
			get
			{
				return TradeSession.trader.TraderKind.WillTrade(this.AnyThing.def);
			}
		}

		public override bool HasAnyThing
		{
			get
			{
				return this.FirstThingColony != null || this.FirstThingTrader != null;
			}
		}

		public override Thing AnyThing
		{
			get
			{
				if (this.FirstThingColony != null)
				{
					return this.FirstThingColony.GetInnerIfMinified();
				}
				if (this.FirstThingTrader != null)
				{
					return this.FirstThingTrader.GetInnerIfMinified();
				}
				Log.Error(base.GetType() + " lacks AnyThing.");
				return null;
			}
		}

		public override ThingDef ThingDef
		{
			get
			{
				if (!this.HasAnyThing)
				{
					return null;
				}
				return this.AnyThing.def;
			}
		}

		public ThingDef StuffDef
		{
			get
			{
				return this.AnyThing.Stuff;
			}
		}

		public override string TipDescription
		{
			get
			{
				if (!this.HasAnyThing)
				{
					return string.Empty;
				}
				return this.AnyThing.GetDescription();
			}
		}

		public TradeAction ActionToDo
		{
			get
			{
				if (base.CountToTransfer == 0)
				{
					return TradeAction.None;
				}
				if (base.CountToTransfer > 0)
				{
					return TradeAction.PlayerBuys;
				}
				return TradeAction.PlayerSells;
			}
		}

		public bool IsCurrency
		{
			get
			{
				return !this.Bugged && this.ThingDef == ThingDefOf.Silver;
			}
		}

		public override TransferablePositiveCountDirection PositiveCountDirection
		{
			get
			{
				return TransferablePositiveCountDirection.Source;
			}
		}

		public float CurTotalSilverCost
		{
			get
			{
				if (this.ActionToDo == TradeAction.None)
				{
					return 0f;
				}
				return (float)base.CountToTransfer * this.GetPriceFor(this.ActionToDo);
			}
		}

		public virtual Window NewInfoDialog
		{
			get
			{
				return new Dialog_InfoCard(this.ThingDef);
			}
		}

		private bool Bugged
		{
			get
			{
				if (!this.HasAnyThing)
				{
					Log.ErrorOnce(this.ToString() + " is bugged. There will be no more logs about this.", 162112);
					return true;
				}
				return false;
			}
		}

		public Tradeable()
		{
		}

		public Tradeable(Thing thingColony, Thing thingTrader)
		{
			this.thingsColony.Add(thingColony);
			this.thingsTrader.Add(thingTrader);
		}

		public void AddThing(Thing t, Transactor trans)
		{
			if (trans == Transactor.Colony)
			{
				this.thingsColony.Add(t);
			}
			if (trans == Transactor.Trader)
			{
				this.thingsTrader.Add(t);
			}
		}

		public PriceType PriceTypeFor(TradeAction action)
		{
			return TradeSession.trader.TraderKind.PriceTypeFor(this.ThingDef, action);
		}

		private void InitPriceDataIfNeeded()
		{
			if (this.pricePlayerBuy > 0f)
			{
				return;
			}
			this.priceFactorBuy_TraderPriceType = this.PriceTypeFor(TradeAction.PlayerBuys).PriceMultiplier();
			this.priceFactorSell_TraderPriceType = this.PriceTypeFor(TradeAction.PlayerSells).PriceMultiplier();
			this.priceGain_PlayerNegotiator = TradeSession.playerNegotiator.GetStatValue(StatDefOf.TradePriceImprovement, true);
			this.priceGain_FactionBase = TradeSession.trader.TradePriceImprovementOffsetForPlayer;
			this.pricePlayerBuy = this.BaseMarketValue * 1.5f * this.priceFactorBuy_TraderPriceType * (1f + Find.Storyteller.difficulty.tradePriceFactorLoss);
			this.pricePlayerBuy *= 1f - this.priceGain_PlayerNegotiator - this.priceGain_FactionBase;
			this.pricePlayerBuy = Mathf.Max(this.pricePlayerBuy, 0.5f);
			if (this.pricePlayerBuy > 99.5f)
			{
				this.pricePlayerBuy = Mathf.Round(this.pricePlayerBuy);
			}
			this.priceFactorSell_ItemSellPriceFactor = this.AnyThing.GetStatValue(StatDefOf.SellPriceFactor, true);
			this.pricePlayerSell = this.BaseMarketValue * 0.5f * this.priceFactorSell_TraderPriceType * this.priceFactorSell_ItemSellPriceFactor * (1f - Find.Storyteller.difficulty.tradePriceFactorLoss);
			this.pricePlayerSell *= 1f + this.priceGain_PlayerNegotiator + this.priceGain_FactionBase;
			this.pricePlayerSell = Mathf.Max(this.pricePlayerSell, 0.01f);
			if (this.pricePlayerSell > 99.5f)
			{
				this.pricePlayerSell = Mathf.Round(this.pricePlayerSell);
			}
			if (this.pricePlayerSell >= this.pricePlayerBuy)
			{
				Log.ErrorOnce("Trying to put player-sells price above player-buys price for " + this.AnyThing, 65387);
				this.pricePlayerSell = this.pricePlayerBuy;
			}
		}

		public string GetPriceTooltip(TradeAction action)
		{
			if (!this.HasAnyThing)
			{
				return string.Empty;
			}
			this.InitPriceDataIfNeeded();
			string text = (action != TradeAction.PlayerBuys) ? "SellPriceDesc".Translate() : "BuyPriceDesc".Translate();
			text += "\n\n";
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				StatDefOf.MarketValue.LabelCap,
				": ",
				this.BaseMarketValue
			});
			if (action == TradeAction.PlayerBuys)
			{
				text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					"\n  x ",
					1.5f.ToString("F2"),
					" (",
					"Buying".Translate(),
					")"
				});
				if (this.priceFactorBuy_TraderPriceType != 1f)
				{
					text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"\n  x ",
						this.priceFactorBuy_TraderPriceType.ToString("F2"),
						" (",
						"TraderTypePrice".Translate(),
						")"
					});
				}
				if (Find.Storyteller.difficulty.tradePriceFactorLoss != 0f)
				{
					text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"\n  x ",
						(1f + Find.Storyteller.difficulty.tradePriceFactorLoss).ToString("F2"),
						" (",
						"DifficultyLevel".Translate(),
						")"
					});
				}
				text += "\n";
				text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					"\n",
					"YourNegotiatorBonus".Translate(),
					": -",
					this.priceGain_PlayerNegotiator.ToStringPercent()
				});
				if (this.priceGain_FactionBase != 0f)
				{
					text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"\n",
						"TradeWithFactionBaseBonus".Translate(),
						": -",
						this.priceGain_FactionBase.ToStringPercent()
					});
				}
			}
			else
			{
				text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					"\n  x ",
					0.5f.ToString("F2"),
					" (",
					"Selling".Translate(),
					")"
				});
				if (this.priceFactorSell_TraderPriceType != 1f)
				{
					text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"\n  x ",
						this.priceFactorSell_TraderPriceType.ToString("F2"),
						" (",
						"TraderTypePrice".Translate(),
						")"
					});
				}
				if (this.priceFactorSell_ItemSellPriceFactor != 1f)
				{
					text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"\n  x ",
						this.priceFactorSell_ItemSellPriceFactor.ToString("F2"),
						" (",
						"ItemSellPriceFactor".Translate(),
						")"
					});
				}
				if (Find.Storyteller.difficulty.tradePriceFactorLoss != 0f)
				{
					text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"\n  x ",
						(1f - Find.Storyteller.difficulty.tradePriceFactorLoss).ToString("F2"),
						" (",
						"DifficultyLevel".Translate(),
						")"
					});
				}
				text += "\n";
				text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					"\n",
					"YourNegotiatorBonus".Translate(),
					": ",
					this.priceGain_PlayerNegotiator.ToStringPercent()
				});
				if (this.priceGain_FactionBase != 0f)
				{
					text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"\n",
						"TradeWithFactionBaseBonus".Translate(),
						": ",
						this.priceGain_FactionBase.ToStringPercent()
					});
				}
			}
			text += "\n\n";
			float priceFor = this.GetPriceFor(action);
			text = text + "FinalPrice".Translate() + ": $" + priceFor.ToString("F2");
			if ((action == TradeAction.PlayerBuys && priceFor <= 0.5f) || (action == TradeAction.PlayerBuys && priceFor <= 0.01f))
			{
				text = text + " (" + "minimum".Translate() + ")";
			}
			return text;
		}

		public float GetPriceFor(TradeAction action)
		{
			this.InitPriceDataIfNeeded();
			if (action == TradeAction.PlayerBuys)
			{
				return this.pricePlayerBuy;
			}
			return this.pricePlayerSell;
		}

		public override int GetMinimum()
		{
			return -this.CountHeldBy(Transactor.Colony);
		}

		public override int GetMaximum()
		{
			return this.CountHeldBy(Transactor.Trader);
		}

		public override AcceptanceReport UnderflowReport()
		{
			return new AcceptanceReport("ColonyHasNoMore".Translate());
		}

		public override AcceptanceReport OverflowReport()
		{
			return new AcceptanceReport("TraderHasNoMore".Translate());
		}

		private List<Thing> TransactorThings(Transactor trans)
		{
			if (trans == Transactor.Colony)
			{
				return this.thingsColony;
			}
			return this.thingsTrader;
		}

		public int CountHeldBy(Transactor trans)
		{
			List<Thing> list = this.TransactorThings(trans);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				num += list[i].stackCount;
			}
			return num;
		}

		public int CountPostDealFor(Transactor trans)
		{
			if (trans == Transactor.Colony)
			{
				return this.CountHeldBy(trans) + base.CountToTransfer;
			}
			return this.CountHeldBy(trans) - base.CountToTransfer;
		}

		public virtual void ResolveTrade()
		{
			if (this.ActionToDo == TradeAction.PlayerSells)
			{
				TransferableUtility.TransferNoSplit(this.thingsColony, -base.CountToTransfer, delegate(Thing thing, int countToTransfer)
				{
					TradeSession.trader.GiveSoldThingToTrader(thing, countToTransfer, TradeSession.playerNegotiator);
				}, true, true);
			}
			else if (this.ActionToDo == TradeAction.PlayerBuys)
			{
				TransferableUtility.TransferNoSplit(this.thingsTrader, base.CountToTransfer, delegate(Thing thing, int countToTransfer)
				{
					this.CheckTeachOpportunity(thing, countToTransfer);
					TradeSession.trader.GiveSoldThingToPlayer(thing, countToTransfer, TradeSession.playerNegotiator);
				}, true, true);
			}
		}

		private void CheckTeachOpportunity(Thing boughtThing, int boughtCount)
		{
			Building building = boughtThing as Building;
			if (building == null)
			{
				MinifiedThing minifiedThing = boughtThing as MinifiedThing;
				if (minifiedThing != null)
				{
					building = (minifiedThing.InnerThing as Building);
				}
			}
			if (building != null && building.def.building != null && building.def.building.boughtConceptLearnOpportunity != null)
			{
				LessonAutoActivator.TeachOpportunity(building.def.building.boughtConceptLearnOpportunity, OpportunityType.GoodToKnow);
			}
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				base.GetType(),
				"(",
				this.ThingDef,
				", countToDrop=",
				base.CountToTransfer,
				")"
			});
		}

		public override int GetHashCode()
		{
			return this.AnyThing.GetHashCode();
		}
	}
}
