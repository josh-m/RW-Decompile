using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class OutfitForcedHandler : IExposable
	{
		private List<Apparel> forcedAps = new List<Apparel>();

		public bool SomethingIsForced
		{
			get
			{
				return this.forcedAps.Count > 0;
			}
		}

		public List<Apparel> ForcedApparel
		{
			get
			{
				return this.forcedAps;
			}
		}

		public void Reset()
		{
			this.forcedAps.Clear();
		}

		public bool AllowedToAutomaticallyDrop(Apparel ap)
		{
			return !this.forcedAps.Contains(ap);
		}

		public void SetForced(Apparel ap, bool forced)
		{
			if (forced)
			{
				if (!this.forcedAps.Contains(ap))
				{
					this.forcedAps.Add(ap);
				}
			}
			else if (this.forcedAps.Contains(ap))
			{
				this.forcedAps.Remove(ap);
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Apparel>(ref this.forcedAps, "forcedAps", LookMode.Reference, new object[0]);
		}

		public bool IsForced(Apparel ap)
		{
			if (ap.Destroyed)
			{
				Log.Error("Apparel was forced while Destroyed: " + ap);
				if (this.forcedAps.Contains(ap))
				{
					this.forcedAps.Remove(ap);
				}
				return false;
			}
			return this.forcedAps.Contains(ap);
		}
	}
}
