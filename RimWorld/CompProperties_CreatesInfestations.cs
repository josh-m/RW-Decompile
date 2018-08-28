using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_CreatesInfestations : CompProperties
	{
		public CompProperties_CreatesInfestations()
		{
			this.compClass = typeof(CompCreatesInfestations);
		}
	}
}
