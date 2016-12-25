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
	public class Caravan : WorldObject, IIncidentTarget, ITrader, ILoadReferenceable
	{
		private const int ImmobilizedCacheDuration = 60;

		private const int DaysWorthOfFoodCacheDuration = 3000;

		public const float WakeUpHour = 6f;

		public const float RestStartHour = 22f;

		public const float CellToTilesConversionRatio = 190f;

		private string nameInt;

		private List<Pawn> pawns = new List<Pawn>();

		public bool autoJoinable;

		public Caravan_PathFollower pather;

		public Caravan_GotoMoteRenderer gotoMote;

		public Caravan_Tweener tweener;

		public Caravan_TraderTracker trader;

		private Material cachedMat;

		private bool cachedImmobilized;

		private int cachedImmobilizedForTicks = -99999;

		private float cachedDaysWorthOfFood;

		private int cachedDaysWorthOfFoodForTicks = -99999;

		private static readonly Texture2D SplitCommand = ContentFinder<Texture2D>.Get("UI/Commands/SplitCaravan", true);

		private static readonly Color PlayerCaravanColor = new Color(1f, 0.863f, 0.33f);

		private static List<Pawn> tmpPawns = new List<Pawn>();

		public List<Pawn> PawnsListForReading
		{
			get
			{
				return this.pawns;
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
					this.cachedMat = MaterialPool.MatFrom(this.def.texture, ShaderDatabase.WorldOverlayTransparentLit, color);
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

		public float DaysWorthOfFood
		{
			get
			{
				if (Find.TickManager.TicksGame - this.cachedDaysWorthOfFoodForTicks < 3000)
				{
					return this.cachedDaysWorthOfFood;
				}
				this.cachedDaysWorthOfFood = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this);
				this.cachedDaysWorthOfFoodForTicks = Find.TickManager.TicksGame;
				return this.cachedDaysWorthOfFood;
			}
		}

		public bool CantMove
		{
			get
			{
				return this.Resting || this.AnyPawnHasExtremeMentalBreak || this.AllOwnersDowned || this.ImmobilizedByMass;
			}
		}

		public float MassCapacity
		{
			get
			{
				return CollectionsMassCalculator.Capacity<Pawn>(this.pawns);
			}
		}

		public float MassUsage
		{
			get
			{
				return CollectionsMassCalculator.MassUsage<Pawn>(this.pawns, false, false, false);
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

		public bool Resting
		{
			get
			{
				float num = GenLocalDate.DayPercent(base.Tile) * 24f;
				return num < 6f || num > 22f;
			}
		}

		public int LeftRestTicks
		{
			get
			{
				if (!this.Resting)
				{
					return 0;
				}
				float num = GenLocalDate.DayPercent(base.Tile) * 24f;
				if (num < 6f)
				{
					return Mathf.CeilToInt((6f - num) * 2500f);
				}
				return Mathf.CeilToInt((24f - num + 6f) * 2500f);
			}
		}

		public int LeftNonRestTicks
		{
			get
			{
				if (this.Resting)
				{
					return 0;
				}
				float num = GenLocalDate.DayPercent(base.Tile) * 24f;
				return Mathf.CeilToInt((22f - num) * 2500f);
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
				float num = 0f;
				if (this.pawns.Count > 0)
				{
					for (int i = 0; i < this.pawns.Count; i++)
					{
						num += (float)this.pawns[i].TicksPerMoveCardinal / (float)this.pawns.Count;
					}
					num *= 190f;
				}
				else
				{
					Log.ErrorOnce("Caravan " + this + " has no pawns.", 983746153);
					num = 100f;
				}
				return Mathf.Max(Mathf.RoundToInt(num), 1);
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
			this.pather = new Caravan_PathFollower(this);
			this.gotoMote = new Caravan_GotoMoteRenderer();
			this.tweener = new Caravan_Tweener(this);
			this.trader = new Caravan_TraderTracker(this);
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
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.pawns.RemoveAll((Pawn x) => x.Destroyed);
			}
			Scribe_Values.LookValue<string>(ref this.nameInt, "name", null, false);
			Scribe_Collections.LookList<Pawn>(ref this.pawns, "pawns", LookMode.Reference, new object[0]);
			Scribe_Values.LookValue<bool>(ref this.autoJoinable, "autoJoinable", false, false);
			Scribe_Deep.LookDeep<Caravan_PathFollower>(ref this.pather, "pather", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Caravan_TraderTracker>(ref this.trader, "trader", new object[]
			{
				this
			});
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.pawns.RemoveAll((Pawn x) => x == null) != 0)
				{
					Log.Warning("Some of the caravan's pawns were null after loading.");
				}
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
			if (this.pawns.Contains(p))
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to add ",
					p,
					" to ",
					this,
					", but this pawn is already here."
				}));
				return;
			}
			Pawn pawn = p.carryTracker.CarriedThing as Pawn;
			if (pawn != null)
			{
				p.carryTracker.innerContainer.Remove(pawn);
				this.AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny);
				if (addCarriedPawnToWorldPawnsIfAny)
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
				}
			}
			this.pawns.Add(p);
			Find.ColonistBar.MarkColonistsDirty();
			this.RecacheImmobilizedNow();
			this.RecacheDaysWorthOfFood();
		}

		public bool ContainsPawn(Pawn p)
		{
			return this.pawns.Contains(p);
		}

		public void RemovePawn(Pawn p)
		{
			if (!this.pawns.Contains(p))
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to remove ",
					p,
					" from ",
					this,
					", but this pawn is not here."
				}));
				return;
			}
			this.pawns.Remove(p);
			Find.ColonistBar.MarkColonistsDirty();
			this.RecacheImmobilizedNow();
			this.RecacheDaysWorthOfFood();
		}

		public void RemoveAllPawnsAndDiscardIfUnimportant()
		{
			Caravan.tmpPawns.Clear();
			Caravan.tmpPawns.AddRange(this.pawns);
			for (int i = 0; i < Caravan.tmpPawns.Count; i++)
			{
				this.RemovePawn(Caravan.tmpPawns[i]);
				Find.WorldPawns.DiscardIfUnimportant(Caravan.tmpPawns[i]);
			}
		}

		public void RemoveAllPawns()
		{
			Caravan.tmpPawns.Clear();
			Caravan.tmpPawns.AddRange(this.pawns);
			for (int i = 0; i < Caravan.tmpPawns.Count; i++)
			{
				this.RemovePawn(Caravan.tmpPawns[i]);
			}
		}

		public bool IsOwner(Pawn p)
		{
			return this.pawns.Contains(p) && CaravanUtility.IsOwner(p, base.Faction);
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
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
				FactionBase factionBase = CaravanVisitUtility.FactionBaseVisitedNow(this);
				if (factionBase != null)
				{
					stringBuilder.Append("CaravanVisiting".Translate(new object[]
					{
						factionBase.Label
					}));
				}
				else
				{
					stringBuilder.Append("CaravanWaiting".Translate());
				}
			}
			if (this.pather.Moving)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanEstimatedTimeToDestination".Translate(new object[]
				{
					CaravanArrivalTimeEstimator.EstimatedTicksToArrive(this, true).ToStringTicksToPeriod(true)
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
			else
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("CaravanDaysOfFood".Translate(new object[]
				{
					this.DaysWorthOfFood.ToString("0.#")
				}));
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
			stringBuilder.Append("CurrentTileMovementTime".Translate() + ": " + (this.TicksPerMove + WorldPathGrid.CalculatedCostAt(base.Tile, false, -1f)).ToStringTicksToPeriod(true));
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
				if (CaravanMergeUtility.CanMergeAnySelectedCaravans)
				{
					yield return CaravanMergeUtility.MergeCommand(this);
				}
				if (Find.WorldSelector.SingleSelectedObject == this)
				{
					if (this.pawns.Count((Pawn x) => x.IsColonist) >= 2)
					{
						yield return new Command_Action
						{
							defaultLabel = "CommandSplitCaravan".Translate(),
							defaultDesc = "CommandSplitCaravanDesc".Translate(),
							icon = Caravan.SplitCommand,
							action = delegate
							{
								Find.WindowStack.Add(new Dialog_SplitCaravan(this.<>f__this));
							}
						};
					}
				}
				if (Find.WorldSelector.SingleSelectedObject == this)
				{
					yield return CaravanSettleUtility.SettleCommand(this);
				}
				FactionBase factionBaseVisitedNow = CaravanVisitUtility.FactionBaseVisitedNow(this);
				if (factionBaseVisitedNow != null && factionBaseVisitedNow.CanTradeNow)
				{
					yield return CaravanVisitUtility.TradeCommand(this);
				}
				if (CaravanJourneyDestinationUtility.AnyJurneyDestinationAt(base.Tile))
				{
					yield return CaravanJourneyDestinationUtility.TakeOffCommand(base.Tile);
				}
			}
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "Dev: Extreme mental break",
					action = delegate
					{
						Pawn pawn;
						if ((from x in this.<>f__this.pawns
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
						if ((from x in this.<>f__this.pawns
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
						if (this.<>f__this.pawns.TryRandomElement(out pawn))
						{
							pawn.health.Kill(null, null);
							Messages.Message("Dev: Killed " + pawn.LabelShort, this.<>f__this, MessageSound.Benefit);
						}
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Dev: Harm random pawn",
					action = delegate
					{
						Pawn pawn;
						if (this.<>f__this.pawns.TryRandomElement(out pawn))
						{
							DamageInfo dinfo = new DamageInfo(DamageDefOf.Scratch, 10, -1f, null, null, null);
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
						if ((from x in this.<>f__this.pawns
						where !x.Downed
						select x).TryRandomElement(out pawn))
						{
							HealthUtility.GiveInjuriesToForceDowned(pawn);
							Messages.Message("Dev: Downed " + pawn.LabelShort, this.<>f__this, MessageSound.Benefit);
						}
					}
				};
				if (this.IsPlayerControlled)
				{
					yield return new Command_Action
					{
						defaultLabel = "Dev: Execute incident",
						action = delegate
						{
							List<FloatMenuOption> list = new List<FloatMenuOption>();
							List<IncidentDef> allDefsListForReading = DefDatabase<IncidentDef>.AllDefsListForReading;
							for (int i = 0; i < allDefsListForReading.Count; i++)
							{
								if (allDefsListForReading[i].TargetAllowed(this.<>f__this))
								{
									IncidentDef localDef = allDefsListForReading[i];
									string text = localDef.defName;
									if (!localDef.Worker.CanFireNow(this.<>f__this))
									{
										text += " [NO]";
									}
									list.Add(new FloatMenuOption(text, delegate
									{
										IncidentParms incidentParms = new IncidentParms();
										incidentParms.target = this.<>f__this;
										localDef.Worker.TryExecute(incidentParms);
									}, MenuOptionPriority.Default, null, null, 0f, null, null));
								}
							}
							Find.WindowStack.Add(new FloatMenu(list));
						}
					};
				}
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
			this.RemovePawn(member);
			if (!this.pawns.Any((Pawn x) => this.IsOwner(x)))
			{
				if (base.Faction == Faction.OfPlayer)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAllCaravanColonistsDied".Translate(), "LetterAllCaravanColonistsDied".Translate(), LetterType.BadNonUrgent, new GlobalTargetInfo(base.Tile), null);
				}
				this.RemoveAllPawnsAndDiscardIfUnimportant();
				Find.WorldObjects.Remove(this);
			}
			else
			{
				CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(member, this.pawns, null);
			}
		}

		private void CheckAnyNonWorldPawns()
		{
			for (int i = this.pawns.Count - 1; i >= 0; i--)
			{
				if (!this.pawns[i].IsWorldPawn())
				{
					Log.Error("Caravan member " + this.pawns[i] + " is not a world pawn. Removing...");
					this.pawns.RemoveAt(i);
				}
			}
		}

		virtual int get_Tile()
		{
			return base.Tile;
		}
	}
}
