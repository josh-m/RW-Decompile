using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class ScenPart_ConfigPage : ScenPart
	{
		[DebuggerHidden]
		public override IEnumerable<Page> GetConfigPages()
		{
			yield return (Page)Activator.CreateInstance(this.def.pageClass);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
		}
	}
}
