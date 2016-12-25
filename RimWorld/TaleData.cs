using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public abstract class TaleData : IExposable
	{
		public abstract void ExposeData();

		[DebuggerHidden]
		public virtual IEnumerable<Rule> GetRules(string prefix)
		{
			Log.Error(base.GetType() + " cannot do GetRules with a prefix.");
		}

		[DebuggerHidden]
		public virtual IEnumerable<Rule> GetRules()
		{
			Log.Error(base.GetType() + " cannot do GetRules without a prefix.");
		}
	}
}
