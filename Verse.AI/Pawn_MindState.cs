using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class Pawn_MindState : IExposable
	{
		private const int UpdateAnyCloseHostilesRecentlyEveryTicks = 120;

		private const int AnyCloseHostilesRecentlyRegionsToScan = 18;

		public Pawn pawn;

		public MentalStateHandler mentalStateHandler;

		public MentalBreaker mentalBreaker;

		public PriorityWork priorityWork = new PriorityWork();

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

		public int lastMeleeThreatHarmTick;

		public int lastEngageTargetTick;

		public Thing enemyTarget;

		public PawnDuty duty;

		public Dictionary<int, int> thinkData = new Dictionary<int, int>();

		public int lastAssignedInteractTime = -99999;

		public int lastInventoryRawFoodUseTick;

		public bool nextMoveOrderIsWait = true;

		public int lastTakeCombatEnancingDrugTick = -99999;

		public int lastHarmTick = -99999;

		public bool anyCloseHostilesRecently;

		private int lastJobGiverKey = -1;

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
					Find.MapPawns.UpdateRegistryForPawn(this.pawn);
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

		public Pawn_MindState()
		{
		}

		public Pawn_MindState(Pawn pawn)
		{
			this.pawn = pawn;
			this.mentalStateHandler = new MentalStateHandler(pawn);
			this.mentalBreaker = new MentalBreaker(pawn);
		}

		public void Reset()
		{
			this.mentalStateHandler.Reset();
			this.mentalBreaker.Reset();
			this.activeInt = true;
			this.lastJobTag = JobTag.NoTag;
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
			this.lastMeleeThreatHarmTick = 0;
			this.lastEngageTargetTick = 0;
			this.enemyTarget = null;
			this.duty = null;
			this.thinkData.Clear();
			this.lastAssignedInteractTime = -99999;
			this.lastInventoryRawFoodUseTick = 0;
			this.priorityWork.Clear();
			this.nextMoveOrderIsWait = true;
			this.lastTakeCombatEnancingDrugTick = -99999;
			this.lastHarmTick = -99999;
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (this.enemyTarget != null && this.enemyTarget.Destroyed)
				{
					this.enemyTarget = null;
				}
				if (this.knownExploder != null && this.knownExploder.Destroyed)
				{
					this.knownExploder = null;
				}
			}
			Scribe_Values.LookValue<bool>(ref this.activeInt, "active", true, false);
			Scribe_Values.LookValue<JobTag>(ref this.lastJobTag, "lastJobTag", JobTag.NoTag, false);
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.lastJobGiverKey = ((this.lastJobGiver == null) ? -1 : this.lastJobGiver.UniqueSaveKey);
			}
			Scribe_Values.LookValue<int>(ref this.lastJobGiverKey, "lastJobGiverKey", -1, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.lastJobGiverKey != -1 && !this.lastJobGiverThinkTree.TryGetThinkNodeWithSaveKey(this.lastJobGiverKey, out this.lastJobGiver))
			{
				Log.Warning("Could not find think node with key " + this.lastJobGiverKey);
			}
			Scribe_Defs.LookDef<ThinkTreeDef>(ref this.lastJobGiverThinkTree, "lastJobGiverThinkTree");
			Scribe_Values.LookValue<int>(ref this.lastIngestTick, "lastIngestTick", -99999, false);
			Scribe_Values.LookValue<int>(ref this.nextApparelOptimizeTick, "nextApparelOptimizeTick", -99999, false);
			Scribe_Values.LookValue<int>(ref this.lastEngageTargetTick, "lastEngageTargetTick", 0, false);
			Scribe_Values.LookValue<bool>(ref this.canFleeIndividual, "canFleeIndividual", false, false);
			Scribe_Values.LookValue<int>(ref this.exitMapAfterTick, "exitMapAfterTick", -99999, false);
			Scribe_Values.LookValue<IntVec3>(ref this.forcedGotoPosition, "forcedGotoPosition", IntVec3.Invalid, false);
			Scribe_Values.LookValue<int>(ref this.lastMeleeThreatHarmTick, "lastMeleeThreatHarmTick", 0, false);
			Scribe_Values.LookValue<int>(ref this.lastAssignedInteractTime, "lastAssignedInteractTime", -99999, false);
			Scribe_Values.LookValue<int>(ref this.lastInventoryRawFoodUseTick, "lastInventoryRawFoodUseTick", 0, false);
			Scribe_Values.LookValue<int>(ref this.lastDisturbanceTick, "lastDisturbanceTick", -99999, false);
			Scribe_Values.LookValue<bool>(ref this.wantsToTradeWithColony, "wantsToTradeWithColony", false, false);
			Scribe_Values.LookValue<int>(ref this.canLovinTick, "canLovinTick", -99999, false);
			Scribe_Values.LookValue<int>(ref this.canSleepTick, "canSleepTick", -99999, false);
			Scribe_Values.LookValue<bool>(ref this.nextMoveOrderIsWait, "nextMoveOrderIsWait", true, false);
			Scribe_Values.LookValue<int>(ref this.lastTakeCombatEnancingDrugTick, "lastTakeCombatEnancingDrugTick", -99999, false);
			Scribe_Values.LookValue<int>(ref this.lastHarmTick, "lastHarmTick", -99999, false);
			Scribe_Values.LookValue<bool>(ref this.anyCloseHostilesRecently, "anyCloseHostilesRecently", false, false);
			Scribe_Collections.LookDictionary<int, int>(ref this.thinkData, "thinkData", LookMode.Undefined, LookMode.Undefined);
			Scribe_References.LookReference<Pawn>(ref this.meleeThreat, "meleeThreat", false);
			Scribe_References.LookReference<Thing>(ref this.enemyTarget, "enemyTarget", false);
			Scribe_References.LookReference<Thing>(ref this.knownExploder, "knownExploder", false);
			Scribe_References.LookReference<Thing>(ref this.lastMannedThing, "lastMannedThing", false);
			Scribe_Deep.LookDeep<PawnDuty>(ref this.duty, "duty", new object[0]);
			Scribe_Deep.LookDeep<MentalStateHandler>(ref this.mentalStateHandler, "mentalStateHandler", new object[]
			{
				this.pawn
			});
			Scribe_Deep.LookDeep<MentalBreaker>(ref this.mentalBreaker, "mentalBreaker", new object[]
			{
				this.pawn
			});
			Scribe_Deep.LookDeep<PriorityWork>(ref this.priorityWork, "priorityWork", new object[0]);
		}

		public void MindTick()
		{
			if (this.wantsToTradeWithColony)
			{
				TradeUtility.CheckInteractWithTradersTeachOpportunity(this.pawn);
			}
			if (this.meleeThreat != null && (this.meleeThreat.Downed || !this.meleeThreat.Spawned || this.pawn.Downed || !this.pawn.Spawned))
			{
				this.meleeThreat = null;
			}
			this.mentalStateHandler.MentalStateHandlerTick();
			this.mentalBreaker.MentalStateStarterTick();
			if (this.pawn.IsHashIntervalTick(120))
			{
				if (this.pawn.Spawned)
				{
					this.anyCloseHostilesRecently = PawnUtility.EnemiesAreNearby(this.pawn, 18, true);
				}
				else
				{
					this.anyCloseHostilesRecently = false;
				}
			}
		}

		public void ResetLastDisturbanceTick()
		{
			this.lastDisturbanceTick = -9999999;
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
				this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}

		public void Notify_DamageTaken(DamageInfo dinfo)
		{
			if (dinfo.Def.externalViolence)
			{
				this.lastHarmTick = Find.TickManager.TicksGame;
				Pawn pawn = dinfo.Instigator as Pawn;
				if (!this.mentalStateHandler.InMentalState && dinfo.Instigator != null && (pawn != null || dinfo.Instigator is Building_Turret) && dinfo.Instigator.Faction != null && (dinfo.Instigator.Faction.def.humanlikeFaction || (pawn != null && pawn.def.race.intelligence >= Intelligence.ToolUser)) && this.pawn.Faction == null && (this.pawn.CurJob == null || this.pawn.CurJob.def != JobDefOf.PredatorHunt) && Rand.Value < this.pawn.RaceProps.manhunterOnDamageChance)
				{
					this.StartManhunterBecauseOfPawnAction("AnimalManhunterFromDamage");
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

		internal bool CheckStartMentalStateBecauseRecruitAttempted(Pawn tamer)
		{
			if (!this.pawn.RaceProps.Animal)
			{
				return false;
			}
			if (!this.mentalStateHandler.InMentalState && this.pawn.Faction == null && Rand.Value < this.pawn.RaceProps.manhunterOnTameFailChance)
			{
				this.StartManhunterBecauseOfPawnAction("AnimalManhunterFromTaming");
				return true;
			}
			return false;
		}

		private void StartManhunterBecauseOfPawnAction(string letterTextKey)
		{
			if (!this.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false))
			{
				return;
			}
			string text = letterTextKey.Translate(new object[]
			{
				this.pawn.Label
			});
			if (Rand.Value < 0.5f)
			{
				int num = 1;
				Room room = this.pawn.GetRoom();
				List<Pawn> allPawnsSpawned = Find.MapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (this.pawn != allPawnsSpawned[i] && allPawnsSpawned[i].RaceProps == this.pawn.RaceProps && allPawnsSpawned[i].Faction == this.pawn.Faction && allPawnsSpawned[i].Position.InHorDistOf(this.pawn.Position, 24f) && allPawnsSpawned[i].GetRoom() == room && allPawnsSpawned[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false))
					{
						num++;
					}
				}
				if (num > 1)
				{
					text += "\n\n";
					text += ((!"AnimalManhunterOthers".CanTranslate()) ? "AnimalManhunterFromDamageOthers".Translate(new object[]
					{
						this.pawn.def.label
					}) : "AnimalManhunterOthers".Translate(new object[]
					{
						this.pawn.def.label
					}));
				}
			}
			string label = (!"LetterLabelAnimalManhunterRevenge".CanTranslate()) ? "LetterLabelAnimalManhunterFromDamage".Translate(new object[]
			{
				this.pawn.Label
			}).CapitalizeFirst() : "LetterLabelAnimalManhunterRevenge".Translate(new object[]
			{
				this.pawn.Label
			}).CapitalizeFirst();
			Find.LetterStack.ReceiveLetter(label, text, LetterType.BadNonUrgent, this.pawn, null);
		}

		internal void Notify_DangerousExploderAboutToExplode(Thing exploder)
		{
			if (this.pawn.RaceProps.intelligence >= Intelligence.Humanlike)
			{
				this.knownExploder = exploder;
				this.pawn.jobs.CheckForJobOverride();
			}
		}
	}
}
