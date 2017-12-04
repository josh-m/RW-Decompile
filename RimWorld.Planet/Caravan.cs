using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class Caravan : WorldObject, IThingHolder, IIncidentTarget, ITrader, ILoadReferenceable
	{
		private string nameInt;

		public ThingOwner<Pawn> pawns;

		public bool autoJoinable;

		public Caravan_PathFollower pather;

		public Caravan_GotoMoteRenderer gotoMote;

		public Caravan_Tweener tweener;

		public Caravan_TraderTracker trader;

		public StoryState storyState;

		private Material cachedMat;

		private bool cachedImmobilized;

		private int cachedImmobilizedForTicks = -99999;

		private Pair<float, float> cachedDaysWorthOfFood;

		private int cachedDaysWorthOfFoodForTicks = -99999;

		private const int ImmobilizedCacheDuration = 60;

		private const int DaysWorthOfFoodCacheDuration = 3000;

		private const int TendIntervalTicks = 2000;

		private const int TryTakeScheduledDrugsIntervalTicks = 120;

		private static readonly Texture2D SplitCommand = ContentFinder<Texture2D>.Get("UI/Commands/SplitCaravan", true);

		private static readonly Color PlayerCaravanColor = new Color(1f, 0.863f, 0.33f);

		public List<Pawn> PawnsListForReading
		{
			get
			{
				return this.pawns.InnerListForReading;
			}
		}

		public override Material Material
		{
			get
			{
				if (this.cachedMat == null)
				{
					Color color;
					if (base.Faction == null)
					{
						color = Color.white;
					}
					else if (base.Faction.IsPlayer)
					{
						color = Caravan.PlayerCaravanColor;
					}
					else
					{
						color = base.Faction.Color;
					}
					this.cachedMat = MaterialPool.MatFrom(this.def.texture, ShaderDatabase.WorldOverlayTransparentLit, color, WorldMaterials.DynamicObjectRenderQueue);
				}
				return this.cachedMat;
			}
		}

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

		public override Vector3 DrawPos
		{
			get
			{
				return this.tweener.TweenedPos;
			}
		}

		public bool IsPlayerControlled
		{
			get
			{
				return base.Faction == Faction.OfPlayer;
			}
		}

		public bool ImmobilizedByMass
		{
			get
			{
				if (Find.TickManager.TicksGame - this.cachedImmobilizedForTicks < 60)
				{
					return this.cachedImmobilized;
				}
				this.cachedImmobilized = (this.MassUsage > this.MassCapacity);
				this.cachedImmobilizedForTicks = Find.TickManager.TicksGame;
				return this.cachedImmobilized;
			}
		}

		public Pair<float, float> DaysWorthOfFood
		{
			get
			{
				if (Find.TickManager.TicksGame - this.cachedDaysWorthOfFoodForTicks < 3000)
				{
					return this.cachedDaysWorthOfFood;
				}
				this.cachedDaysWorthOfFood = new Pair<float, float>(DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this), DaysUntilRotCalculator.ApproxDaysUntilRot(this));
				this.cachedDaysWorthOfFoodForTicks = Find.TickManager.TicksGame;
				return this.cachedDaysWorthOfFood;
			}
		}

		public bool CantMove
		{
			get
			{
				return this.Resting || this.AnyPawnHasExtremeMentalBreak || this.AllOwnersHaveMentalBreak || this.AllOwnersDowned || this.ImmobilizedByMass;
			}
		}

		public float MassCapacity
		{
			get
			{
				return CollectionsMassCalculator.Capacity<Pawn>(this.PawnsListForReading);
			}
		}

		public float MassUsage
		{
			get
			{
				return CollectionsMassCalculator.MassUsage<Pawn>(this.PawnsListForReading, IgnorePawnsInventoryMode.DontIgnore, false, false);
			}
		}

		public bool AllOwnersDowned
		{
			get
			{
				for (int i = 0; i < this.pawns.Count; i++)
				{
					if (this.IsOwner(this.pawns[i]) && !this.pawns[i].Downed)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool AllOwnersHaveMentalBreak
		{
			get
			{
				for (int i = 0; i < this.pawns.Count; i++)
				{
					if (this.IsOwner(this.pawns[i]) && !this.pawns[i].InMentalState)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool Resting
		{
			get
			{
				return CaravanRestUtility.RestingNowAt(base.Tile);
			}
		}

		public int LeftRestTicks
		{
			get
			{
				return CaravanRestUtility.LeftRestTicksAt(base.Tile);
			}
		}

		public int LeftNonRestTicks
		{
			get
			{
				return CaravanRestUtility.LeftNonRestTicksAt(base.Tile);
			}
		}

		public override string Label
		{
			get
			{
				if (this.nameInt != null)
				{
					return this.nameInt;
				}
				return base.Label;
			}
		}

		private bool AnyPawnHasExtremeMentalBreak
		{
			get
			{
				return this.FirstPawnWithExtremeMentalBreak != null;
			}
		}

		private Pawn FirstPawnWithExtremeMentalBreak
		{
			get
			{
				for (int i = 0; i < this.pawns.Count; i++)
				{
					if (this.pawns[i].InMentalState && this.pawns[i].MentalStateDef.IsExtreme)
					{
						return this.pawns[i];
					}
				}
				return null;
			}
		}

		public int TicksPerMove
		{
			get
			{
				return CaravanTicksPerMoveUtility.GetTicksPerMove(this);
			}
		}

		public override bool AppendFactionToInspectString
		{
			get
			{
				return false;
			}
		}

		public StoryState StoryState
		{
			get
			{
				return this.storyState;
			}
		}

		public GameConditionManager GameConditionManager
		{
			get
			{
				Log.ErrorOnce("Attempted to retrieve condition manager directly from caravan", 13291050);
				return null;
			}
		}

		public float PlayerWealthForStoryteller
		{
			get
			{
				if (!this.IsPlayerControlled)
				{
					return 0f;
				}
				float num = 0f;
				for (int i = 0; i < this.pawns.Count; i++)
				{
					num += WealthWatcher.GetEquipmentApparelAndInventoryWealth(this.pawns[i]);
				}
				return num * 0.5f;
			}
		}

		public IEnumerable<Pawn> FreeColonistsForStoryteller
		{
			get
			{
				if (!this.IsPlayerControlled)
				{
					return Enumerable.Empty<Pawn>();
				}
				return from x in this.PawnsListForReading
				where x.IsFreeColonist
				select x;
			}
		}

		public FloatRange IncidentPointsRandomFactorRange
		{
			get
			{
				return CaravanIncidentUtility.IncidentPointsRandomFactorRange;
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
				return this.trader.Goods;
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
				return 0f;
			}
		}

		public Caravan()
		{
			this.pawns = new ThingOwner<Pawn>(this, false, LookMode.Reference);
			this.pather = new Caravan_PathFollower(this);
			this.gotoMote = new Caravan_GotoMoteRenderer();
			this.tweener = new Caravan_Tweener(this);
			this.trader = new Caravan_TraderTracker(this);
			this.storyState = new StoryState(this);
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
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
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.pawns.RemoveAll((Pawn x) => x.Destroyed);
			}
			Scribe_Values.Look<string>(ref this.nameInt, "name", null, false);
			Scribe_Deep.Look<ThingOwner<Pawn>>(ref this.pawns, "pawns", new object[]
			{
				this
			});
			Scribe_Values.Look<bool>(ref this.autoJoinable, "autoJoinable", false, false);
			Scribe_Deep.Look<Caravan_PathFollower>(ref this.pather, "pather", new object[]
			{
				this
			});
			Scribe_Deep.Look<Caravan_TraderTracker>(ref this.trader, "trader", new object[]
			{
				this
			});
			Scribe_Deep.Look<StoryState>(ref this.storyState, "storyState", new object[]
			{
				this
			});
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.CaravanPostLoadInit(this);
			}
		}

		public override void PostAdd()
		{
			base.PostAdd();
			Find.ColonistBar.MarkColonistsDirty();
		}

		public override void PostRemove()
		{
			base.PostRemove();
			this.pather.StopDead();
			Find.ColonistBar.MarkColonistsDirty();
		}

		public override void Tick()
		{
			base.Tick();
			this.CheckAnyNonWorldPawns();
			this.pather.PatherTick();
			this.tweener.TweenerTick();
			CaravanPawnsNeedsUtility.TrySatisfyPawnsNeeds(this);
			if (this.IsHashIntervalTick(120))
			{
				CaravanDrugPolicyUtility.TryTakeScheduledDrugs(this);
			}
			if (this.IsHashIntervalTick(2000))
			{
				CaravanTendUtility.TryTendToRandomPawn(this);
			}
		}

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			this.tweener.ResetToPosition();
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (this.IsPlayerControlled && this.pather.curPath != null)
			{
				this.pather.curPath.DrawPath(this);
			}
			this.gotoMote.RenderMote();
		}

		public void AddPawn(Pawn p, bool addCarriedPawnToWorldPawnsIfAny)
		{
			if (p == null)
			{
				Log.Warning("Tried to add a null pawn to " + this);
				return;
			}
			if (p.Dead)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to add ",
					p,
					" to ",
					this,
					", but this pawn is dead."
				}));
				return;
			}
			Pawn pawn = p.carryTracker.CarriedThing as Pawn;
			if (p.Spawned)
			{
				p.DeSpawn();
			}
			if (this.pawns.TryAdd(p, true))
			{
				if (pawn != null)
				{
					p.carryTracker.innerContainer.Remove(pawn);
					this.AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny);
					if (addCarriedPawnToWorldPawnsIfAny)
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					}
				}
			}
			else
			{
				Log.Error("Couldn't add pawn " + p + " to caravan.");
			}
		}

		public void AddPawnOrItem(Thing thing, bool addCarriedPawnToWorldPawnsIfAny)
		{
			if (thing == null)
			{
				Log.Warning("Tried to add a null thing to " + this);
				return;
			}
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				this.AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny);
			}
			else
			{
				CaravanInventoryUtility.GiveThing(this, thing);
			}
		}

		public bool ContainsPawn(Pawn p)
		{
			return this.pawns.Contains(p);
		}

		public void RemovePawn(Pawn p)
		{
			this.pawns.Remove(p);
		}

		public void RemoveAllPawns()
		{
			this.pawns.Clear();
		}

		public bool IsOwner(Pawn p)
		{
			return this.pawns.Contains(p) && CaravanUtility.IsOwner(p, base.Faction);
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			if (this.Resting)
			{
				stringBuilder.Append("CaravanResting".Translate());
			}
			else if (this.AnyPawnHasExtremeMentalBreak)
			{
				stringBuilder.Append("CaravanMemberMentalBreak".Translate(new object[]
				{
					this.FirstPawnWithExtremeMentalBreak.LabelShort
				}));
			}
			else if (this.AllOwnersDowned)
			{
				stringBuilder.Append("AllCaravanMembersDowned".Translate());
			}
			else if (this.AllOwnersHaveMentalBreak)
			{
				stringBuilder.Append("AllCaravanMembersMentalBreak".Translate());
			}
			else if (this.pather.Moving)
			{
				if (this.pather.arrivalAction != null)
				{
					stringBuilder.Append(this.pather.arrivalAction.ReportString);
				}
				else
				{
					stringBuilder.Append("CaravanTraveling".Translate());
				}
			}
			else
			{
				Settlement settlement = CaravanVisitUtility.SettlementVisitedNow(this);
				if (settlement != null)
				{
					stringBuilder.Append("CaravanVisiting".Translate(new object[]
					{
						settlement.Label
					}));
				}
				else
				{
					stringBuilder.Append("CaravanWaiting".Translate());
				}
			}
			if (this.pather.Moving)
			{
				float num = (float)CaravanArrivalTimeEstimator.EstimatedTicksToArrive(this, true) / 60000f;
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanEstimatedTimeToDestination".Translate(new object[]
				{
					num.ToString("0.#")
				}));
			}
			if (this.ImmobilizedByMass)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanImmobilizedByMass".Translate());
			}
			string text;
			if (CaravanPawnsNeedsUtility.AnyPawnOutOfFood(this, out text))
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanOutOfFood".Translate());
				if (!text.NullOrEmpty())
				{
					stringBuilder.Append(" ");
					stringBuilder.Append(text);
					stringBuilder.Append(".");
				}
			}
			else if (this.DaysWorthOfFood.First < 1000f)
			{
				Pair<float, float> daysWorthOfFood = this.DaysWorthOfFood;
				stringBuilder.AppendLine();
				if (daysWorthOfFood.Second < 1000f)
				{
					stringBuilder.Append("CaravanDaysOfFoodRot".Translate(new object[]
					{
						daysWorthOfFood.First.ToString("0.#"),
						daysWorthOfFood.Second.ToString("0.#")
					}));
				}
				else
				{
					stringBuilder.Append("CaravanDaysOfFood".Translate(new object[]
					{
						daysWorthOfFood.First.ToString("0.#")
					}));
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(string.Concat(new string[]
			{
				"CaravanBaseMovementTime".Translate(),
				": ",
				((float)this.TicksPerMove / 2500f).ToString("0.##"),
				" ",
				"CaravanHoursPerTile".Translate()
			}));
			stringBuilder.AppendLine("CurrentTileMovementTime".Translate() + ": " + Caravan_PathFollower.CostToDisplay(this, base.Tile, this.pather.nextTile, -1f).ToStringTicksToPeriod(true, false, true));
			stringBuilder.Append("StealthFactor".Translate() + ": " + CaravanIncidentUtility.CalculateCaravanStealthFactor(this.PawnsListForReading.Count).ToString("F1"));
			return stringBuilder.ToString();
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (this.IsPlayerControlled)
			{
				if (CaravanMergeUtility.ShouldShowMergeCommand)
				{
					yield return CaravanMergeUtility.MergeCommand(this);
				}
				if (Find.WorldSelector.SingleSelectedObject == this)
				{
					if (this.PawnsListForReading.Count((Pawn x) => x.IsColonist) >= 2)
					{
						yield return new Command_Action
						{
							defaultLabel = "CommandSplitCaravan".Translate(),
							defaultDesc = "CommandSplitCaravanDesc".Translate(),
							icon = Caravan.SplitCommand,
							action = delegate
							{
								Find.WindowStack.Add(new Dialog_SplitCaravan(this.$this));
							}
						};
					}
				}
				if (Find.WorldSelector.SingleSelectedObject == this)
				{
					yield return SettleInEmptyTileUtility.SettleCommand(this);
				}
				foreach (WorldObject wo in Find.WorldObjects.ObjectsAt(base.Tile))
				{
					foreach (Gizmo gizmo in wo.GetCaravanGizmos(this))
					{
						yield return gizmo;
					}
				}
			}
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "Dev: Mental break",
					action = delegate
					{
						Pawn pawn;
						if ((from x in this.$this.PawnsListForReading
						where x.RaceProps.Humanlike && !x.InMentalState
						select x).TryRandomElement(out pawn))
						{
							pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.WanderSad, null, false, false, null);
						}
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Dev: Extreme mental break",
					action = delegate
					{
						Pawn pawn;
						if ((from x in this.$this.PawnsListForReading
						where x.RaceProps.Humanlike && !x.InMentalState
						select x).TryRandomElement(out pawn))
						{
							pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false, false, null);
						}
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Dev: Make random pawn hungry",
					action = delegate
					{
						Pawn pawn;
						if ((from x in this.$this.PawnsListForReading
						where x.needs.food != null
						select x).TryRandomElement(out pawn))
						{
							pawn.needs.food.CurLevelPercentage = 0f;
						}
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Dev: Kill random pawn",
					action = delegate
					{
						Pawn pawn;
						if (this.$this.PawnsListForReading.TryRandomElement(out pawn))
						{
							pawn.Kill(null, null);
							Messages.Message("Dev: Killed " + pawn.LabelShort, this.$this, MessageTypeDefOf.TaskCompletion);
						}
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Dev: Harm random pawn",
					action = delegate
					{
						Pawn pawn;
						if (this.$this.PawnsListForReading.TryRandomElement(out pawn))
						{
							DamageInfo dinfo = new DamageInfo(DamageDefOf.Scratch, 10, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
							pawn.TakeDamage(dinfo);
						}
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Dev: Down random pawn",
					action = delegate
					{
						Pawn pawn;
						if ((from x in this.$this.PawnsListForReading
						where !x.Downed
						select x).TryRandomElement(out pawn))
						{
							HealthUtility.DamageUntilDowned(pawn);
							Messages.Message("Dev: Downed " + pawn.LabelShort, this.$this, MessageTypeDefOf.TaskCompletion);
						}
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Dev: Teleport to destination",
					action = delegate
					{
						this.$this.Tile = this.$this.pather.Destination;
						this.$this.pather.StopDead();
					}
				};
			}
		}

		public void RecacheImmobilizedNow()
		{
			this.cachedImmobilizedForTicks = -99999;
		}

		public void RecacheDaysWorthOfFood()
		{
			this.cachedDaysWorthOfFoodForTicks = -99999;
		}

		public virtual void Notify_MemberDied(Pawn member)
		{
			if (!this.PawnsListForReading.Any((Pawn x) => x != member && this.IsOwner(x)))
			{
				this.RemovePawn(member);
				if (base.Faction == Faction.OfPlayer)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAllCaravanColonistsDied".Translate(), "LetterAllCaravanColonistsDied".Translate(), LetterDefOf.NegativeEvent, new GlobalTargetInfo(base.Tile), null);
				}
				Find.WorldObjects.Remove(this);
			}
			else
			{
				member.Strip();
				this.RemovePawn(member);
			}
		}

		private void CheckAnyNonWorldPawns()
		{
			for (int i = this.pawns.Count - 1; i >= 0; i--)
			{
				if (!this.pawns[i].IsWorldPawn())
				{
					Log.Error("Caravan member " + this.pawns[i] + " is not a world pawn. Removing...");
					this.pawns.Remove(this.pawns[i]);
				}
			}
		}

		public void Notify_PawnRemoved(Pawn p)
		{
			Find.ColonistBar.MarkColonistsDirty();
			this.RecacheImmobilizedNow();
			this.RecacheDaysWorthOfFood();
		}

		public void Notify_PawnAdded(Pawn p)
		{
			Find.ColonistBar.MarkColonistsDirty();
			this.RecacheImmobilizedNow();
			this.RecacheDaysWorthOfFood();
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.pawns;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
	}
}
