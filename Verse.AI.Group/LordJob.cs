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

		protected Map Map
		{
			get
			{
				return this.lord.lordManager.map;
			}
		}

		public abstract StateGraph CreateGraph();

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
	}
}
