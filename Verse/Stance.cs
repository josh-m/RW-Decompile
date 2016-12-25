using System;

namespace Verse
{
	public abstract class Stance : IExposable
	{
		public Pawn_StanceTracker stanceTracker;

		public virtual bool StanceBusy
		{
			get
			{
				return false;
			}
		}

		protected Pawn Pawn
		{
			get
			{
				return this.stanceTracker.pawn;
			}
		}

		public virtual void StanceTick()
		{
		}

		public virtual void StanceDraw()
		{
		}

		public virtual void ExposeData()
		{
		}
	}
}
