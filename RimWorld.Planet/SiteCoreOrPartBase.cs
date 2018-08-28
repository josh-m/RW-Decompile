using System;
using Verse;

namespace RimWorld.Planet
{
	public abstract class SiteCoreOrPartBase : IExposable
	{
		public SiteCoreOrPartParams parms;

		public abstract SiteCoreOrPartDefBase Def
		{
			get;
		}

		public virtual void ExposeData()
		{
			Scribe_Deep.Look<SiteCoreOrPartParams>(ref this.parms, "parms", new object[0]);
		}
	}
}
