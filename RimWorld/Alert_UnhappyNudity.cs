using System;
using Verse;

namespace RimWorld
{
	public class Alert_UnhappyNudity : Alert_Thought
	{
		protected override ThoughtDef Thought
		{
			get
			{
				return ThoughtDefOf.Naked;
			}
		}

		public Alert_UnhappyNudity()
		{
			this.defaultLabel = "AlertUnhappyNudity".Translate();
			this.explanationKey = "AlertUnhappyNudityDesc";
		}
	}
}
