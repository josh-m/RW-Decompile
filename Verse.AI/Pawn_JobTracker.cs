using RimWorld;
using System;
using System.Collections.Generic;
using Verse.AI.Group;

namespace Verse.AI
{
	public class Pawn_JobTracker : IExposable
	{
		private const int ConstantThinkTreeJobCheckIntervalTicks = 30;

		private const int RecentJobQueueMaxLength = 10;

		private const int MaxRecentJobs = 10;

		private const int DamageCheckMinInterval = 180;

		protected Pawn pawn;

		public Job curJob;

		public JobDriver curDriver;

		public JobQueue jobQueue = new JobQueue();

		private int jobsGivenThisTick;

		private string jobsGivenThisTickTextual = string.Empty;

		private int lastJobGivenAtFrame = -1;

		private List<int> jobsGivenRecentTicks = new List<int>(10);

		private List<string> jobsGivenRecentTicksTextual = new List<string>(10);

		public bool debugLog;

		private bool startingErrorRecoverJob;

		private int lastDamageCheckTick = -99999;

		public bool HandlingFacing
		{
			get
			{
				return this.curDriver != null && this.curDriver.HandlingFacing;
			}
		}

		public Pawn_JobTracker(Pawn newPawn)
		{
			this.pawn = newPawn;
		}

		public virtual void ExposeData()
		{
			Scribe_Deep.Look<Job>(ref this.curJob, "curJob", new object[0]);
			Scribe_Deep.Look<JobDriver>(ref this.curDriver, "curDriver", new object[0]);
			Scribe_Deep.Look<JobQueue>(ref this.jobQueue, "jobQueue", new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (this.curDriver != null)
				{
					this.curDriver.pawn = this.pawn;
				}
				BackCompatibility.JobTrackerPostLoadInit(this);
			}
		}

		public virtual void JobTrackerTick()
		{
			this.jobsGivenThisTick = 0;
			this.jobsGivenThisTickTextual = string.Empty;
			if (this.pawn.IsHashIntervalTick(30))
			{
				ThinkResult thinkResult = this.DetermineNextConstantThinkTreeJob();
				if (thinkResult.IsValid && this.ShouldStartJobFromThinkTree(thinkResult))
				{
					this.CheckLeaveJoinableLordBecauseJobIssued(thinkResult);
					this.StartJob(thinkResult.Job, JobCondition.InterruptForced, thinkResult.SourceNode, false, false, this.pawn.thinker.ConstantThinkTree, thinkResult.Tag);
				}
			}
			if (this.curDriver != null)
			{
				if (this.curJob.expiryInterval > 0 && (Find.TickManager.TicksGame - this.curJob.startTick) % this.curJob.expiryInterval == 0 && Find.TickManager.TicksGame != this.curJob.startTick)
				{
					if (!this.curJob.expireRequiresEnemiesNearby || PawnUtility.EnemiesAreNearby(this.pawn, 25, false))
					{
						if (this.debugLog)
						{
							this.DebugLogEvent("Job expire");
						}
						if (!this.curJob.checkOverrideOnExpire)
						{
							this.EndCurrentJob(JobCondition.Succeeded, true);
						}
						else
						{
							this.CheckForJobOverride();
						}
						this.FinalizeTick();
						return;
					}
					if (this.debugLog)
					{
						this.DebugLogEvent("Job expire skipped because there are no enemies nearby");
					}
				}
				this.curDriver.DriverTick();
			}
			if (this.curJob == null && !this.pawn.Dead && this.pawn.mindState.Active && this.CanDoAnyJob())
			{
				if (this.debugLog)
				{
					this.DebugLogEvent("Starting job from Tick because curJob == null.");
				}
				this.TryFindAndStartJob();
			}
			this.FinalizeTick();
		}

		private void FinalizeTick()
		{
			this.jobsGivenRecentTicks.Add(this.jobsGivenThisTick);
			this.jobsGivenRecentTicksTextual.Add(this.jobsGivenThisTickTextual);
			while (this.jobsGivenRecentTicks.Count > 10)
			{
				this.jobsGivenRecentTicks.RemoveAt(0);
				this.jobsGivenRecentTicksTextual.RemoveAt(0);
			}
			if (this.jobsGivenThisTick != 0)
			{
				int num = 0;
				for (int i = 0; i < this.jobsGivenRecentTicks.Count; i++)
				{
					num += this.jobsGivenRecentTicks[i];
				}
				if (num >= 10)
				{
					string text = GenText.ToCommaList(this.jobsGivenRecentTicksTextual, true);
					this.jobsGivenRecentTicks.Clear();
					this.jobsGivenRecentTicksTextual.Clear();
					this.StartErrorRecoverJob(string.Concat(new object[]
					{
						this.pawn,
						" started ",
						10,
						" jobs in ",
						10,
						" ticks. List: ",
						text
					}));
				}
			}
		}

		public void StartJob(Job newJob, JobCondition lastJobEndCondition = JobCondition.None, ThinkNode jobGiver = null, bool resumeCurJobAfterwards = false, bool cancelBusyStances = true, ThinkTreeDef thinkTree = null, JobTag? tag = null)
		{
			if (!Find.TickManager.Paused || this.lastJobGivenAtFrame == RealTime.frameCount)
			{
				this.jobsGivenThisTick++;
				this.jobsGivenThisTickTextual = this.jobsGivenThisTickTextual + "(" + newJob.ToString() + ") ";
			}
			this.lastJobGivenAtFrame = RealTime.frameCount;
			if (this.jobsGivenThisTick > 10)
			{
				string text = this.jobsGivenThisTickTextual;
				this.jobsGivenThisTick = 0;
				this.jobsGivenThisTickTextual = string.Empty;
				this.StartErrorRecoverJob(string.Concat(new object[]
				{
					this.pawn,
					" started 10 jobs in one tick. newJob=",
					newJob,
					" jobGiver=",
					jobGiver,
					" jobList=",
					text
				}));
				return;
			}
			PawnPosture posture = this.pawn.GetPosture();
			LayingDownState layingDown = (this.pawn.jobs == null || this.pawn.jobs.curDriver == null) ? LayingDownState.NotLaying : this.pawn.jobs.curDriver.layingDown;
			if (this.debugLog)
			{
				this.DebugLogEvent(string.Concat(new object[]
				{
					"StartJob [",
					newJob,
					"] lastJobEndCondition=",
					lastJobEndCondition,
					", jobGiver=",
					jobGiver,
					", cancelBusyStances=",
					cancelBusyStances
				}));
			}
			if (cancelBusyStances && this.pawn.stances.FullBodyBusy)
			{
				this.pawn.stances.CancelBusyStanceHard();
			}
			if (this.curJob != null)
			{
				if (lastJobEndCondition == JobCondition.None)
				{
					Log.Warning(string.Concat(new object[]
					{
						this.pawn,
						" starting job ",
						newJob,
						" from JobGiver ",
						this.pawn.mindState.lastJobGiver,
						" while already having job ",
						this.curJob,
						" without a specific job end condition."
					}));
					lastJobEndCondition = JobCondition.InterruptForced;
				}
				if (resumeCurJobAfterwards && this.curJob.def.suspendable)
				{
					this.jobQueue.EnqueueFirst(this.curJob, null);
					if (this.debugLog)
					{
						this.DebugLogEvent("   JobQueue EnqueueFirst curJob: " + this.curJob);
					}
				}
				this.CleanupCurrentJob(lastJobEndCondition, !resumeCurJobAfterwards, cancelBusyStances);
			}
			if (newJob == null)
			{
				Log.Warning(this.pawn + " tried to start doing a null job.");
				return;
			}
			newJob.startTick = Find.TickManager.TicksGame;
			if (this.pawn.Drafted || newJob.playerForced)
			{
				newJob.ignoreForbidden = true;
				newJob.ignoreDesignations = true;
			}
			this.curJob = newJob;
			this.pawn.mindState.lastJobGiver = jobGiver;
			this.pawn.mindState.lastJobGiverThinkTree = thinkTree;
			if (tag.HasValue)
			{
				this.pawn.mindState.lastJobTag = tag.Value;
			}
			this.curDriver = this.curJob.MakeDriver(this.pawn);
			this.curDriver.Notify_Starting();
			this.curDriver.Notify_LastPosture(posture, layingDown);
			this.curDriver.SetupToils();
			this.curDriver.ReadyForNextToil();
		}

		public void EndCurrentJob(JobCondition condition, bool startNewJob = true)
		{
			if (this.debugLog)
			{
				this.DebugLogEvent(string.Concat(new object[]
				{
					"EndCurrentJob ",
					(this.curJob == null) ? "null" : this.curJob.ToString(),
					" condition=",
					condition,
					" curToil=",
					(this.curDriver == null) ? "null_driver" : this.curDriver.CurToilIndex.ToString()
				}));
			}
			Job job = this.curJob;
			this.CleanupCurrentJob(condition, true, true);
			if (startNewJob)
			{
				if (condition == JobCondition.ErroredPather || condition == JobCondition.Errored)
				{
					this.StartJob(new Job(JobDefOf.Wait, 250, false), JobCondition.None, null, false, true, null, null);
					return;
				}
				if (condition == JobCondition.Succeeded && job != null && job.def != JobDefOf.WaitMaintainPosture && !this.pawn.pather.Moving)
				{
					this.StartJob(new Job(JobDefOf.WaitMaintainPosture, 1, false), JobCondition.None, null, false, false, null, null);
				}
				else
				{
					this.TryFindAndStartJob();
				}
			}
		}

		private void CleanupCurrentJob(JobCondition condition, bool releaseReservations, bool cancelBusyStancesSoft = true)
		{
			if (this.debugLog)
			{
				this.DebugLogEvent(string.Concat(new object[]
				{
					"CleanupCurrentJob ",
					(this.curJob == null) ? "null" : this.curJob.def.ToString(),
					" condition ",
					condition
				}));
			}
			if (this.curJob == null)
			{
				return;
			}
			this.curDriver.ended = true;
			this.curDriver.Cleanup(condition);
			this.curDriver = null;
			this.curJob = null;
			if (releaseReservations)
			{
				this.pawn.ClearReservations(false);
			}
			if (cancelBusyStancesSoft)
			{
				this.pawn.stances.CancelBusyStanceSoft();
			}
			if (!this.pawn.Destroyed && this.pawn.carryTracker != null && this.pawn.carryTracker.CarriedThing != null)
			{
				Thing thing;
				this.pawn.carryTracker.TryDropCarriedThing(this.pawn.Position, ThingPlaceMode.Near, out thing, null);
			}
		}

		public void CheckForJobOverride()
		{
			if (this.debugLog)
			{
				this.DebugLogEvent("CheckForJobOverride");
			}
			ThinkTreeDef thinkTree;
			ThinkResult thinkResult = this.DetermineNextJob(out thinkTree);
			if (this.ShouldStartJobFromThinkTree(thinkResult))
			{
				this.CheckLeaveJoinableLordBecauseJobIssued(thinkResult);
				this.StartJob(thinkResult.Job, JobCondition.InterruptOptional, thinkResult.SourceNode, false, false, thinkTree, thinkResult.Tag);
			}
		}

		public void StopAll(bool ifLayingKeepLaying = false)
		{
			if (ifLayingKeepLaying && this.curJob != null && this.curDriver.layingDown != LayingDownState.NotLaying)
			{
				return;
			}
			this.CleanupCurrentJob(JobCondition.InterruptForced, true, true);
			this.jobQueue.Clear();
		}

		private void TryFindAndStartJob()
		{
			if (this.pawn.thinker == null)
			{
				Log.ErrorOnce(this.pawn + " did TryFindAndStartJob but had no thinker.", 8573261);
				return;
			}
			if (this.curJob != null)
			{
				Log.Warning(this.pawn + " doing TryFindAndStartJob while still having job " + this.curJob);
			}
			if (this.debugLog)
			{
				this.DebugLogEvent("TryFindAndStartJob");
			}
			if (!this.CanDoAnyJob())
			{
				if (this.debugLog)
				{
					this.DebugLogEvent("   CanDoAnyJob is false. Clearing queue and returning");
				}
				if (this.jobQueue != null)
				{
					this.jobQueue.Clear();
				}
				return;
			}
			ThinkTreeDef thinkTreeDef;
			ThinkResult result = this.DetermineNextJob(out thinkTreeDef);
			if (result.IsValid)
			{
				this.CheckLeaveJoinableLordBecauseJobIssued(result);
				ThinkNode sourceNode = result.SourceNode;
				ThinkTreeDef thinkTree = thinkTreeDef;
				this.StartJob(result.Job, JobCondition.None, sourceNode, false, false, thinkTree, result.Tag);
			}
		}

		private ThinkResult DetermineNextJob(out ThinkTreeDef thinkTree)
		{
			ThinkResult result = this.DetermineNextConstantThinkTreeJob();
			if (result.Job != null)
			{
				thinkTree = this.pawn.thinker.ConstantThinkTree;
				return result;
			}
			if (this.jobQueue != null)
			{
				while (this.jobQueue.Count > 0 && !this.jobQueue.Peek().job.CanBeginNow(this.pawn))
				{
					QueuedJob queuedJob = this.jobQueue.Dequeue();
					if (this.debugLog)
					{
						this.DebugLogEvent("   Throwing away queued job that I cannot begin now: " + queuedJob.job);
					}
				}
				if (this.jobQueue.Count > 0)
				{
					QueuedJob queuedJob2 = this.jobQueue.Dequeue();
					if (this.debugLog)
					{
						this.DebugLogEvent("   Returning queued job: " + queuedJob2.job);
					}
					thinkTree = null;
					return new ThinkResult(queuedJob2.job, null, queuedJob2.tag);
				}
			}
			ThinkResult result2 = ThinkResult.NoJob;
			try
			{
				result2 = this.pawn.thinker.MainThinkNodeRoot.TryIssueJobPackage(this.pawn, default(JobIssueParams));
			}
			catch (Exception ex)
			{
				this.StartErrorRecoverJob(this.pawn + " threw exception while determining job (main): " + ex.ToString());
				thinkTree = null;
				return ThinkResult.NoJob;
			}
			finally
			{
			}
			thinkTree = this.pawn.thinker.MainThinkTree;
			return result2;
		}

		private ThinkResult DetermineNextConstantThinkTreeJob()
		{
			if (this.pawn.thinker.ConstantThinkTree == null)
			{
				return ThinkResult.NoJob;
			}
			try
			{
				return this.pawn.thinker.ConstantThinkNodeRoot.TryIssueJobPackage(this.pawn, default(JobIssueParams));
			}
			catch (Exception ex)
			{
				this.StartErrorRecoverJob(this.pawn + " threw exception while determining job (constant): " + ex.ToString());
			}
			finally
			{
			}
			return ThinkResult.NoJob;
		}

		public void StartErrorRecoverJob(string message)
		{
			string text = message + " lastJobGiver=" + this.pawn.mindState.lastJobGiver;
			if (this.curJob != null)
			{
				text = text + ", curJob.def=" + this.curJob.def.defName;
			}
			if (this.curDriver != null)
			{
				text = text + ", curDriver=" + this.curDriver.GetType();
			}
			Log.Error(text);
			if (this.curJob != null)
			{
				this.EndCurrentJob(JobCondition.Errored, false);
			}
			if (this.startingErrorRecoverJob)
			{
				Log.Error("An error occurred while starting an error recover job. We have to stop now to avoid infinite loops. This means that the pawn is now jobless which can cause further bugs. pawn=" + this.pawn.ToStringSafe<Pawn>());
			}
			else
			{
				this.startingErrorRecoverJob = true;
				try
				{
					this.StartJob(new Job(JobDefOf.Wait, 150, false), JobCondition.None, null, false, true, null, null);
				}
				finally
				{
					this.startingErrorRecoverJob = false;
				}
			}
		}

		private void CheckLeaveJoinableLordBecauseJobIssued(ThinkResult result)
		{
			if (!result.IsValid || result.SourceNode == null)
			{
				return;
			}
			Lord lord = this.pawn.GetLord();
			if (lord == null || !(lord.LordJob is LordJob_VoluntarilyJoinable))
			{
				return;
			}
			bool flag = false;
			ThinkNode thinkNode = result.SourceNode;
			while (!thinkNode.leaveJoinableLordIfIssuesJob)
			{
				thinkNode = thinkNode.parent;
				if (thinkNode == null)
				{
					IL_6F:
					if (flag)
					{
						lord.Notify_PawnLost(this.pawn, PawnLostCondition.LeftVoluntarily);
					}
					return;
				}
			}
			flag = true;
			goto IL_6F;
		}

		private bool CanDoAnyJob()
		{
			return this.pawn.Spawned;
		}

		private bool ShouldStartJobFromThinkTree(ThinkResult thinkResult)
		{
			return this.curJob == null || (thinkResult.Job.def != this.curJob.def || thinkResult.SourceNode != this.pawn.mindState.lastJobGiver || !this.curDriver.IsContinuation(thinkResult.Job));
		}

		public bool IsCurrentJobPlayerInterruptible()
		{
			return (this.curJob == null || this.curJob.def.playerInterruptible) && !this.pawn.HasAttachment(ThingDefOf.Fire);
		}

		public bool TryTakeOrderedJobPrioritizedWork(Job job, WorkGiver giver, IntVec3 cell)
		{
			if (this.TryTakeOrderedJob(job, giver.def.tagToGive))
			{
				this.pawn.mindState.lastGivenWorkType = giver.def.workType;
				if (giver.def.prioritizeSustains)
				{
					this.pawn.mindState.priorityWork.Set(cell, giver.def.workType);
				}
				return true;
			}
			return false;
		}

		public bool TryTakeOrderedJob(Job job, JobTag tag = JobTag.Misc)
		{
			if (this.debugLog)
			{
				this.DebugLogEvent("TakeOrderedJob " + job);
			}
			job.playerForced = true;
			if (this.curJob != null && this.curJob.JobIsSameAs(job))
			{
				return true;
			}
			this.pawn.stances.CancelBusyStanceSoft();
			this.pawn.Map.pawnDestinationManager.UnreserveAllFor(this.pawn);
			if (job.def == JobDefOf.Goto)
			{
				this.pawn.Map.pawnDestinationManager.ReserveDestinationFor(this.pawn, job.targetA.Cell);
			}
			if (this.debugLog)
			{
				this.DebugLogEvent("    Queueing job");
			}
			this.jobQueue.Clear();
			this.jobQueue.EnqueueFirst(job, new JobTag?(tag));
			if (this.IsCurrentJobPlayerInterruptible())
			{
				if (this.curJob != null)
				{
					this.curDriver.EndJobWith(JobCondition.InterruptForced);
				}
				else
				{
					this.CheckForJobOverride();
				}
			}
			return true;
		}

		public void Notify_TuckedIntoBed(Building_Bed bed)
		{
			this.pawn.Position = RestUtility.GetBedSleepingSlotPosFor(this.pawn, bed);
			this.pawn.Notify_Teleported(false);
			this.pawn.stances.CancelBusyStanceHard();
			this.StartJob(new Job(JobDefOf.LayDown, bed), JobCondition.InterruptForced, null, false, true, null, new JobTag?(JobTag.TuckedIntoBed));
		}

		public void Notify_DamageTaken(DamageInfo dinfo)
		{
			if (this.curJob == null)
			{
				return;
			}
			if (Find.TickManager.TicksGame < this.lastDamageCheckTick + 180)
			{
				return;
			}
			if (dinfo.Def.externalViolence && dinfo.Def.canInterruptJobs && !this.curJob.playerForced)
			{
				Thing instigator = dinfo.Instigator;
				if (this.curJob.def.checkOverrideOnDamage == CheckJobOverrideOnDamageMode.Always || (this.curJob.def.checkOverrideOnDamage == CheckJobOverrideOnDamageMode.OnlyIfInstigatorNotJobTarget && !this.curJob.AnyTargetIs(instigator)))
				{
					this.lastDamageCheckTick = Find.TickManager.TicksGame;
					this.CheckForJobOverride();
				}
			}
		}

		internal void Notify_MasterDrafted()
		{
			this.EndCurrentJob(JobCondition.InterruptForced, true);
		}

		public void DebugLogEvent(string s)
		{
			if (this.debugLog)
			{
				Log.Message(string.Concat(new object[]
				{
					Find.TickManager.TicksGame,
					" ",
					this.pawn,
					": ",
					s
				}));
			}
		}
	}
}
