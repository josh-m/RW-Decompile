using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class CompProperties_Hibernatable : CompProperties
	{
		public float startupDays = 15f;

		public CompProperties_Hibernatable()
		{
			this.compClass = typeof(CompHibernatable);
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (parentDef.tickerType != TickerType.Normal)
			{
				yield return string.Concat(new object[]
				{
					"CompHibernatable needs tickerType ",
					TickerType.Normal,
					", has ",
					parentDef.tickerType
				});
			}
		}
	}
}
