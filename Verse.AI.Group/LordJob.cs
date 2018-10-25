using System;

namespace Verse.AI.Group
{
	public abstract class LordJob : IExposable
	{
		public Lord lord;

		public virtual bool LostImportantReferenceDuringLoading
		{
			get
			{
				return false;
			}
		}

		public virtual bool AllowStartNewGatherings
		{
			get
			{
				return true;
			}
		}

		public virtual bool NeverInRestraints
		{
			get
			{
				return false;
			}
		}

		public virtual bool GuiltyOnDowned
		{
			get
			{
				return false;
			}
		}

		public virtual bool CanBlockHostileVisitors
		{
			get
			{
				return true;
			}
		}

		public virtual bool AddFleeToil
		{
			get
			{
				return true;
			}
		}

		protected Map Map
		{
			get
			{
				return this.lord.lordManager.map;
			}
		}

		public abstract StateGraph CreateGraph();

		public virtual void LordJobTick()
		{
		}

		public virtual void ExposeData()
		{
		}

		public virtual void Cleanup()
		{
		}

		public virtual void Notify_PawnAdded(Pawn p)
		{
		}

		public virtual void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
		}

		public virtual string GetReport()
		{
			return null;
		}

		public virtual bool CanOpenAnyDoor(Pawn p)
		{
			return false;
		}

		public virtual bool ValidateAttackTarget(Pawn searcher, Thing target)
		{
			return true;
		}
	}
}
