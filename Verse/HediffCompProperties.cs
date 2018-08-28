using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class HediffCompProperties
	{
		[TranslationHandle]
		public Type compClass;

		public virtual void PostLoad()
		{
		}

		[DebuggerHidden]
		public virtual IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			if (this.compClass == null)
			{
				yield return "compClass is null";
			}
			for (int i = 0; i < parentDef.comps.Count; i++)
			{
				if (parentDef.comps[i] != this && parentDef.comps[i].compClass == this.compClass)
				{
					yield return "two comps with same compClass: " + this.compClass;
				}
			}
		}
	}
}
