using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class Pawn_GuestTracker : IExposable
	{
		private Pawn pawn;

		public PrisonerInteractionModeDef interactionMode = PrisonerInteractionModeDefOf.NoInteraction;

		private Faction hostFactionInt;

		public bool isPrisonerInt;

		private bool releasedInt;

		private int ticksWhenAllowedToEscapeAgain;

		public IntVec3 spotToWaitInsteadOfEscaping = IntVec3.Invalid;

		public int lastPrisonBreakTicks = -1;

		public bool everParticipatedInPrisonBreak;

		public float resistance = -1f;

		public bool getRescuedThoughtOnUndownedBecauseOfPlayer;

		private const int DefaultWaitInsteadOfEscapingTicks = 25000;

		public const int MinInteractionInterval = 10000;

		public const int MaxInteractionsPerDay = 2;

		private const int CheckInitiatePrisonBreakIntervalTicks = 2500;

		private static readonly SimpleCurve StartingResistancePerRecruitDifficultyCurve = new SimpleCurve
		{
			{
				new CurvePoint(0.1f, 0f),
				true
			},
			{
				new CurvePoint(0.5f, 15f),
				true
			},
			{
				new CurvePoint(0.9f, 25f),
				true
			},
			{
				new CurvePoint(1f, 50f),
				true
			}
		};

		private static readonly SimpleCurve StartingResistanceFactorFromPopulationIntentCurve = new SimpleCurve
		{
			{
				new CurvePoint(-1f, 2f),
				true
			},
			{
				new CurvePoint(0f, 1.5f),
				true
			},
			{
				new CurvePoint(1f, 1f),
				true
			},
			{
				new CurvePoint(2f, 0.8f),
				true
			}
		};

		private static readonly FloatRange StartingResistanceRandomFactorRange = new FloatRange(0.8f, 1.2f);

		public Faction HostFaction
		{
			get
			{
				return this.hostFactionInt;
			}
		}

		public bool CanBeBroughtFood
		{
			get
			{
				return this.interactionMode != PrisonerInteractionModeDefOf.Execution && (this.interactionMode != PrisonerInteractionModeDefOf.Release || this.pawn.Downed);
			}
		}

		public bool IsPrisoner
		{
			get
			{
				return this.isPrisonerInt;
			}
		}

		public bool ScheduledForInteraction
		{
			get
			{
				return this.pawn.mindState.lastAssignedInteractTime < Find.TickManager.TicksGame - 10000 && this.pawn.mindState.interactionsToday < 2;
			}
		}

		public bool Released
		{
			get
			{
				return this.releasedInt;
			}
			set
			{
				if (value == this.releasedInt)
				{
					return;
				}
				this.releasedInt = value;
				ReachabilityUtility.ClearCacheFor(this.pawn);
			}
		}

		public bool PrisonerIsSecure
		{
			get
			{
				if (this.Released)
				{
					return false;
				}
				if (this.pawn.HostFaction == null)
				{
					return false;
				}
				if (this.pawn.InMentalState)
				{
					return false;
				}
				if (this.pawn.Spawned)
				{
					if (this.pawn.jobs.curJob != null && this.pawn.jobs.curJob.exitMapOnArrival)
					{
						return false;
					}
					if (PrisonBreakUtility.IsPrisonBreaking(this.pawn))
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool ShouldWaitInsteadOfEscaping
		{
			get
			{
				if (!this.IsPrisoner)
				{
					return false;
				}
				Map mapHeld = this.pawn.MapHeld;
				return mapHeld != null && mapHeld.mapPawns.FreeColonistsSpawnedCount != 0 && Find.TickManager.TicksGame < this.ticksWhenAllowedToEscapeAgain;
			}
		}

		public float Resistance
		{
			get
			{
				return this.resistance;
			}
		}

		public Pawn_GuestTracker()
		{
		}

		public Pawn_GuestTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void GuestTrackerTick()
		{
			if (this.pawn.IsHashIntervalTick(2500))
			{
				float num = PrisonBreakUtility.InitiatePrisonBreakMtbDays(this.pawn);
				if (num >= 0f && Rand.MTBEventOccurs(num, 60000f, 2500f))
				{
					PrisonBreakUtility.StartPrisonBreak(this.pawn);
				}
			}
		}

		public void ExposeData()
		{
			Scribe_References.Look<Faction>(ref this.hostFactionInt, "hostFaction", false);
			Scribe_Values.Look<bool>(ref this.isPrisonerInt, "prisoner", false, false);
			Scribe_Defs.Look<PrisonerInteractionModeDef>(ref this.interactionMode, "interactionMode");
			Scribe_Values.Look<bool>(ref this.releasedInt, "released", false, false);
			Scribe_Values.Look<int>(ref this.ticksWhenAllowedToEscapeAgain, "ticksWhenAllowedToEscapeAgain", 0, false);
			Scribe_Values.Look<IntVec3>(ref this.spotToWaitInsteadOfEscaping, "spotToWaitInsteadOfEscaping", default(IntVec3), false);
			Scribe_Values.Look<int>(ref this.lastPrisonBreakTicks, "lastPrisonBreakTicks", 0, false);
			Scribe_Values.Look<bool>(ref this.everParticipatedInPrisonBreak, "everParticipatedInPrisonBreak", false, false);
			Scribe_Values.Look<bool>(ref this.getRescuedThoughtOnUndownedBecauseOfPlayer, "getRescuedThoughtOnUndownedBecauseOfPlayer", false, false);
			Scribe_Values.Look<float>(ref this.resistance, "resistance", -1f, false);
		}

		public void SetGuestStatus(Faction newHost, bool prisoner = false)
		{
			if (newHost != null)
			{
				this.Released = false;
			}
			if (newHost == this.HostFaction && prisoner == this.IsPrisoner)
			{
				return;
			}
			if (!prisoner && this.pawn.Faction.HostileTo(newHost))
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to make ",
					this.pawn,
					" a guest of ",
					newHost,
					" but their faction ",
					this.pawn.Faction,
					" is hostile to ",
					newHost
				}), false);
				return;
			}
			if (newHost != null && newHost == this.pawn.Faction && !prisoner)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to make ",
					this.pawn,
					" a guest of their own faction ",
					this.pawn.Faction
				}), false);
				return;
			}
			bool flag = prisoner && (!this.IsPrisoner || this.HostFaction != newHost);
			this.isPrisonerInt = prisoner;
			this.hostFactionInt = newHost;
			this.pawn.ClearMind(false, false);
			if (flag)
			{
				this.pawn.DropAndForbidEverything(false);
				Lord lord = this.pawn.GetLord();
				if (lord != null)
				{
					lord.Notify_PawnLost(this.pawn, PawnLostCondition.MadePrisoner, null);
				}
				if (newHost == Faction.OfPlayer)
				{
					Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(this.pawn, PopAdaptationEvent.GainedPrisoner);
				}
				if (this.pawn.Drafted)
				{
					this.pawn.drafter.Drafted = false;
				}
				float x = this.pawn.RecruitDifficulty(Faction.OfPlayer);
				this.resistance = Pawn_GuestTracker.StartingResistancePerRecruitDifficultyCurve.Evaluate(x);
				this.resistance *= Pawn_GuestTracker.StartingResistanceFactorFromPopulationIntentCurve.Evaluate(StorytellerUtilityPopulation.PopulationIntent);
				this.resistance *= Pawn_GuestTracker.StartingResistanceRandomFactorRange.RandomInRange;
				this.resistance = (float)GenMath.RoundRandom(this.resistance);
			}
			PawnComponentsUtility.AddAndRemoveDynamicComponents(this.pawn, false);
			this.pawn.health.surgeryBills.Clear();
			if (this.pawn.ownership != null)
			{
				this.pawn.ownership.Notify_ChangedGuestStatus();
			}
			ReachabilityUtility.ClearCacheFor(this.pawn);
			if (this.pawn.Spawned)
			{
				this.pawn.Map.mapPawns.UpdateRegistryForPawn(this.pawn);
				this.pawn.Map.attackTargetsCache.UpdateTarget(this.pawn);
			}
			AddictionUtility.CheckDrugAddictionTeachOpportunity(this.pawn);
			if (prisoner && this.pawn.playerSettings != null)
			{
				this.pawn.playerSettings.Notify_MadePrisoner();
			}
		}

		public void CapturedBy(Faction by, Pawn byPawn = null)
		{
			if (this.pawn.Faction != null)
			{
				this.pawn.Faction.Notify_MemberCaptured(this.pawn, by);
			}
			this.SetGuestStatus(by, true);
			if (this.IsPrisoner && byPawn != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.Captured, new object[]
				{
					byPawn,
					this.pawn
				});
				byPawn.records.Increment(RecordDefOf.PeopleCaptured);
			}
		}

		public void WaitInsteadOfEscapingForDefaultTicks()
		{
			this.WaitInsteadOfEscapingFor(25000);
		}

		public void WaitInsteadOfEscapingFor(int ticks)
		{
			if (!this.IsPrisoner)
			{
				return;
			}
			this.ticksWhenAllowedToEscapeAgain = Find.TickManager.TicksGame + ticks;
			this.spotToWaitInsteadOfEscaping = IntVec3.Invalid;
		}

		internal void Notify_PawnUndowned()
		{
			if (this.pawn.RaceProps.Humanlike && (this.HostFaction == Faction.OfPlayer || (this.pawn.IsWildMan() && this.pawn.InBed() && this.pawn.CurrentBed().Faction == Faction.OfPlayer)) && !this.IsPrisoner && this.pawn.SpawnedOrAnyParentSpawned)
			{
				if (this.getRescuedThoughtOnUndownedBecauseOfPlayer && this.pawn.needs != null && this.pawn.needs.mood != null)
				{
					this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Rescued, null);
				}
				if (this.pawn.Faction == null || this.pawn.Faction.def.rescueesCanJoin)
				{
					Map mapHeld = this.pawn.MapHeld;
					float num;
					if (!this.pawn.SafeTemperatureRange().Includes(mapHeld.mapTemperature.OutdoorTemp) || mapHeld.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
					{
						num = 1f;
					}
					else
					{
						num = 0.5f;
					}
					if (Rand.ValueSeeded(this.pawn.thingIDNumber ^ 8976612) < num)
					{
						this.pawn.SetFaction(Faction.OfPlayer, null);
						Messages.Message("MessageRescueeJoined".Translate(this.pawn.LabelShort, this.pawn).AdjustedFor(this.pawn, "PAWN"), this.pawn, MessageTypeDefOf.PositiveEvent, true);
					}
				}
			}
			this.getRescuedThoughtOnUndownedBecauseOfPlayer = false;
		}
	}
}
