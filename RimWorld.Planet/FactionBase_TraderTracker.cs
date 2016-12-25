using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class FactionBase_TraderTracker : IExposable
	{
		private const int RegenerateStockEveryDays = 10;

		private const float TradePriceImprovement = 0.04f;

		private FactionBase factionBase;

		private List<Thing> stock;

		private int lastStockGenerationTicks = -1;

		private List<Pawn> tmpSavedPawns = new List<Pawn>();

		public List<Thing> Stock
		{
			get
			{
				if (this.stock == null || Find.TickManager.TicksGame - this.lastStockGenerationTicks > 600000)
				{
					this.RegenerateStock();
				}
				return this.stock;
			}
		}

		public TraderKindDef TraderKind
		{
			get
			{
				List<TraderKindDef> factionBaseTraderKinds = this.factionBase.Faction.def.factionBaseTraderKinds;
				if (factionBaseTraderKinds.NullOrEmpty<TraderKindDef>())
				{
					return null;
				}
				int index = Mathf.Abs(this.factionBase.HashOffset()) % factionBaseTraderKinds.Count;
				return factionBaseTraderKinds[index];
			}
		}

		public int RandomPriceFactorSeed
		{
			get
			{
				return Gen.HashCombineInt(this.factionBase.ID, 1933327354);
			}
		}

		public string TraderName
		{
			get
			{
				return "FactionBaseTrader".Translate(new object[]
				{
					this.factionBase.LabelCap,
					this.factionBase.Faction.Name
				});
			}
		}

		public bool CanTradeNow
		{
			get
			{
				return this.TraderKind != null && this.Stock.Any((Thing x) => this.TraderKind.WillTrade(x.def));
			}
		}

		public float TradePriceImprovementOffsetForPlayer
		{
			get
			{
				return 0.04f;
			}
		}

		public FactionBase_TraderTracker(FactionBase factionBase)
		{
			this.factionBase = factionBase;
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.tmpSavedPawns.Clear();
				if (this.stock != null)
				{
					for (int i = this.stock.Count - 1; i >= 0; i--)
					{
						Pawn pawn = this.stock[i] as Pawn;
						if (pawn != null)
						{
							this.stock.RemoveAt(i);
							this.tmpSavedPawns.Add(pawn);
						}
					}
				}
			}
			Scribe_Collections.LookList<Pawn>(ref this.tmpSavedPawns, "tmpSavedPawns", LookMode.Reference, new object[0]);
			Scribe_Collections.LookList<Thing>(ref this.stock, "stock", LookMode.Deep, new object[0]);
			Scribe_Values.LookValue<int>(ref this.lastStockGenerationTicks, "lastStockGenerationTicks", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving)
			{
				for (int j = 0; j < this.tmpSavedPawns.Count; j++)
				{
					this.stock.Add(this.tmpSavedPawns[j]);
				}
				this.tmpSavedPawns.Clear();
			}
		}

		[DebuggerHidden]
		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			Caravan caravan = playerNegotiator.GetCaravan();
			foreach (Thing item in CaravanInventoryUtility.AllInventoryItems(caravan))
			{
				yield return item;
			}
			List<Pawn> pawns = caravan.PawnsListForReading;
			for (int i = 0; i < pawns.Count; i++)
			{
				if (!caravan.IsOwner(pawns[i]))
				{
					yield return pawns[i];
				}
			}
		}

		public void AddToStock(Thing thing, Pawn playerNegotiator)
		{
			Caravan caravan = playerNegotiator.GetCaravan();
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading, null);
				caravan.RemovePawn(pawn);
				if (pawn.RaceProps.Humanlike)
				{
					Find.WorldPawns.DiscardIfUnimportant(pawn);
					return;
				}
			}
			else
			{
				Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, thing);
				if (ownerOf != null)
				{
					ownerOf.inventory.innerContainer.Remove(thing);
				}
			}
			this.Stock.Add(thing);
		}

		public void GiveSoldThingToPlayer(Thing toGive, Thing originalThingFromStock, Pawn playerNegotiator)
		{
			if (toGive == originalThingFromStock)
			{
				this.Stock.Remove(originalThingFromStock);
			}
			Caravan caravan = playerNegotiator.GetCaravan();
			Pawn pawn = toGive as Pawn;
			if (pawn != null)
			{
				caravan.AddPawn(pawn, true);
			}
			else
			{
				Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(toGive, caravan.PawnsListForReading, null, null);
				if (pawn2 != null)
				{
					pawn2.inventory.innerContainer.TryAdd(toGive, true);
				}
				else
				{
					Log.Error("Could not find any pawn to give sold thing to.");
				}
			}
		}

		public void TraderTrackerTick()
		{
			if (this.stock != null)
			{
				for (int i = this.stock.Count - 1; i >= 0; i--)
				{
					Pawn pawn = this.stock[i] as Pawn;
					if (pawn != null && pawn.Destroyed)
					{
						this.stock.RemoveAt(i);
						Find.WorldPawns.DiscardIfUnimportant(pawn);
					}
				}
				for (int j = this.stock.Count - 1; j >= 0; j--)
				{
					Pawn pawn2 = this.stock[j] as Pawn;
					if (pawn2 != null && !pawn2.IsWorldPawn())
					{
						Log.Error("Faction base has non-world-pawns in its stock. Removing...");
						this.stock.RemoveAt(j);
					}
				}
			}
		}

		public void DestroyStock()
		{
			if (this.stock != null)
			{
				for (int i = this.stock.Count - 1; i >= 0; i--)
				{
					Thing thing = this.stock[i];
					this.stock.RemoveAt(i);
					Pawn pawn = thing as Pawn;
					if (pawn != null)
					{
						Find.WorldPawns.DiscardIfUnimportant(pawn);
					}
					else
					{
						thing.Destroy(DestroyMode.Vanish);
					}
				}
				this.stock = null;
			}
		}

		public bool ContainsPawn(Pawn p)
		{
			return this.stock != null && this.stock.Contains(p);
		}

		private void RegenerateStock()
		{
			this.DestroyStock();
			this.stock = new List<Thing>();
			if (!this.factionBase.Faction.IsPlayer)
			{
				this.stock.AddRange(TraderStockGenerator.GenerateTraderThings(this.TraderKind, null));
			}
			for (int i = 0; i < this.stock.Count; i++)
			{
				Pawn pawn = this.stock[i] as Pawn;
				if (pawn != null)
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
				}
			}
			this.lastStockGenerationTicks = Find.TickManager.TicksGame;
		}
	}
}
