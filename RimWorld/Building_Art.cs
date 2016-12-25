using System;
using Verse;

namespace RimWorld
{
	public class Building_Art : Building
	{
		public override string GetInspectString()
		{
			string inspectString = base.GetInspectString();
			return inspectString + StatDefOf.Beauty.LabelCap + ": " + StatDefOf.Beauty.ValueToString(this.GetStatValue(StatDefOf.Beauty, true), ToStringNumberSense.Absolute);
		}
	}
}
