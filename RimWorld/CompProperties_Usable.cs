using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_Usable : CompProperties
	{
		public JobDef useJob;

		[MustTranslate]
		public string useLabel;

		public int useDuration = 100;

		public CompProperties_Usable()
		{
			this.compClass = typeof(CompUsable);
		}
	}
}
