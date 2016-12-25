using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Tradeable
	{
		public List<Thing> thingsColony = new List<Thing>();

		public List<Thing> thingsTrader = new List<Thing>();

		public int countToDrop;

		public string editBuffer;

		private static readonly SimpleCurve LaunchPricePostFactorCurve = new SimpleCurve
		{
			new CurvePoint(2000f, 1f),
			new CurvePoint(12000f, 0.5f),
			new CurvePoint(200000f, 0.2f)
		};

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

		public virtual float ListOrderPriority
		{
			get
			{
				int num;
				if (this.IsCurrency)
				{
					num = 100;
				}
				else if (this.ThingDef == ThingDefOf.Gold)
				{
					num = 99;
				}
				else if (this.ThingDef.Minifiable)
				{
					num = 90;
				}
				else if (this.ThingDef.IsApparel)
				{
					num = 80;
				}
				else if (this.ThingDef.IsRangedWeapon)
				{
					num = 70;
				}
				else if (this.ThingDef.IsMeleeWeapon)
				{
					num = 60;
				}
				else if (this.ThingDef.isBodyPartOrImplant)
				{
					num = 50;
				}
				else if (this.ThingDef.CountAsResource)
				{
					num = -10;
				}
				else
				{
					num = 20;
				}
				return (float)num;
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

		public float CurTotalSilverCost
		{
			get
			{
				if (this.ActionToDo == TradeAction.None)
				{
					return 0f;
				}
				return (float)this.countToDrop * this.PriceFor(this.ActionToDo);
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
				if (this.AnyThing == null)
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

		public AcceptanceReport CanSetToDropOneMore()
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

		public AcceptanceReport TrySetToDropOneMore()
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
			AcceptanceReport result = this.CanSetToDropOneMore();
			if (!result.Accepted)
			{
				return result;
			}
			this.countToDrop++;
			return true;
		}

		public void SetToDropMax()
		{
			this.countToDrop = this.CountHeldBy(Transactor.Trader);
		}

		public AcceptanceReport CanSetToLaunchOneMore()
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

		public AcceptanceReport TrySetToLaunchOneMore()
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
			AcceptanceReport result = this.CanSetToLaunchOneMore();
			if (!result.Accepted)
			{
				return result;
			}
			this.countToDrop--;
			return true;
		}

		public void SetToLaunchMax()
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

		public float PriceFor(TradeAction action)
		{
			float num = TradeSession.trader.TraderKind.PriceTypeFor(this.ThingDef, action).PriceMultiplier();
			float num2 = TradeUtility.RandomPriceFactorFor(TradeSession.trader, this);
			float num3;
			if (action == TradeAction.PlayerBuys)
			{
				num3 = this.BaseMarketValue * (1f - TradeSession.playerNegotiator.GetStatValue(StatDefOf.TradePriceImprovement, true)) * num * num2;
				num3 = Mathf.Max(num3, 0.5f);
			}
			else
			{
				num3 = this.BaseMarketValue * Find.Storyteller.difficulty.baseSellPriceFactor * this.AnyThing.GetStatValue(StatDefOf.SellPriceFactor, true) * (1f + TradeSession.playerNegotiator.GetStatValue(StatDefOf.TradePriceImprovement, true)) * num * num2;
				num3 *= Tradeable.LaunchPricePostFactorCurve.Evaluate(num3);
				num3 = Mathf.Max(num3, 0.01f);
				if (num3 >= this.PriceFor(TradeAction.PlayerBuys))
				{
					Log.ErrorOnce("Skill of negotitator trying to put sell price above buy price.", 65387);
					num3 = this.PriceFor(TradeAction.PlayerBuys);
				}
			}
			if (num3 > 99.5f)
			{
				num3 = Mathf.Round(num3);
			}
			return num3;
		}

		public virtual void ResolveTrade()
		{
			if (this.ActionToDo == TradeAction.PlayerSells)
			{
				int i = -this.countToDrop;
				while (i > 0)
				{
					if (this.thingsColony.Count == 0)
					{
						Log.Error("Nothing left to give to trader for " + this);
						return;
					}
					Thing thing = this.thingsColony[0];
					int num = Mathf.Min(i, thing.stackCount);
					Thing thing2 = thing.SplitOff(num);
					i -= num;
					if (thing2 == thing)
					{
						this.thingsColony.Remove(thing);
					}
					thing2.PreTraded(TradeAction.PlayerSells, TradeSession.playerNegotiator, TradeSession.trader);
					TradeSession.trader.AddToStock(thing2);
				}
			}
			else if (this.ActionToDo == TradeAction.PlayerBuys)
			{
				int j = this.countToDrop;
				while (j > 0)
				{
					if (this.thingsTrader.Count == 0)
					{
						Log.Error("Nothing left to take from trader for " + this);
						return;
					}
					Thing thing3 = this.thingsTrader[0];
					int num2 = Mathf.Min(j, thing3.stackCount);
					Thing thing4 = thing3.SplitOff(num2);
					j -= num2;
					if (thing4 == thing3)
					{
						this.thingsTrader.Remove(thing3);
					}
					thing4.PreTraded(TradeAction.PlayerBuys, TradeSession.playerNegotiator, TradeSession.trader);
					TradeSession.trader.GiveSoldThingToBuyer(thing4, thing3);
					this.CheckTeachOpportunity(thing4);
				}
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
