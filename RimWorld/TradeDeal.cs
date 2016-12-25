using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class TradeDeal
	{
		private List<Tradeable> tradeables = new List<Tradeable>();

		public List<string> cannotSellReasons = new List<string>();

		public int TradeableCount
		{
			get
			{
				return this.tradeables.Count;
			}
		}

		public Tradeable SilverTradeable
		{
			get
			{
				for (int i = 0; i < this.tradeables.Count; i++)
				{
					if (this.tradeables[i].ThingDef == ThingDefOf.Silver)
					{
						return this.tradeables[i];
					}
				}
				return null;
			}
		}

		public IEnumerable<Tradeable> AllTradeables
		{
			get
			{
				return this.tradeables;
			}
		}

		public TradeDeal()
		{
			this.Reset();
		}

		public IEnumerator<Tradeable> GetEnumerator()
		{
			return this.tradeables.GetEnumerator();
		}

		public void Reset()
		{
			this.tradeables.Clear();
			this.cannotSellReasons.Clear();
			this.AddAllTradeables();
		}

		private void AddAllTradeables()
		{
			foreach (Thing current in TradeSession.trader.ColonyThingsWillingToBuy)
			{
				string item;
				if (!this.InSellablePosition(current, out item))
				{
					if (!this.cannotSellReasons.Contains(item))
					{
						this.cannotSellReasons.Add(item);
					}
				}
				else
				{
					this.AddToTradeables(current, Transactor.Colony);
				}
			}
			foreach (Thing current2 in TradeSession.trader.Goods)
			{
				this.AddToTradeables(current2, Transactor.Trader);
			}
			if (this.tradeables.Find((Tradeable x) => x.IsCurrency) == null)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver, null);
				thing.stackCount = 0;
				this.AddToTradeables(thing, Transactor.Trader);
			}
		}

		private bool InSellablePosition(Thing t, out string reason)
		{
			if (t.Position.Fogged())
			{
				reason = null;
				return false;
			}
			Room room = t.Position.GetRoom();
			int num = GenRadial.NumCellsInRadius(6.9f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds() && intVec.GetRoom() == room)
				{
					List<Thing> thingList = intVec.GetThingList();
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j].PreventPlayerSellingThingsNearby(out reason))
						{
							return false;
						}
					}
				}
			}
			reason = null;
			return true;
		}

		private void AddToTradeables(Thing t, Transactor trans)
		{
			Tradeable tradeable = this.TradeableMatching(t);
			if (tradeable == null)
			{
				Pawn pawn = t as Pawn;
				if (pawn != null)
				{
					tradeable = new Tradeable_Pawn();
				}
				else
				{
					tradeable = new Tradeable();
				}
				this.tradeables.Add(tradeable);
			}
			tradeable.AddThing(t, trans);
		}

		private Tradeable TradeableMatching(Thing thing)
		{
			foreach (Tradeable current in this.tradeables)
			{
				if (TradeUtility.TradeAsOne(thing, current.AnyThing))
				{
					return current;
				}
			}
			return null;
		}

		public void UpdateCurrencyCount()
		{
			float num = 0f;
			foreach (Tradeable current in this.tradeables)
			{
				if (!current.IsCurrency)
				{
					num += current.CurTotalSilverCost;
				}
			}
			this.SilverTradeable.countToDrop = -Mathf.RoundToInt(num);
		}

		public bool TryExecute(out bool actuallyTraded)
		{
			if (this.SilverTradeable.CountPostDealFor(Transactor.Colony) < 0)
			{
				Find.WindowStack.WindowOfType<Dialog_Trade>().FlashSilver();
				Messages.Message("MessageColonyCannotAfford".Translate(), MessageSound.RejectInput);
				actuallyTraded = false;
				return false;
			}
			this.UpdateCurrencyCount();
			this.LimitCurrencyCountToTraderFunds();
			actuallyTraded = false;
			foreach (Tradeable current in this.tradeables)
			{
				if (current.ActionToDo != TradeAction.None)
				{
					actuallyTraded = true;
				}
				current.ResolveTrade();
			}
			this.Reset();
			return true;
		}

		public bool DoesTraderHaveEnoughSilver()
		{
			return this.SilverTradeable.CountPostDealFor(Transactor.Trader) >= 0;
		}

		private void LimitCurrencyCountToTraderFunds()
		{
			if (this.SilverTradeable.countToDrop > this.SilverTradeable.CountHeldBy(Transactor.Trader))
			{
				this.SilverTradeable.countToDrop = this.SilverTradeable.CountHeldBy(Transactor.Trader);
			}
		}
	}
}
