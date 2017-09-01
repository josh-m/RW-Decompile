using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class CompProperties
	{
		public Type compClass = typeof(ThingComp);

		public CompProperties()
		{
		}

		public CompProperties(Type compClass)
		{
			this.compClass = compClass;
		}

		[DebuggerHidden]
		public virtual IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (this.compClass == null)
			{
				yield return parentDef.defName + " has CompProperties with null compClass.";
			}
		}

		public virtual void ResolveReferences(ThingDef parentDef)
		{
		}

		[DebuggerHidden]
		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
		}
	}
}
