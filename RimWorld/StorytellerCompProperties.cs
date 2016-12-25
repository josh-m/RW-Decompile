using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RimWorld
{
	public class StorytellerCompProperties
	{
		public Type compClass;

		public float minDaysPassed;

		public StorytellerCompProperties()
		{
		}

		public StorytellerCompProperties(Type compClass)
		{
			this.compClass = compClass;
		}

		[DebuggerHidden]
		public virtual IEnumerable<string> ConfigErrors(StorytellerDef parentDef)
		{
			if (this.compClass == null)
			{
				yield return parentDef.defName + " has StorytellerCompProperties with null compClass.";
			}
		}

		public virtual void ResolveReferences(StorytellerDef parentDef)
		{
		}
	}
}
