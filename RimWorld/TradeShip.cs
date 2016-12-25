using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class TradeShip : PassingShip, ITrader
	{
		public TraderKindDef def;

		private List<Thing> things = new List<Thing>();

		private List<Pawn> soldPrisoners = new List<Pawn>();

		private int randomPriceFactorSeed = -1;

		private static List<string> tmpExtantNames = new List<string>();

		public override string FullTitle
		{
			get
			{
				return this.name + " (" + this.def.label + ")";
			}
		}

		public int Silver
		{
			get
			{
				return this.CountHeldOf(ThingDefOf.Silver, null);
			}
		}

		public TraderKindDef TraderKind
		{
			get
			{
				return this.def;
			}
		}

		public IEnumerable<Thing> Goods
		{
			get
			{
				return this.things.Concat(this.soldPrisoners.Cast<Thing>());
			}
		}

		public int RandomPriceFactorSeed
		{
			get
			{
				return this.randomPriceFactorSeed;
			}
		}

		public string TraderName
		{
			get
			{
				return this.name;
			}
		}

		public bool CanTradeNow
		{
			get
			{
				return !base.Departed;
			}
		}

		public float TradePriceImprovementOffsetForPlayer
		{
			get
			{
				return 0f;
			}
		}

		public TradeShip()
		{
		}

		public TradeShip(TraderKindDef def)
		{
			this.def = def;
			TradeShip.tmpExtantNames.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				TradeShip.tmpExtantNames.AddRange(from x in maps[i].passingShipManager.passingShips
				select x.name);
			}
			this.name = NameGenerator.GenerateName(RulePackDefOf.NamerTraderGeneral, TradeShip.tmpExtantNames, false);
			this.randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
			this.loadID = Find.World.uniqueIDsManager.GetNextPassingShipID();
		}

		[DebuggerHidden]
		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			foreach (Thing t in TradeUtility.AllLaunchableThings(base.Map))
			{
				yield return t;
			}
			foreach (Pawn p in TradeUtility.AllSellableColonyPawns(base.Map))
			{
				yield return p;
			}
		}

		public void GenerateThings()
		{
			this.things = TraderStockGenerator.GenerateTraderThings(this.def, base.Map).ToList<Thing>();
		}

		public override void PassingShipTick()
		{
			base.PassingShipTick();
			for (int i = this.things.Count - 1; i >= 0; i--)
			{
				Pawn pawn = this.things[i] as Pawn;
				if (pawn != null)
				{
					pawn.Tick();
					if (pawn.Dead)
					{
						this.things.RemoveAt(i);
					}
				}
			}
			for (int j = this.soldPrisoners.Count - 1; j >= 0; j--)
			{
				this.soldPrisoners[j].Tick();
				if (this.soldPrisoners[j].Dead)
				{
					this.soldPrisoners.RemoveAt(j);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<TraderKindDef>(ref this.def, "def");
			Scribe_Collections.LookList<Thing>(ref this.things, "things", LookMode.Deep, new object[0]);
			Scribe_Collections.LookList<Pawn>(ref this.soldPrisoners, "soldPrisoners", LookMode.Deep, new object[0]);
			Scribe_Values.LookValue<int>(ref this.randomPriceFactorSeed, "randomPriceFactorSeed", 0, false);
		}

		public override void TryOpenComms(Pawn negotiator)
		{
			if (!this.CanTradeNow)
			{
				return;
			}
			Find.WindowStack.Add(new Dialog_Trade(negotiator, this));
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
			string empty = string.Empty;
			string empty2 = string.Empty;
			PawnRelationUtility.Notify_PawnsSeenByPlayer(this.Goods.OfType<Pawn>(), ref empty, ref empty2, "LetterRelatedPawnsTradeShip".Translate(), false);
			if (!empty2.NullOrEmpty())
			{
				Find.LetterStack.ReceiveLetter(empty, empty2, LetterType.Good, null);
			}
			TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon);
		}

		public override void Depart()
		{
			base.Depart();
			foreach (Thing current in this.Goods)
			{
				current.DestroyOrPassToWorld(DestroyMode.Vanish);
			}
			this.things.Clear();
			this.soldPrisoners.Clear();
		}

		public override string GetCallLabel()
		{
			return this.name + " (" + this.def.label + ")";
		}

		public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
		{
			Thing thing = this.HeldThingMatching(thingDef, stuffDef);
			if (thing != null)
			{
				return thing.stackCount;
			}
			return 0;
		}

		public void AddToStock(Thing thing, Pawn playerNegotiator)
		{
			Thing thing2 = TradeUtility.ThingFromStockMatching(this, thing);
			if (thing2 != null)
			{
				thing2.stackCount += thing.stackCount;
				thing.Destroy(DestroyMode.Vanish);
			}
			else
			{
				if (thing.Spawned)
				{
					thing.DeSpawn();
				}
				Pawn pawn = thing as Pawn;
				if (pawn != null && pawn.RaceProps.Humanlike)
				{
					this.soldPrisoners.Add(pawn);
				}
				else
				{
					this.things.Add(thing);
				}
			}
		}

		public void GiveSoldThingToPlayer(Thing toGive, Thing originalThingFromStock, Pawn playerNegotiator)
		{
			if (toGive == originalThingFromStock)
			{
				if (!this.things.Contains(originalThingFromStock))
				{
					Log.Error(string.Concat(new object[]
					{
						"Tried to remove ",
						originalThingFromStock,
						" from trader ",
						this.name,
						" who didn't have it."
					}));
					return;
				}
				this.things.Remove(originalThingFromStock);
			}
			Pawn pawn = toGive as Pawn;
			if (pawn != null && this.soldPrisoners.Contains(pawn))
			{
				this.soldPrisoners.Remove(pawn);
				TradeUtility.MakePrisonerOfColony(pawn);
			}
			TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(base.Map), base.Map, toGive);
		}

		private Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
		{
			for (int i = 0; i < this.things.Count; i++)
			{
				if (this.things[i].def == thingDef && this.things[i].Stuff == stuffDef)
				{
					return this.things[i];
				}
			}
			return null;
		}

		public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
		{
			Thing thing = this.HeldThingMatching(thingDef, stuffDef);
			if (thing == null)
			{
				Log.Error("Changing count of thing trader doesn't have: " + thingDef);
			}
			thing.stackCount += count;
		}

		public override string ToString()
		{
			return this.FullTitle;
		}
	}
}
