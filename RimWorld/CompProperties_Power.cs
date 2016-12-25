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

		public bool startElectricalFires;

		public bool shortCircuitInRain = true;

		public SoundDef soundPowerOn;

		public SoundDef soundPowerOff;

		public SoundDef soundAmbientPowered;

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string err in base.ConfigErrors(parentDef))
			{
				yield return err;
			}
		}
	}
}
