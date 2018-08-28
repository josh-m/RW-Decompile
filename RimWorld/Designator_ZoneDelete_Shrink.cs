using System;
using Verse;

namespace RimWorld
{
	public class Designator_ZoneDelete_Shrink : Designator_ZoneDelete
	{
		public Designator_ZoneDelete_Shrink()
		{
			this.defaultLabel = "DesignatorZoneDeleteSingular".Translate();
			this.defaultDesc = "DesignatorZoneDeleteDesc".Translate();
		}
	}
}
