using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public abstract class JobDriver : IJobEndable, IExposable
	{
		public Pawn pawn;

		private List<Toil> toils = new List<Toil>();

		public List<Func<JobCondition>> globalFailConditions = new List<Func<JobCondition>>();

		public List<Action> globalFinishActions = new List<Action>();

		public bool ended;

		private int curToilIndex = -1;

		private ToilCompleteMode curToilCompleteMode;

		public int ticksLeftThisToil = 99999;

		private bool wantBeginNextToil;

		protected int startTick = -1;

		public TargetIndex rotateToFace = TargetIndex.A;

		public bool layingDown;

		public Building_Bed layingDownBed;

		public bool asleep;

		public float uninstallWorkLeft;

		public int debugTicksSpentThisToil;

		protected Toil CurToil
		{
			get
			{
				if (this.curToilIndex < 0)
				{
					return null;
				}
				if (this.curToilIndex >= this.toils.Count)
				{
					Log.Error(string.Concat(new object[]
					{
						this.pawn,
						" with job ",
						this.pawn.CurJob,
						" tried to get CurToil with curToilIndex=",
						this.curToilIndex,
						" but only has ",
						this.toils.Count,
						" toils."
					}));
					return null;
				}
				return this.toils[this.curToilIndex];
			}
		}

		protected bool HaveCurToil
		{
			get
			{
				return this.curToilIndex >= 0 && this.curToilIndex < this.toils.Count;
			}
		}

		private bool CanStartNextToilInBusyStance
		{
			get
			{
				int num = this.curToilIndex + 1;
				return num < this.toils.Count && this.toils[num].atomicWithPrevious;
			}
		}

		public virtual PawnPosture Posture
		{
			get
			{
				return (!this.layingDown) ? PawnPosture.Standing : PawnPosture.LayingAny;
			}
		}

		public int CurToilIndex
		{
			get
			{
				return this.curToilIndex;
			}
		}

		protected Job CurJob
		{
			get
			{
				return (this.pawn.jobs == null) ? null : this.pawn.jobs.curJob;
			}
		}

		protected LocalTargetInfo TargetA
		{
			get
			{
				return this.pawn.jobs.curJob.targetA;
			}
		}

		protected LocalTargetInfo TargetB
		{
			get
			{
				return this.pawn.jobs.curJob.targetB;
			}
		}

		protected LocalTargetInfo TargetC
		{
			get
			{
				return this.pawn.jobs.curJob.targetC;
			}
		}

		protected Thing TargetThingA
		{
			get
			{
				return this.pawn.jobs.curJob.targetA.Thing;
			}
			set
			{
				this.pawn.jobs.curJob.targetA = value;
			}
		}

		protected Thing TargetThingB
		{
			get
			{
				return this.pawn.jobs.curJob.targetB.Thing;
			}
			set
			{
				this.pawn.jobs.curJob.targetB = value;
			}
		}

		protected IntVec3 TargetLocA
		{
			get
			{
				return this.pawn.jobs.curJob.targetA.Cell;
			}
		}

		protected Map Map
		{
			get
			{
				return this.pawn.Map;
			}
		}

		public virtual string GetReport()
		{
			return this.ReportStringProcessed(this.CurJob.def.reportString);
		}

		protected string ReportStringProcessed(string str)
		{
			Job curJob = this.CurJob;
			if (curJob.targetA.HasThing)
			{
				str = str.Replace("TargetA", curJob.targetA.Thing.LabelShort);
			}
			else
			{
				str = str.Replace("TargetA", "AreaLower".Translate());
			}
			if (curJob.targetB.HasThing)
			{
				str = str.Replace("TargetB", curJob.targetB.Thing.LabelShort);
			}
			else
			{
				str = str.Replace("TargetB", "AreaLower".Translate());
			}
			if (curJob.targetC.HasThing)
			{
				str = str.Replace("TargetC", curJob.targetC.Thing.LabelShort);
			}
			else
			{
				str = str.Replace("TargetC", "AreaLower".Translate());
			}
			return str;
		}

		protected abstract IEnumerable<Toil> MakeNewToils();

		public virtual void ExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.ended, "ended", false, false);
			Scribe_Values.LookValue<int>(ref this.curToilIndex, "curToilIndex", 0, true);
			Scribe_Values.LookValue<int>(ref this.ticksLeftThisToil, "ticksLeftThisToil", 0, false);
			Scribe_Values.LookValue<bool>(ref this.wantBeginNextToil, "wantBeginNextToil", false, false);
			Scribe_Values.LookValue<ToilCompleteMode>(ref this.curToilCompleteMode, "curToilCompleteMode", ToilCompleteMode.Undefined, false);
			Scribe_Values.LookValue<int>(ref this.startTick, "startTick", 0, false);
			Scribe_Values.LookValue<TargetIndex>(ref this.rotateToFace, "rotateToFace", TargetIndex.A, false);
			Scribe_Values.LookValue<bool>(ref this.layingDown, "layingDown", false, false);
			Scribe_References.LookReference<Building_Bed>(ref this.layingDownBed, "layingDownBed", false);
			Scribe_Values.LookValue<bool>(ref this.asleep, "asleep", false, false);
			Scribe_Values.LookValue<float>(ref this.uninstallWorkLeft, "uninstallWorkLeft", 0f, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.SetupToils();
			}
		}

		public void Cleanup(JobCondition condition)
		{
			for (int i = 0; i < this.globalFinishActions.Count; i++)
			{
				try
				{
					this.globalFinishActions[i]();
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Pawn ",
						this.pawn,
						" threw exception while executing a global finish action (",
						i,
						"), jobDriver=",
						base.GetType(),
						": ",
						ex
					}));
				}
			}
			if (this.HaveCurToil)
			{
				this.CurToil.Cleanup();
			}
		}

		internal void SetupToils()
		{
			try
			{
				this.toils.Clear();
				foreach (Toil current in this.MakeNewToils())
				{
					if (current.defaultCompleteMode == ToilCompleteMode.Undefined)
					{
						Log.Error("Toil has undefined complete mode.");
						current.defaultCompleteMode = ToilCompleteMode.Instant;
					}
					current.actor = this.pawn;
					this.toils.Add(current);
				}
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new object[]
				{
					"Exception in SetupToils (pawn=",
					this.pawn,
					", job=",
					this.CurJob,
					"): ",
					ex.ToString()
				}));
				this.EndJobWith(JobCondition.Errored);
			}
		}

		public void DriverTick()
		{
			try
			{
				this.ticksLeftThisToil--;
				this.debugTicksSpentThisToil++;
				if (this.CurToil == null)
				{
					if (!this.pawn.stances.FullBodyBusy || this.CanStartNextToilInBusyStance)
					{
						this.ReadyForNextToil();
					}
				}
				else if (!this.CheckCurrentToilEndOrFail())
				{
					if (this.curToilCompleteMode == ToilCompleteMode.Delay)
					{
						if (this.ticksLeftThisToil <= 0)
						{
							this.ReadyForNextToil();
							return;
						}
					}
					else if (this.curToilCompleteMode == ToilCompleteMode.FinishedBusy && !this.pawn.stances.FullBodyBusy)
					{
						this.ReadyForNextToil();
						return;
					}
					if (this.wantBeginNextToil)
					{
						this.TryActuallyStartNextToil();
					}
					else if (this.curToilCompleteMode == ToilCompleteMode.Instant && this.debugTicksSpentThisToil > 300)
					{
						Log.Error(string.Concat(new object[]
						{
							this.pawn,
							" had to be broken from frozen state. He was doing job ",
							this.CurJob,
							", toilindex=",
							this.curToilIndex
						}));
						this.ReadyForNextToil();
					}
					else
					{
						Job curJob = this.CurJob;
						if (this.CurToil.preTickActions != null)
						{
							Toil curToil = this.CurToil;
							for (int i = 0; i < curToil.preTickActions.Count; i++)
							{
								curToil.preTickActions[i]();
								if (this.CurJob != curJob)
								{
									return;
								}
								if (this.CurToil != curToil || this.wantBeginNextToil)
								{
									return;
								}
							}
						}
						if (this.CurToil.tickAction != null)
						{
							this.CurToil.tickAction();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new object[]
				{
					"Exception in Tick (pawn=",
					this.pawn,
					", job=",
					this.CurJob,
					", CurToil=",
					this.curToilIndex,
					"): ",
					ex.ToString()
				}));
				this.EndJobWith(JobCondition.Errored);
			}
		}

		public void ReadyForNextToil()
		{
			this.wantBeginNextToil = true;
			this.TryActuallyStartNextToil();
		}

		private void TryActuallyStartNextToil()
		{
			if (!this.pawn.Spawned)
			{
				return;
			}
			if (this.pawn.stances.FullBodyBusy && !this.CanStartNextToilInBusyStance)
			{
				return;
			}
			if (this.HaveCurToil)
			{
				this.CurToil.Cleanup();
			}
			this.curToilIndex++;
			this.wantBeginNextToil = false;
			if (!this.HaveCurToil)
			{
				if (this.pawn.stances != null && this.pawn.stances.curStance.StanceBusy)
				{
					Log.ErrorOnce(string.Concat(new object[]
					{
						this.pawn,
						" ended job ",
						this.CurJob,
						" due to running out of toils during a busy stance."
					}), 6453432);
				}
				this.EndJobWith(JobCondition.Succeeded);
				return;
			}
			this.debugTicksSpentThisToil = 0;
			this.ticksLeftThisToil = this.CurToil.defaultDuration;
			this.curToilCompleteMode = this.CurToil.defaultCompleteMode;
			if (!this.CheckCurrentToilEndOrFail())
			{
				if (this.CurToil.preInitActions != null)
				{
					for (int i = 0; i < this.CurToil.preInitActions.Count; i++)
					{
						this.CurToil.preInitActions[i]();
					}
				}
				if (this.CurToil.initAction != null)
				{
					try
					{
						this.CurToil.initAction();
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat(new object[]
						{
							"JobDriver threw exception in initAction. Pawn=",
							this.pawn,
							", Job=",
							this.CurJob,
							", Exception: ",
							ex.ToString()
						}));
						this.EndJobWith(JobCondition.Errored);
						return;
					}
				}
				if (!this.ended && this.curToilCompleteMode == ToilCompleteMode.Instant)
				{
					this.ReadyForNextToil();
				}
			}
		}

		public void EndJobWith(JobCondition condition)
		{
			if (condition == JobCondition.Ongoing)
			{
				Log.Warning("Ending a job with Ongoing as the condition. This makes no sense.");
			}
			if (!this.pawn.Destroyed)
			{
				this.pawn.jobs.EndCurrentJob(condition, true);
			}
		}

		private bool CheckCurrentToilEndOrFail()
		{
			Toil curToil = this.CurToil;
			if (this.globalFailConditions != null)
			{
				for (int i = 0; i < this.globalFailConditions.Count; i++)
				{
					JobCondition jobCondition = this.globalFailConditions[i]();
					if (jobCondition != JobCondition.Ongoing)
					{
						this.EndJobWith(jobCondition);
						return true;
					}
				}
			}
			if (curToil != null && curToil.endConditions != null)
			{
				for (int j = 0; j < curToil.endConditions.Count; j++)
				{
					JobCondition jobCondition2 = curToil.endConditions[j]();
					if (jobCondition2 != JobCondition.Ongoing)
					{
						this.EndJobWith(jobCondition2);
						return true;
					}
				}
			}
			return false;
		}

		public void SetNextToil(Toil to)
		{
			this.curToilIndex = this.toils.IndexOf(to) - 1;
		}

		public void SetCompleteMode(ToilCompleteMode compMode)
		{
			this.curToilCompleteMode = compMode;
		}

		public void JumpToToil(Toil to)
		{
			this.SetNextToil(to);
			this.ReadyForNextToil();
		}

		public void Notify_Starting()
		{
			this.startTick = Find.TickManager.TicksGame;
		}

		public virtual void Notify_PatherArrived()
		{
			if (this.curToilCompleteMode == ToilCompleteMode.PatherArrival)
			{
				this.ReadyForNextToil();
			}
		}

		public virtual void Notify_PatherFailed()
		{
			this.EndJobWith(JobCondition.ErroredPather);
		}

		public virtual void Notify_StanceChanged()
		{
		}

		public Pawn GetActor()
		{
			return this.pawn;
		}

		public void AddEndCondition(Func<JobCondition> newEndCondition)
		{
			this.globalFailConditions.Add(newEndCondition);
		}

		public void AddFailCondition(Func<bool> newFailCondition)
		{
			this.globalFailConditions.Add(delegate
			{
				if (newFailCondition())
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
		}

		public void AddFinishAction(Action newAct)
		{
			this.globalFinishActions.Add(newAct);
		}

		public virtual bool ModifyCarriedThingDrawPos(ref Vector3 drawPos)
		{
			return false;
		}

		public virtual RandomSocialMode DesiredSocialMode()
		{
			if (this.CurToil != null)
			{
				return this.CurToil.socialMode;
			}
			return RandomSocialMode.Normal;
		}

		public virtual bool IsContinuation(Job j)
		{
			return true;
		}
	}
}
