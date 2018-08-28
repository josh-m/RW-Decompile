using System;
using Verse;

namespace RimWorld
{
	public class QueuedIncident : IExposable
	{
		private FiringIncident firingInc;

		private int fireTick = -1;

		private int retryDurationTicks;

		private bool triedToFire;

		public const int RetryIntervalTicks = 833;

		public int FireTick
		{
			get
			{
				return this.fireTick;
			}
		}

		public FiringIncident FiringIncident
		{
			get
			{
				return this.firingInc;
			}
		}

		public int RetryDurationTicks
		{
			get
			{
				return this.retryDurationTicks;
			}
		}

		public bool TriedToFire
		{
			get
			{
				return this.triedToFire;
			}
		}

		public QueuedIncident()
		{
		}

		public QueuedIncident(FiringIncident firingInc, int fireTick, int retryDurationTicks = 0)
		{
			this.firingInc = firingInc;
			this.fireTick = fireTick;
			this.retryDurationTicks = retryDurationTicks;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<FiringIncident>(ref this.firingInc, "firingInc", new object[0]);
			Scribe_Values.Look<int>(ref this.fireTick, "fireTick", 0, false);
			Scribe_Values.Look<int>(ref this.retryDurationTicks, "retryDurationTicks", 0, false);
			Scribe_Values.Look<bool>(ref this.triedToFire, "triedToFire", false, false);
		}

		public void Notify_TriedToFire()
		{
			this.triedToFire = true;
		}

		public override string ToString()
		{
			return this.fireTick + "->" + this.firingInc.ToString();
		}
	}
}
