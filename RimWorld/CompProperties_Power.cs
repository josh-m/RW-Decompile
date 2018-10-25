using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class CompProperties_Power : CompProperties
	{
		public bool transmitsPower;

		public float basePowerConsumption;

		public bool shortCircuitInRain;

		public SoundDef soundPowerOn;

		public SoundDef soundPowerOff;

		public SoundDef soundAmbientPowered;

		[DebuggerHidden]
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			foreach (StatDrawEntry s in base.SpecialDisplayStats(req))
			{
				yield return s;
			}
			if (this.basePowerConsumption > 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Building, "PowerConsumption".Translate(), this.basePowerConsumption.ToString("F0") + " W", 0, string.Empty);
			}
		}
	}
}
