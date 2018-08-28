using System;
using Verse;

namespace RimWorld.Planet
{
	public class SiteCore : SiteCoreOrPartBase
	{
		public SiteCoreDef def;

		public override SiteCoreOrPartDefBase Def
		{
			get
			{
				return this.def;
			}
		}

		public SiteCore()
		{
		}

		public SiteCore(SiteCoreDef def, SiteCoreOrPartParams parms)
		{
			this.def = def;
			this.parms = parms;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<SiteCoreDef>(ref this.def, "def");
		}
	}
}
