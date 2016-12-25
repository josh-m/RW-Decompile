using System;
using Verse;

namespace RimWorld
{
	public class QueuedIncident : IExposable
	{
		private FiringIncident firingInc;

		private int fireTick = -1;

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

		public QueuedIncident()
		{
		}

		public QueuedIncident(FiringIncident firingInc, int fireTick)
		{
			this.firingInc = firingInc;
			this.fireTick = fireTick;
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<FiringIncident>(ref this.firingInc, "firingInc", new object[0]);
			Scribe_Values.LookValue<int>(ref this.fireTick, "fireTick", 0, false);
		}

		public override string ToString()
		{
			return this.fireTick + "->" + this.firingInc.ToString();
		}
	}
}
