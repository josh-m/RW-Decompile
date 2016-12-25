using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class Pawn_TraderTracker : IExposable
	{
		private Pawn pawn;

		public TraderKindDef traderKind;

		private List<Pawn> soldPrisoners = new List<Pawn>();

		public IEnumerable<Thing> Goods
		{
			get
			{
				Lord lord = this.pawn.GetLord();
				if (lord == null || !(lord.LordJob is LordJob_TradeWithColony))
				{
					for (int i = 0; i < this.pawn.inventory.innerContainer.Count; i++)
					{
						Thing t = this.pawn.inventory.innerContainer[i];
						if (!this.pawn.inventory.NotForSale(t))
						{
							yield return t;
						}
					}
				}
				if (lord != null)
				{
					for (int j = 0; j < lord.ownedPawns.Count; j++)
					{
						Pawn p = lord.ownedPawns[j];
						TraderCaravanRole role = p.GetTraderCaravanRole();
						if (role == TraderCaravanRole.Carrier)
						{
							for (int k = 0; k < p.inventory.innerContainer.Count; k++)
							{
								yield return p.inventory.innerContainer[k];
							}
						}
						else if (role == TraderCaravanRole.Chattel)
						{
							yield return p;
						}
					}
				}
			}
		}

		public int RandomPriceFactorSeed
		{
			get
			{
				return Gen.HashCombineInt(this.pawn.thingIDNumber, 1149275593);
			}
		}

		public string TraderName
		{
			get
			{
				return this.pawn.LabelShort;
			}
		}

		public bool CanTradeNow
		{
			get
			{
				return !this.pawn.Dead && this.pawn.Spawned && this.pawn.mindState.wantsToTradeWithColony && this.pawn.CanCasuallyInteractNow(false) && !this.pawn.Downed && !this.pawn.IsPrisoner && this.pawn.Faction != Faction.OfPlayer && (this.pawn.Faction == null || !this.pawn.Faction.HostileTo(Faction.OfPlayer)) && this.Goods.Any((Thing x) => this.traderKind.WillTrade(x.def));
			}
		}

		public Pawn_TraderTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.soldPrisoners.RemoveAll((Pawn x) => x.Destroyed);
			}
			Scribe_Defs.LookDef<TraderKindDef>(ref this.traderKind, "traderKind");
			Scribe_Collections.LookList<Pawn>(ref this.soldPrisoners, "soldPrisoners", LookMode.Reference, new object[0]);
		}

		[DebuggerHidden]
		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			IEnumerable<Thing> items = from x in this.pawn.Map.listerThings.AllThings
			where TradeUtility.EverTradeable(x.def) && x.def.category == ThingCategory.Item && !x.Position.Fogged(x.Map) && (this.<>f__this.pawn.Map.areaManager.Home[x.Position] || x.IsInAnyStorage()) && TradeUtility.TradeableNow(x) && this.<>f__this.ReachableForTrade(x)
			select x;
			foreach (Thing t in items)
			{
				yield return t;
			}
			bool hasLord = this.pawn.GetLord() != null;
			if (hasLord)
			{
				foreach (Pawn p in from x in TradeUtility.AllSellableColonyPawns(this.pawn.Map)
				where !x.Downed && this.<>f__this.ReachableForTrade(x)
				select x)
				{
					yield return p;
				}
			}
		}

		public void AddToStock(Thing thing, Pawn playerNegotiator)
		{
			if (this.Goods.Contains(thing))
			{
				Log.Error("Tried to add " + thing + " to stock (pawn's trader tracker), but it's already here.");
				return;
			}
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				this.AddPawnToStock(pawn);
			}
			else
			{
				Thing thing2 = TradeUtility.ThingFromStockMatching(this.pawn, thing);
				if (thing2 != null)
				{
					thing2.stackCount += thing.stackCount;
					thing.Destroy(DestroyMode.Vanish);
				}
				else
				{
					this.AddThingToRandomInventory(thing);
				}
			}
		}

		public void GiveSoldThingToPlayer(Thing toGive, Thing originalThingFromStock, Pawn playerNegotiator)
		{
			Pawn pawn = toGive as Pawn;
			if (pawn != null)
			{
				if (this.soldPrisoners.Contains(pawn))
				{
					this.soldPrisoners.Remove(pawn);
					TradeUtility.MakePrisonerOfColony(pawn);
				}
				return;
			}
			Pawn pawn2 = null;
			Lord lord = this.pawn.GetLord();
			if (lord != null)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Pawn pawn3 = lord.ownedPawns[i];
					if (pawn3.GetTraderCaravanRole() == TraderCaravanRole.Carrier && pawn3.inventory.Contains(originalThingFromStock))
					{
						pawn2 = pawn3;
						break;
					}
				}
			}
			if (pawn2 == null && this.pawn.inventory.Contains(originalThingFromStock))
			{
				pawn2 = this.pawn;
			}
			if (pawn2 == null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to give ",
					originalThingFromStock,
					" to the player from trader ",
					this.pawn.LabelShort,
					" (pawn's trader tracker), but no carrier has this thing and it's not in trader's inventory."
				}));
				return;
			}
			if (toGive == originalThingFromStock)
			{
				pawn2.inventory.innerContainer.Remove(originalThingFromStock);
			}
			if (GenPlace.TryPlaceThing(toGive, pawn2.Position, this.pawn.Map, ThingPlaceMode.Near, null))
			{
				if (lord != null)
				{
					lord.extraForbiddenThings.Add(toGive);
				}
			}
			else
			{
				Log.Error(string.Concat(new object[]
				{
					"Could not place bought thing ",
					toGive,
					" at ",
					pawn2.Position
				}));
				toGive.Destroy(DestroyMode.Vanish);
			}
		}

		private void AddPawnToStock(Pawn newPawn)
		{
			if (!newPawn.Spawned)
			{
				GenSpawn.Spawn(newPawn, this.pawn.Position, this.pawn.Map);
			}
			if (newPawn.Faction != this.pawn.Faction)
			{
				newPawn.SetFaction(this.pawn.Faction, null);
			}
			if (newPawn.RaceProps.Humanlike)
			{
				newPawn.kindDef = PawnKindDefOf.Slave;
			}
			Lord lord = this.pawn.GetLord();
			if (lord == null)
			{
				newPawn.Destroy(DestroyMode.Vanish);
				Log.Error(string.Concat(new object[]
				{
					"Tried to sell pawn ",
					newPawn,
					" to ",
					this.pawn,
					", but ",
					this.pawn,
					" has no lord. Traders without lord can't buy pawns."
				}));
				return;
			}
			if (newPawn.RaceProps.Humanlike)
			{
				this.soldPrisoners.Add(newPawn);
			}
			lord.AddPawn(newPawn);
		}

		private void AddThingToRandomInventory(Thing thing)
		{
			Lord lord = this.pawn.GetLord();
			IEnumerable<Pawn> source = Enumerable.Empty<Pawn>();
			if (lord != null)
			{
				source = from x in lord.ownedPawns
				where x.GetTraderCaravanRole() == TraderCaravanRole.Carrier
				select x;
			}
			if (source.Any<Pawn>())
			{
				if (!source.RandomElement<Pawn>().inventory.innerContainer.TryAdd(thing, true))
				{
					thing.Destroy(DestroyMode.Vanish);
				}
			}
			else if (!this.pawn.inventory.innerContainer.TryAdd(thing, true))
			{
				thing.Destroy(DestroyMode.Vanish);
			}
		}

		private bool ReachableForTrade(Thing thing)
		{
			return this.pawn.Map == thing.Map && this.pawn.Map.reachability.CanReach(this.pawn.Position, thing, PathEndMode.Touch, TraverseMode.PassDoors, Danger.Some);
		}
	}
}
