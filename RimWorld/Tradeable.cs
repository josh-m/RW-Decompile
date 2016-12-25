using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Tradeable : ITransferable
	{
		private const float MinimumBuyPrice = 0.5f;

		private const float MinimumSellPrice = 0.01f;

		private const float PriceFactorBuy_Global = 1.25f;

		private const float PriceFactorSell_Global = 0.75f;

		public List<Thing> thingsColony = new List<Thing>();

		public List<Thing> thingsTrader = new List<Thing>();

		public int countToDrop;

		public string editBuffer;

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

		public virtual string Label
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

		public bool Interactive
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

		public bool HasAnyThing
		{
			get
			{
				return this.FirstThingColony != null || this.FirstThingTrader != null;
			}
		}

		public Thing AnyThing
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

		public virtual ThingDef ThingDef
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

		public virtual string TipDescription
		{
			get
			{
				return this.ThingDef.description;
			}
		}

		public TradeAction ActionToDo
		{
			get
			{
				if (this.countToDrop == 0)
				{
					return TradeAction.None;
				}
				if (this.countToDrop > 0)
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

		public int CountToTransfer
		{
			get
			{
				return this.countToDrop;
			}
			set
			{
				this.countToDrop = value;
			}
		}

		public string EditBuffer
		{
			get
			{
				return this.editBuffer;
			}
			set
			{
				this.editBuffer = value;
			}
		}

		public TransferablePositiveCountDirection PositiveCountDirection
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
				return (float)this.countToDrop * this.GetPriceFor(this.ActionToDo);
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
			this.pricePlayerBuy = this.BaseMarketValue * 1.25f * this.priceFactorBuy_TraderPriceType * (1f + Find.Storyteller.difficulty.tradePriceFactorLoss);
			this.pricePlayerBuy *= 1f - this.priceGain_PlayerNegotiator - this.priceGain_FactionBase;
			this.pricePlayerBuy = Mathf.Max(this.pricePlayerBuy, 0.5f);
			if (this.pricePlayerBuy > 99.5f)
			{
				this.pricePlayerBuy = Mathf.Round(this.pricePlayerBuy);
			}
			this.priceFactorSell_ItemSellPriceFactor = this.AnyThing.GetStatValue(StatDefOf.SellPriceFactor, true);
			this.pricePlayerSell = this.BaseMarketValue * 0.75f * this.priceFactorSell_TraderPriceType * this.priceFactorSell_ItemSellPriceFactor * (1f - Find.Storyteller.difficulty.tradePriceFactorLoss);
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
					1.25f.ToString("F2"),
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
					0.75f.ToString("F2"),
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
						": -",
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

		public AcceptanceReport CanSetToTransferOneMoreToSource()
		{
			if (this.Bugged)
			{
				return false;
			}
			if (this.CountPostDealFor(Transactor.Trader) <= 0)
			{
				return new AcceptanceReport("TraderHasNoMore".Translate());
			}
			return true;
		}

		public AcceptanceReport TrySetToTransferOneMoreToSource()
		{
			if (this.Bugged)
			{
				return false;
			}
			if (this.IsCurrency)
			{
				Log.Error("Should not increment currency tradeable " + this);
				return false;
			}
			AcceptanceReport result = this.CanSetToTransferOneMoreToSource();
			if (!result.Accepted)
			{
				return result;
			}
			this.countToDrop++;
			return true;
		}

		public void SetToTransferMaxToSource()
		{
			this.countToDrop = this.CountHeldBy(Transactor.Trader);
		}

		public AcceptanceReport CanSetToTransferOneMoreToDest()
		{
			if (this.Bugged)
			{
				return false;
			}
			if (this.CountPostDealFor(Transactor.Colony) <= 0)
			{
				return new AcceptanceReport("ColonyHasNoMore".Translate());
			}
			return true;
		}

		public AcceptanceReport TrySetToTransferOneMoreToDest()
		{
			if (this.Bugged)
			{
				return false;
			}
			if (this.IsCurrency)
			{
				Log.Error("Should not decrement currency tradeable " + this);
				return false;
			}
			AcceptanceReport result = this.CanSetToTransferOneMoreToDest();
			if (!result.Accepted)
			{
				return result;
			}
			this.countToDrop--;
			return true;
		}

		public void SetToTransferMaxToDest()
		{
			this.countToDrop = -this.CountHeldBy(Transactor.Colony);
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
				return this.CountHeldBy(trans) + this.countToDrop;
			}
			return this.CountHeldBy(trans) - this.countToDrop;
		}

		public virtual void ResolveTrade()
		{
			if (this.ActionToDo == TradeAction.PlayerSells)
			{
				TransferableUtility.Transfer(this.thingsColony, -this.countToDrop, delegate(Thing splitPiece, Thing originalThing)
				{
					splitPiece.PreTraded(TradeAction.PlayerSells, TradeSession.playerNegotiator, TradeSession.trader);
					TradeSession.trader.AddToStock(splitPiece, TradeSession.playerNegotiator);
				});
			}
			else if (this.ActionToDo == TradeAction.PlayerBuys)
			{
				TransferableUtility.Transfer(this.thingsTrader, this.countToDrop, delegate(Thing splitPiece, Thing originalThing)
				{
					splitPiece.PreTraded(TradeAction.PlayerBuys, TradeSession.playerNegotiator, TradeSession.trader);
					TradeSession.trader.GiveSoldThingToPlayer(splitPiece, originalThing, TradeSession.playerNegotiator);
					this.CheckTeachOpportunity(splitPiece);
				});
			}
		}

		private void CheckTeachOpportunity(Thing boughtThing)
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
				this.countToDrop,
				")"
			});
		}

		public override int GetHashCode()
		{
			return this.AnyThing.GetHashCode();
		}
	}
}
