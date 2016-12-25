using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_Usable : CompProperties
	{
		public JobDef useJob;

		public string useLabel;

		public CompProperties_Usable()
		{
			this.compClass = typeof(CompUsable);
		}
	}
}
