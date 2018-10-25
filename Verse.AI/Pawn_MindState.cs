using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class Pawn_MindState : IExposable
	{
		public Pawn pawn;

		public MentalStateHandler mentalStateHandler;

		public MentalBreaker mentalBreaker;

		public InspirationHandler inspirationHandler;

		public PriorityWork priorityWork;

		private bool activeInt = true;

		public JobTag lastJobTag;

		public int lastIngestTick = -99999;

		public int nextApparelOptimizeTick = -99999;

		public ThinkNode lastJobGiver;

		public ThinkTreeDef lastJobGiverThinkTree;

		public WorkTypeDef lastGivenWorkType;

		public bool canFleeIndividual = true;

		public int exitMapAfterTick = -99999;

		public int lastDisturbanceTick = -99999;

		public IntVec3 forcedGotoPosition = IntVec3.Invalid;

		public Thing knownExploder;

		public bool wantsToTradeWithColony;

		public Thing lastMannedThing;

		public int canLovinTick = -99999;

		public int canSleepTick = -99999;

		public Pawn meleeThreat;

		public int lastMeleeThreatHarmTick = -99999;

		public int lastEngageTargetTick = -99999;

		public int lastAttackTargetTick = -99999;

		public LocalTargetInfo lastAttackedTarget;

		public Thing enemyTarget;

		public PawnDuty duty;

		public Dictionary<int, int> thinkData = new Dictionary<int, int>();

		public int lastAssignedInteractTime = -99999;

		public int interactionsToday;

		public int lastInventoryRawFoodUseTick;

		public bool nextMoveOrderIsWait;

		public int lastTakeCombatEnhancingDrugTick = -99999;

		public int lastHarmTick = -99999;

		public bool anyCloseHostilesRecently;

		public int applyBedThoughtsTick;

		public bool applyBedThoughtsOnLeave;

		public bool willJoinColonyIfRescuedInt;

		private bool wildManEverReachedOutsideInt;

		public int timesGuestTendedToByPlayer;

		public int lastSelfTendTick = -99999;

		public bool spawnedByInfestationThingComp;

		public int lastPredatorHuntingPlayerNotificationTick = -99999;

		public float maxDistToSquadFlag = -1f;

		private int lastJobGiverKey = -1;

		private const int UpdateAnyCloseHostilesRecentlyEveryTicks = 100;

		private const int AnyCloseHostilesRecentlyRegionsToScan_ToActivate = 18;

		private const int AnyCloseHostilesRecentlyRegionsToScan_ToDeactivate = 24;

		private const float HarmForgetDistance = 3f;

		private const int MeleeHarmForgetDelay = 400;

		public bool Active
		{
			get
			{
				return this.activeInt;
			}
			set
			{
				if (value != this.activeInt)
				{
					this.activeInt = value;
					if (this.pawn.Spawned)
					{
						this.pawn.Map.mapPawns.UpdateRegistryForPawn(this.pawn);
					}
				}
			}
		}

		public bool IsIdle
		{
			get
			{
				return !this.pawn.Downed && this.pawn.Spawned && this.lastJobTag == JobTag.Idle;
			}
		}

		public bool MeleeThreatStillThreat
		{
			get
			{
				return this.meleeThreat != null && this.meleeThreat.Spawned && !this.meleeThreat.Downed && this.pawn.Spawned && Find.TickManager.TicksGame <= this.lastMeleeThreatHarmTick + 400 && (float)(this.pawn.Position - this.meleeThreat.Position).LengthHorizontalSquared <= 9f && GenSight.LineOfSight(this.pawn.Position, this.meleeThreat.Position, this.pawn.Map, false, null, 0, 0);
			}
		}

		public bool WildManEverReachedOutside
		{
			get
			{
				return this.wildManEverReachedOutsideInt;
			}
			set
			{
				if (this.wildManEverReachedOutsideInt == value)
				{
					return;
				}
				this.wildManEverReachedOutsideInt = value;
				ReachabilityUtility.ClearCacheFor(this.pawn);
			}
		}

		public bool WillJoinColonyIfRescued
		{
			get
			{
				return this.willJoinColonyIfRescuedInt;
			}
			set
			{
				if (this.willJoinColonyIfRescuedInt == value)
				{
					return;
				}
				this.willJoinColonyIfRescuedInt = value;
				if (this.pawn.Spawned)
				{
					this.pawn.Map.attackTargetsCache.UpdateTarget(this.pawn);
				}
			}
		}

		public bool AnythingPreventsJoiningColonyIfRescued
		{
			get
			{
				return this.pawn.Faction == Faction.OfPlayer || (this.pawn.IsPrisoner && !this.pawn.HostFaction.HostileTo(Faction.OfPlayer)) || (!this.pawn.IsPrisoner && this.pawn.Faction != null && this.pawn.Faction.HostileTo(Faction.OfPlayer) && !this.pawn.Downed);
			}
		}

		public Pawn_MindState()
		{
		}

		public Pawn_MindState(Pawn pawn)
		{
			this.pawn = pawn;
			this.mentalStateHandler = new MentalStateHandler(pawn);
			this.mentalBreaker = new MentalBreaker(pawn);
			this.inspirationHandler = new InspirationHandler(pawn);
			this.priorityWork = new PriorityWork(pawn);
		}

		public void Reset(bool clearInspiration = false)
		{
			this.mentalStateHandler.Reset();
			this.mentalBreaker.Reset();
			if (clearInspiration)
			{
				this.inspirationHandler.Reset();
			}
			this.activeInt = true;
			this.lastJobTag = JobTag.Misc;
			this.lastIngestTick = -99999;
			this.nextApparelOptimizeTick = -99999;
			this.lastJobGiver = null;
			this.lastJobGiverThinkTree = null;
			this.lastGivenWorkType = null;
			this.canFleeIndividual = true;
			this.exitMapAfterTick = -99999;
			this.lastDisturbanceTick = -99999;
			this.forcedGotoPosition = IntVec3.Invalid;
			this.knownExploder = null;
			this.wantsToTradeWithColony = false;
			this.lastMannedThing = null;
			this.canLovinTick = -99999;
			this.canSleepTick = -99999;
			this.meleeThreat = null;
			this.lastMeleeThreatHarmTick = -99999;
			this.lastEngageTargetTick = -99999;
			this.lastAttackTargetTick = -99999;
			this.lastAttackedTarget = LocalTargetInfo.Invalid;
			this.enemyTarget = null;
			this.duty = null;
			this.thinkData.Clear();
			this.lastAssignedInteractTime = -99999;
			this.interactionsToday = 0;
			this.lastInventoryRawFoodUseTick = 0;
			this.priorityWork.Clear();
			this.nextMoveOrderIsWait = true;
			this.lastTakeCombatEnhancingDrugTick = -99999;
			this.lastHarmTick = -99999;
			this.anyCloseHostilesRecently = false;
			this.WillJoinColonyIfRescued = false;
			this.WildManEverReachedOutside = false;
			this.timesGuestTendedToByPlayer = 0;
			this.lastSelfTendTick = -99999;
			this.spawnedByInfestationThingComp = false;
			this.lastPredatorHuntingPlayerNotificationTick = -99999;
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.lastJobGiverKey = ((this.lastJobGiver == null) ? -1 : this.lastJobGiver.UniqueSaveKey);
			}
			Scribe_Values.Look<int>(ref this.lastJobGiverKey, "lastJobGiverKey", -1, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.lastJobGiverKey != -1 && !this.lastJobGiverThinkTree.TryGetThinkNodeWithSaveKey(this.lastJobGiverKey, out this.lastJobGiver))
			{
				Log.Warning("Could not find think node with key " + this.lastJobGiverKey, false);
			}
			Scribe_References.Look<Pawn>(ref this.meleeThreat, "meleeThreat", false);
			Scribe_References.Look<Thing>(ref this.enemyTarget, "enemyTarget", false);
			Scribe_References.Look<Thing>(ref this.knownExploder, "knownExploder", false);
			Scribe_References.Look<Thing>(ref this.lastMannedThing, "lastMannedThing", false);
			Scribe_Defs.Look<ThinkTreeDef>(ref this.lastJobGiverThinkTree, "lastJobGiverThinkTree");
			Scribe_TargetInfo.Look(ref this.lastAttackedTarget, "lastAttackedTarget");
			Scribe_Collections.Look<int, int>(ref this.thinkData, "thinkData", LookMode.Value, LookMode.Value);
			Scribe_Values.Look<bool>(ref this.activeInt, "active", true, false);
			Scribe_Values.Look<JobTag>(ref this.lastJobTag, "lastJobTag", JobTag.Misc, false);
			Scribe_Values.Look<int>(ref this.lastIngestTick, "lastIngestTick", -99999, false);
			Scribe_Values.Look<int>(ref this.nextApparelOptimizeTick, "nextApparelOptimizeTick", -99999, false);
			Scribe_Values.Look<int>(ref this.lastEngageTargetTick, "lastEngageTargetTick", 0, false);
			Scribe_Values.Look<int>(ref this.lastAttackTargetTick, "lastAttackTargetTick", 0, false);
			Scribe_Values.Look<bool>(ref this.canFleeIndividual, "canFleeIndividual", false, false);
			Scribe_Values.Look<int>(ref this.exitMapAfterTick, "exitMapAfterTick", -99999, false);
			Scribe_Values.Look<IntVec3>(ref this.forcedGotoPosition, "forcedGotoPosition", IntVec3.Invalid, false);
			Scribe_Values.Look<int>(ref this.lastMeleeThreatHarmTick, "lastMeleeThreatHarmTick", 0, false);
			Scribe_Values.Look<int>(ref this.lastAssignedInteractTime, "lastAssignedInteractTime", -99999, false);
			Scribe_Values.Look<int>(ref this.interactionsToday, "interactionsToday", 0, false);
			Scribe_Values.Look<int>(ref this.lastInventoryRawFoodUseTick, "lastInventoryRawFoodUseTick", 0, false);
			Scribe_Values.Look<int>(ref this.lastDisturbanceTick, "lastDisturbanceTick", -99999, false);
			Scribe_Values.Look<bool>(ref this.wantsToTradeWithColony, "wantsToTradeWithColony", false, false);
			Scribe_Values.Look<int>(ref this.canLovinTick, "canLovinTick", -99999, false);
			Scribe_Values.Look<int>(ref this.canSleepTick, "canSleepTick", -99999, false);
			Scribe_Values.Look<bool>(ref this.nextMoveOrderIsWait, "nextMoveOrderIsWait", true, false);
			Scribe_Values.Look<int>(ref this.lastTakeCombatEnhancingDrugTick, "lastTakeCombatEnhancingDrugTick", -99999, false);
			Scribe_Values.Look<int>(ref this.lastHarmTick, "lastHarmTick", -99999, false);
			Scribe_Values.Look<bool>(ref this.anyCloseHostilesRecently, "anyCloseHostilesRecently", false, false);
			Scribe_Deep.Look<PawnDuty>(ref this.duty, "duty", new object[0]);
			Scribe_Deep.Look<MentalStateHandler>(ref this.mentalStateHandler, "mentalStateHandler", new object[]
			{
				this.pawn
			});
			Scribe_Deep.Look<MentalBreaker>(ref this.mentalBreaker, "mentalBreaker", new object[]
			{
				this.pawn
			});
			Scribe_Deep.Look<InspirationHandler>(ref this.inspirationHandler, "inspirationHandler", new object[]
			{
				this.pawn
			});
			Scribe_Deep.Look<PriorityWork>(ref this.priorityWork, "priorityWork", new object[]
			{
				this.pawn
			});
			Scribe_Values.Look<int>(ref this.applyBedThoughtsTick, "applyBedThoughtsTick", 0, false);
			Scribe_Values.Look<bool>(ref this.applyBedThoughtsOnLeave, "applyBedThoughtsOnLeave", false, false);
			Scribe_Values.Look<bool>(ref this.willJoinColonyIfRescuedInt, "willJoinColonyIfRescued", false, false);
			Scribe_Values.Look<bool>(ref this.wildManEverReachedOutsideInt, "wildManEverReachedOutside", false, false);
			Scribe_Values.Look<int>(ref this.timesGuestTendedToByPlayer, "timesGuestTendedToByPlayer", 0, false);
			Scribe_Values.Look<int>(ref this.lastSelfTendTick, "lastSelfTendTick", 0, false);
			Scribe_Values.Look<bool>(ref this.spawnedByInfestationThingComp, "spawnedByInfestationThingComp", false, false);
			Scribe_Values.Look<int>(ref this.lastPredatorHuntingPlayerNotificationTick, "lastPredatorHuntingPlayerNotificationTick", -99999, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.MindStatePostLoadInit(this);
			}
		}

		public void MindStateTick()
		{
			if (this.wantsToTradeWithColony)
			{
				TradeUtility.CheckInteractWithTradersTeachOpportunity(this.pawn);
			}
			if (this.meleeThreat != null && !this.MeleeThreatStillThreat)
			{
				this.meleeThreat = null;
			}
			this.mentalStateHandler.MentalStateHandlerTick();
			this.mentalBreaker.MentalBreakerTick();
			this.inspirationHandler.InspirationHandlerTick();
			if (!this.pawn.GetPosture().Laying())
			{
				this.applyBedThoughtsTick = 0;
			}
			if (this.pawn.IsHashIntervalTick(100))
			{
				if (this.pawn.Spawned)
				{
					int regionsToScan = (!this.anyCloseHostilesRecently) ? 18 : 24;
					this.anyCloseHostilesRecently = PawnUtility.EnemiesAreNearby(this.pawn, regionsToScan, true);
				}
				else
				{
					this.anyCloseHostilesRecently = false;
				}
			}
			if (this.WillJoinColonyIfRescued && this.AnythingPreventsJoiningColonyIfRescued)
			{
				this.WillJoinColonyIfRescued = false;
			}
			if (this.pawn.Spawned && this.pawn.IsWildMan() && !this.WildManEverReachedOutside && this.pawn.GetRoom(RegionType.Set_Passable) != null && this.pawn.GetRoom(RegionType.Set_Passable).TouchesMapEdge)
			{
				this.WildManEverReachedOutside = true;
			}
			if (Find.TickManager.TicksGame % 123 == 0 && this.pawn.Spawned && this.pawn.RaceProps.IsFlesh && this.pawn.needs.mood != null)
			{
				TerrainDef terrain = this.pawn.Position.GetTerrain(this.pawn.Map);
				if (terrain.traversedThought != null)
				{
					this.pawn.needs.mood.thoughts.memories.TryGainMemoryFast(terrain.traversedThought);
				}
				WeatherDef curWeatherLerped = this.pawn.Map.weatherManager.CurWeatherLerped;
				if (curWeatherLerped.exposedThought != null && !this.pawn.Position.Roofed(this.pawn.Map))
				{
					this.pawn.needs.mood.thoughts.memories.TryGainMemoryFast(curWeatherLerped.exposedThought);
				}
			}
			if (GenLocalDate.DayTick(this.pawn) == 0)
			{
				this.interactionsToday = 0;
			}
		}

		public void JoinColonyBecauseRescuedBy(Pawn by)
		{
			this.WillJoinColonyIfRescued = false;
			if (this.AnythingPreventsJoiningColonyIfRescued)
			{
				return;
			}
			InteractionWorker_RecruitAttempt.DoRecruit(by, this.pawn, 1f, false);
			if (this.pawn.needs != null && this.pawn.needs.mood != null)
			{
				this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Rescued, null);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelRescueQuestFinished".Translate(), "LetterRescueQuestFinished".Translate(this.pawn.Named("PAWN")).AdjustedFor(this.pawn, "PAWN").CapitalizeFirst(), LetterDefOf.PositiveEvent, this.pawn, null, null);
		}

		public void ResetLastDisturbanceTick()
		{
			this.lastDisturbanceTick = -9999999;
		}

		[DebuggerHidden]
		public IEnumerable<Gizmo> GetGizmos()
		{
			if (this.pawn.IsColonistPlayerControlled)
			{
				foreach (Gizmo g in this.priorityWork.GetGizmos())
				{
					yield return g;
				}
			}
			foreach (Gizmo g2 in CaravanFormingUtility.GetGizmos(this.pawn))
			{
				yield return g2;
			}
		}

		public void Notify_OutfitChanged()
		{
			this.nextApparelOptimizeTick = Find.TickManager.TicksGame;
		}

		public void Notify_WorkPriorityDisabled(WorkTypeDef wType)
		{
			JobGiver_Work jobGiver_Work = this.lastJobGiver as JobGiver_Work;
			if (jobGiver_Work != null && this.lastGivenWorkType == wType)
			{
				this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
			}
		}

		public void Notify_DamageTaken(DamageInfo dinfo)
		{
			this.mentalStateHandler.Notify_DamageTaken(dinfo);
			if (dinfo.Def.ExternalViolenceFor(this.pawn))
			{
				this.lastHarmTick = Find.TickManager.TicksGame;
				if (this.pawn.Spawned)
				{
					Pawn pawn = dinfo.Instigator as Pawn;
					if (!this.mentalStateHandler.InMentalState && dinfo.Instigator != null && (pawn != null || dinfo.Instigator is Building_Turret) && dinfo.Instigator.Faction != null && (dinfo.Instigator.Faction.def.humanlikeFaction || (pawn != null && pawn.def.race.intelligence >= Intelligence.ToolUser)) && this.pawn.Faction == null && (this.pawn.RaceProps.Animal || this.pawn.IsWildMan()) && (this.pawn.CurJob == null || this.pawn.CurJob.def != JobDefOf.PredatorHunt || dinfo.Instigator != ((JobDriver_PredatorHunt)this.pawn.jobs.curDriver).Prey) && Rand.Chance(PawnUtility.GetManhunterOnDamageChance(this.pawn, dinfo.Instigator)))
					{
						this.StartManhunterBecauseOfPawnAction("AnimalManhunterFromDamage");
					}
					else if (dinfo.Instigator != null && dinfo.Def.makesAnimalsFlee && Pawn_MindState.CanStartFleeingBecauseOfPawnAction(this.pawn))
					{
						this.StartFleeingBecauseOfPawnAction(dinfo.Instigator);
					}
				}
				if (this.pawn.GetPosture() != PawnPosture.Standing)
				{
					this.lastDisturbanceTick = Find.TickManager.TicksGame;
				}
			}
		}

		internal void Notify_EngagedTarget()
		{
			this.lastEngageTargetTick = Find.TickManager.TicksGame;
		}

		internal void Notify_AttackedTarget(LocalTargetInfo target)
		{
			this.lastAttackTargetTick = Find.TickManager.TicksGame;
			this.lastAttackedTarget = target;
		}

		internal bool CheckStartMentalStateBecauseRecruitAttempted(Pawn tamer)
		{
			if (!this.pawn.RaceProps.Animal && (!this.pawn.IsWildMan() || this.pawn.IsPrisoner))
			{
				return false;
			}
			if (!this.mentalStateHandler.InMentalState && this.pawn.Faction == null && Rand.Chance(this.pawn.RaceProps.manhunterOnTameFailChance))
			{
				this.StartManhunterBecauseOfPawnAction("AnimalManhunterFromTaming");
				return true;
			}
			return false;
		}

		internal void Notify_DangerousExploderAboutToExplode(Thing exploder)
		{
			if (this.pawn.RaceProps.intelligence >= Intelligence.Humanlike)
			{
				this.knownExploder = exploder;
				this.pawn.jobs.CheckForJobOverride();
			}
		}

		public void Notify_Explosion(Explosion explosion)
		{
			if (this.pawn.Faction != null)
			{
				return;
			}
			if (explosion.radius < 3.5f || !this.pawn.Position.InHorDistOf(explosion.Position, explosion.radius + 7f))
			{
				return;
			}
			if (Pawn_MindState.CanStartFleeingBecauseOfPawnAction(this.pawn))
			{
				this.StartFleeingBecauseOfPawnAction(explosion);
			}
		}

		public void Notify_TuckedIntoBed()
		{
			if (this.pawn.IsWildMan())
			{
				this.WildManEverReachedOutside = false;
			}
		}

		public void Notify_SelfTended()
		{
			this.lastSelfTendTick = Find.TickManager.TicksGame;
		}

		public void Notify_PredatorHuntingPlayerNotification()
		{
			this.lastPredatorHuntingPlayerNotificationTick = Find.TickManager.TicksGame;
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> GetPackmates(Pawn pawn, float radius)
		{
			Room pawnRoom = pawn.GetRoom(RegionType.Set_Passable);
			List<Pawn> raceMates = pawn.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < raceMates.Count; i++)
			{
				if (pawn != raceMates[i] && raceMates[i].def == pawn.def && raceMates[i].Faction == pawn.Faction && raceMates[i].Position.InHorDistOf(pawn.Position, radius) && raceMates[i].GetRoom(RegionType.Set_Passable) == pawnRoom)
				{
					yield return raceMates[i];
				}
			}
		}

		private void StartManhunterBecauseOfPawnAction(string letterTextKey)
		{
			if (!this.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false, false, null, false))
			{
				return;
			}
			string text = letterTextKey.Translate(this.pawn.Label, this.pawn.Named("PAWN")).AdjustedFor(this.pawn, "PAWN");
			GlobalTargetInfo target = this.pawn;
			int num = 1;
			if (Find.Storyteller.difficulty.allowBigThreats && Rand.Value < 0.5f)
			{
				foreach (Pawn current in this.GetPackmates(this.pawn, 24f))
				{
					if (current.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false, false, null, false))
					{
						num++;
					}
				}
				if (num > 1)
				{
					target = new TargetInfo(this.pawn.Position, this.pawn.Map, false);
					text += "\n\n";
					text += "AnimalManhunterOthers".Translate(this.pawn.kindDef.GetLabelPlural(-1), this.pawn);
				}
			}
			string value = (!this.pawn.RaceProps.Animal) ? this.pawn.def.label : this.pawn.Label;
			string label = "LetterLabelAnimalManhunterRevenge".Translate(value).CapitalizeFirst();
			Find.LetterStack.ReceiveLetter(label, text, (num != 1) ? LetterDefOf.ThreatBig : LetterDefOf.ThreatSmall, target, null, null);
		}

		private static bool CanStartFleeingBecauseOfPawnAction(Pawn p)
		{
			return p.RaceProps.Animal && !p.InMentalState && !p.IsFighting() && !p.Downed && !p.Dead && !ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(p);
		}

		public void StartFleeingBecauseOfPawnAction(Thing instigator)
		{
			List<Thing> threats = new List<Thing>
			{
				instigator
			};
			IntVec3 fleeDest = CellFinderLoose.GetFleeDest(this.pawn, threats, this.pawn.Position.DistanceTo(instigator.Position) + 14f);
			if (fleeDest != this.pawn.Position)
			{
				this.pawn.jobs.StartJob(new Job(JobDefOf.Flee, fleeDest, instigator), JobCondition.InterruptOptional, null, false, true, null, null, false);
			}
			if (this.pawn.RaceProps.herdAnimal && Rand.Chance(0.1f))
			{
				foreach (Pawn current in this.GetPackmates(this.pawn, 24f))
				{
					if (Pawn_MindState.CanStartFleeingBecauseOfPawnAction(current))
					{
						IntVec3 fleeDest2 = CellFinderLoose.GetFleeDest(current, threats, current.Position.DistanceTo(instigator.Position) + 14f);
						if (fleeDest2 != current.Position)
						{
							current.jobs.StartJob(new Job(JobDefOf.Flee, fleeDest2, instigator), JobCondition.InterruptOptional, null, false, true, null, null, false);
						}
					}
				}
			}
		}
	}
}
