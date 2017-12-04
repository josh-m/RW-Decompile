using System;

namespace RimWorld
{
	public class CompProperties_Targetable : CompProperties_UseEffect
	{
		public bool psychicSensitiveTargetsOnly;

		public bool fleshCorpsesOnly;

		public bool nonDessicatedCorpsesOnly;

		public CompProperties_Targetable()
		{
			this.compClass = typeof(CompTargetable);
		}
	}
}
