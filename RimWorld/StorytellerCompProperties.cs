using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StorytellerCompProperties
	{
		[TranslationHandle]
		public Type compClass;

		public float minDaysPassed;

		public List<IncidentTargetTagDef> allowedTargetTags;

		public List<IncidentTargetTagDef> disallowedTargetTags;

		public float minIncChancePopulationIntentFactor = 0.05f;

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
				yield return "a StorytellerCompProperties has null compClass.";
			}
		}

		public virtual void ResolveReferences(StorytellerDef parentDef)
		{
		}
	}
}
