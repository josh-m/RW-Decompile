using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class FactionBase : MapParent, ITrader
	{
		private string nameInt;

		public FactionBase_TraderTracker trader;

		public bool namedByPlayer;

		private Material cachedMat;

		public string Name
		{
			get
			{
				return this.nameInt;
			}
			set
			{
				this.nameInt = value;
			}
		}

		public override Material Material
		{
			get
			{
				if (this.cachedMat == null)
				{
					this.cachedMat = MaterialPool.MatFrom(base.Faction.def.homeIconPath, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color);
				}
				return this.cachedMat;
			}
		}

		public override Texture2D ExpandingIcon
		{
			get
			{
				return base.Faction.def.ExpandingIconTexture;
			}
		}

		public override string Label
		{
			get
			{
				return (this.nameInt == null) ? base.Label : this.nameInt;
			}
		}

		protected override bool UseGenericEnterMapFloatMenuOption
		{
			get
			{
				return base.Faction.IsPlayer;
			}
		}

		public TraderKindDef TraderKind
		{
			get
			{
				return this.trader.TraderKind;
			}
		}

		public IEnumerable<Thing> Goods
		{
			get
			{
				return this.trader.Stock;
			}
		}

		public int RandomPriceFactorSeed
		{
			get
			{
				return this.trader.RandomPriceFactorSeed;
			}
		}

		public string TraderName
		{
			get
			{
				return this.trader.TraderName;
			}
		}

		public bool CanTradeNow
		{
			get
			{
				return this.trader.CanTradeNow;
			}
		}

		public float TradePriceImprovementOffsetForPlayer
		{
			get
			{
				return this.trader.TradePriceImprovementOffsetForPlayer;
			}
		}

		public FactionBase()
		{
			this.trader = new FactionBase_TraderTracker(this);
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			return this.trader.ColonyThingsWillingToBuy(playerNegotiator);
		}

		public void AddToStock(Thing thing, Pawn playerNegotiator)
		{
			this.trader.AddToStock(thing, playerNegotiator);
		}

		public void GiveSoldThingToPlayer(Thing toGive, Thing originalThingFromStock, Pawn playerNegotiator)
		{
			this.trader.GiveSoldThingToPlayer(toGive, originalThingFromStock, playerNegotiator);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<string>(ref this.nameInt, "nameInt", null, false);
			Scribe_Deep.LookDeep<FactionBase_TraderTracker>(ref this.trader, "trader", new object[]
			{
				this
			});
			Scribe_Values.LookValue<bool>(ref this.namedByPlayer, "namedByPlayer", false, false);
		}

		public override void Tick()
		{
			base.Tick();
			this.trader.TraderTrackerTick();
			FactionBaseDefeatUtility.CheckDefeated(this);
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = false;
			return !base.Map.IsPlayerHome && !base.Map.mapPawns.AnyColonistTameAnimalOrPrisonerOfColony;
		}

		public override void PostRemove()
		{
			base.PostRemove();
			this.trader.DestroyStock();
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				yield return FactionBaseAbandonUtility.AbandonCommand(this);
			}
			else if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.FormCaravan))
			{
				Command_Action formCaravan = new Command_Action();
				formCaravan.defaultLabel = "CommandFormCaravan".Translate();
				formCaravan.defaultDesc = "CommandFormCaravanDesc".Translate();
				formCaravan.icon = MapParent.FormCaravanCommand;
				formCaravan.action = delegate
				{
					Find.Tutor.learningReadout.TryActivateConcept(ConceptDefOf.FormCaravan);
					Messages.Message("MessageSelectOwnBaseToFormCaravan".Translate(), MessageSound.RejectInput);
				};
				yield return formCaravan;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan))
			{
				yield return o;
			}
			if (!base.Faction.IsPlayer)
			{
				if (!base.Faction.HostileTo(Faction.OfPlayer) && CaravanVisitUtility.FactionBaseVisitedNow(caravan) != this)
				{
					yield return new FloatMenuOption("VisitFactionBase".Translate(new object[]
					{
						this.Label
					}), delegate
					{
						this.caravan.pather.StartPath(this.<>f__this.Tile, new CaravanArrivalAction_Visit(this.<>f__this), true);
					}, MenuOptionPriority.Default, null, null, 0f, null, this);
					if (Prefs.DevMode)
					{
						yield return new FloatMenuOption("VisitFactionBase".Translate(new object[]
						{
							this.Label
						}) + " (Dev: instantly)", delegate
						{
							this.caravan.Tile = this.<>f__this.Tile;
							new CaravanArrivalAction_Visit(this.<>f__this).Arrived(this.caravan);
						}, MenuOptionPriority.Default, null, null, 0f, null, this);
					}
				}
				yield return new FloatMenuOption("AttackFactionBase".Translate(new object[]
				{
					this.Label
				}), delegate
				{
					this.caravan.pather.StartPath(this.<>f__this.Tile, new CaravanArrivalAction_Attack(this.<>f__this), true);
				}, MenuOptionPriority.Default, null, null, 0f, null, this);
				if (Prefs.DevMode)
				{
					yield return new FloatMenuOption("AttackFactionBase".Translate(new object[]
					{
						this.Label
					}) + " (Dev: instantly)", delegate
					{
						this.caravan.Tile = this.<>f__this.Tile;
						new CaravanArrivalAction_Attack(this.<>f__this).Arrived(this.caravan);
					}, MenuOptionPriority.Default, null, null, 0f, null, this);
				}
			}
		}
	}
}
