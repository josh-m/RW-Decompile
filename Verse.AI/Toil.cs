using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public sealed class Toil : IJobEndable
	{
		public Pawn actor;

		public Action initAction;

		public Action tickAction;

		public List<Func<JobCondition>> endConditions = new List<Func<JobCondition>>();

		public List<Action> preInitActions;

		public List<Action> preTickActions;

		public List<Action> finishActions;

		public bool atomicWithPrevious;

		public RandomSocialMode socialMode = RandomSocialMode.Normal;

		public ToilCompleteMode defaultCompleteMode = ToilCompleteMode.Instant;

		public int defaultDuration;

		public void Cleanup()
		{
			if (this.finishActions != null)
			{
				for (int i = 0; i < this.finishActions.Count; i++)
				{
					try
					{
						this.finishActions[i]();
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat(new object[]
						{
							"Pawn ",
							this.actor,
							" threw exception while executing toil's finish action (",
							i,
							"), curJob=",
							this.actor.CurJob,
							": ",
							ex
						}));
					}
				}
			}
		}

		public Pawn GetActor()
		{
			return this.actor;
		}

		public void AddFailCondition(Func<bool> newFailCondition)
		{
			this.endConditions.Add(delegate
			{
				if (newFailCondition())
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
		}

		public void AddEndCondition(Func<JobCondition> newEndCondition)
		{
			this.endConditions.Add(newEndCondition);
		}

		public void AddPreInitAction(Action newAct)
		{
			if (this.preInitActions == null)
			{
				this.preInitActions = new List<Action>();
			}
			this.preInitActions.Add(newAct);
		}

		public void AddPreTickAction(Action newAct)
		{
			if (this.preTickActions == null)
			{
				this.preTickActions = new List<Action>();
			}
			this.preTickActions.Add(newAct);
		}

		public void AddFinishAction(Action newAct)
		{
			if (this.finishActions == null)
			{
				this.finishActions = new List<Action>();
			}
			this.finishActions.Add(newAct);
		}
	}
}
