using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class SitePartWorker : SiteCoreOrPartWorkerBase
	{
		public SitePartDef Def
		{
			get
			{
				return (SitePartDef)this.def;
			}
		}

		public virtual void SitePartWorkerTick(Site site)
		{
		}
	}
}
