using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI.Group
{
	public abstract class LordToil
	{
		public Lord lord;

		public LordToilData data;

		private List<Func<bool>> failConditions = new List<Func<bool>>();

		public AvoidGridMode avoidGridMode = AvoidGridMode.Basic;

		public Map Map
		{
			get
			{
				return this.lord.lordManager.map;
			}
		}

		public virtual IntVec3 FlagLoc
		{
			get
			{
				return IntVec3.Invalid;
			}
		}

		public virtual bool AllowSatisfyLongNeeds
		{
			get
			{
				return true;
			}
		}

		public virtual bool ShouldFail
		{
			get
			{
				for (int i = 0; i < this.failConditions.Count; i++)
				{
					if (this.failConditions[i]())
					{
						return true;
					}
				}
				return false;
			}
		}

		public virtual void Init()
		{
		}

		public abstract void UpdateAllDuties();

		public virtual void LordToilTick()
		{
		}

		public virtual void Cleanup()
		{
		}

		public virtual ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
		{
			return ThinkTreeDutyHook.None;
		}

		public virtual void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
		{
		}

		public virtual void Notify_ReachedDutyLocation(Pawn pawn)
		{
		}

		public virtual void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
		{
		}

		public void AddFailCondition(Func<bool> failCondition)
		{
			this.failConditions.Add(failCondition);
		}

		public override string ToString()
		{
			string text = base.GetType().ToString();
			if (text.Contains('.'))
			{
				text = text.Substring(text.LastIndexOf('.') + 1);
			}
			if (text.Contains('_'))
			{
				text = text.Substring(text.LastIndexOf('_') + 1);
			}
			return text;
		}
	}
}
