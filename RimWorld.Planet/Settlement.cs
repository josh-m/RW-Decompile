using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class Settlement : MapParent, ITrader
	{
		public Settlement_TraderTracker trader;

		public List<Pawn> previouslyGeneratedInhabitants = new List<Pawn>();

		public static readonly Texture2D FormCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan", true);

		protected override bool UseGenericEnterMapFloatMenuOption
		{
			get
			{
				return !this.Attackable;
			}
		}

		public virtual bool Visitable
		{
			get
			{
				return base.Faction != Faction.OfPlayer && (base.Faction == null || !base.Faction.HostileTo(Faction.OfPlayer));
			}
		}

		public virtual bool Attackable
		{
			get
			{
				return base.Faction != Faction.OfPlayer;
			}
		}

		public override bool TransportPodsCanLandAndGenerateMap
		{
			get
			{
				return this.Attackable;
			}
		}

		public TraderKindDef TraderKind
		{
			get
			{
				if (this.trader == null)
				{
					return null;
				}
				return this.trader.TraderKind;
			}
		}

		public IEnumerable<Thing> Goods
		{
			get
			{
				if (this.trader == null)
				{
					return null;
				}
				return this.trader.StockListForReading;
			}
		}

		public int RandomPriceFactorSeed
		{
			get
			{
				if (this.trader == null)
				{
					return 0;
				}
				return this.trader.RandomPriceFactorSeed;
			}
		}

		public string TraderName
		{
			get
			{
				if (this.trader == null)
				{
					return null;
				}
				return this.trader.TraderName;
			}
		}

		public bool CanTradeNow
		{
			get
			{
				return this.trader != null && this.trader.CanTradeNow;
			}
		}

		public float TradePriceImprovementOffsetForPlayer
		{
			get
			{
				if (this.trader == null)
				{
					return 0f;
				}
				return this.trader.TradePriceImprovementOffsetForPlayer;
			}
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			if (this.trader == null)
			{
				return null;
			}
			return this.trader.ColonyThingsWillingToBuy(playerNegotiator);
		}

		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			this.trader.GiveSoldThingToTrader(toGive, countToGive, playerNegotiator);
		}

		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			this.trader.GiveSoldThingToPlayer(toGive, countToGive, playerNegotiator);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look<Pawn>(ref this.previouslyGeneratedInhabitants, "previouslyGeneratedInhabitants", LookMode.Reference, new object[0]);
			Scribe_Deep.Look<Settlement_TraderTracker>(ref this.trader, "trader", new object[]
			{
				this
			});
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.SettlementPostLoadInit(this);
				this.previouslyGeneratedInhabitants.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (this.trader != null)
			{
				this.trader.TraderTrackerTick();
			}
		}

		public override void Notify_MyMapRemoved(Map map)
		{
			base.Notify_MyMapRemoved(map);
			for (int i = this.previouslyGeneratedInhabitants.Count - 1; i >= 0; i--)
			{
				Pawn pawn = this.previouslyGeneratedInhabitants[i];
				if (pawn.DestroyedOrNull() || !pawn.IsWorldPawn())
				{
					this.previouslyGeneratedInhabitants.RemoveAt(i);
				}
			}
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = false;
			return !base.Map.IsPlayerHome && !base.Map.mapPawns.AnyPawnBlockingMapRemoval;
		}

		public override void PostRemove()
		{
			base.PostRemove();
			if (this.trader != null)
			{
				this.trader.TryDestroyStock();
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (base.Faction != Faction.OfPlayer && Current.ProgramState == ProgramState.Playing && !PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.FormCaravan))
			{
				Command_Action formCaravan = new Command_Action();
				formCaravan.defaultLabel = "CommandFormCaravan".Translate();
				formCaravan.defaultDesc = "CommandFormCaravanDesc".Translate();
				formCaravan.icon = Settlement.FormCaravanCommand;
				formCaravan.action = delegate
				{
					Find.Tutor.learningReadout.TryActivateConcept(ConceptDefOf.FormCaravan);
					Messages.Message("MessageSelectOwnBaseToFormCaravan".Translate(), MessageTypeDefOf.RejectInput);
				};
				yield return formCaravan;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
		{
			if (this.CanTradeNow)
			{
				yield return CaravanVisitUtility.TradeCommand(caravan);
			}
			if (base.GetComponent<CaravanRequestComp>() != null && base.GetComponent<CaravanRequestComp>().ActiveRequest)
			{
				yield return CaravanVisitUtility.FulfillRequestCommand(caravan);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan))
			{
				yield return o;
			}
			if (this.Visitable && CaravanVisitUtility.SettlementVisitedNow(caravan) != this)
			{
				yield return new FloatMenuOption("VisitSettlement".Translate(new object[]
				{
					this.Label
				}), delegate
				{
					caravan.pather.StartPath(this.$this.Tile, new CaravanArrivalAction_VisitSettlement(this.$this), true);
				}, MenuOptionPriority.Default, null, null, 0f, null, this);
				if (Prefs.DevMode)
				{
					yield return new FloatMenuOption("VisitSettlement".Translate(new object[]
					{
						this.Label
					}) + " (Dev: instantly)", delegate
					{
						caravan.Tile = this.$this.Tile;
						caravan.pather.StopDead();
						new CaravanArrivalAction_VisitSettlement(this.$this).Arrived(caravan);
					}, MenuOptionPriority.Default, null, null, 0f, null, this);
				}
			}
			if (this.Attackable)
			{
				yield return new FloatMenuOption("AttackSettlement".Translate(new object[]
				{
					this.Label
				}), delegate
				{
					caravan.pather.StartPath(this.$this.Tile, new CaravanArrivalAction_AttackSettlement(this.$this), true);
				}, MenuOptionPriority.Default, null, null, 0f, null, this);
				if (Prefs.DevMode)
				{
					yield return new FloatMenuOption("AttackSettlement".Translate(new object[]
					{
						this.Label
					}) + " (Dev: instantly)", delegate
					{
						caravan.Tile = this.$this.Tile;
						new CaravanArrivalAction_AttackSettlement(this.$this).Arrived(caravan);
					}, MenuOptionPriority.Default, null, null, 0f, null, this);
				}
			}
		}

		public override void GetChildHolders(List<IThingHolder> outChildren)
		{
			base.GetChildHolders(outChildren);
			if (this.trader != null)
			{
				outChildren.Add(this.trader);
			}
		}
	}
}
