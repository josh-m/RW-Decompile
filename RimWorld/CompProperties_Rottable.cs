using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_Rottable : CompProperties
	{
		public float daysToRotStart = 2f;

		public bool rotDestroys;

		public float rotDamagePerDay = 40f;

		public float daysToDessicated = 999f;

		public float dessicatedDamagePerDay;

		public bool disableIfHatcher;

		public int TicksToRotStart
		{
			get
			{
				return Mathf.RoundToInt(this.daysToRotStart * 60000f);
			}
		}

		public int TicksToDessicated
		{
			get
			{
				return Mathf.RoundToInt(this.daysToDessicated * 60000f);
			}
		}

		public CompProperties_Rottable()
		{
			this.compClass = typeof(CompRottable);
		}

		public CompProperties_Rottable(float daysToRotStart)
		{
			this.daysToRotStart = daysToRotStart;
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string e in base.ConfigErrors(parentDef))
			{
				yield return e;
			}
			if (parentDef.tickerType != TickerType.Normal && parentDef.tickerType != TickerType.Rare)
			{
				yield return string.Concat(new object[]
				{
					"CompRottable needs tickerType ",
					TickerType.Rare,
					" or ",
					TickerType.Normal,
					", has ",
					parentDef.tickerType
				});
			}
		}
	}
}
