using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_Refuelable : CompProperties
	{
		public float fuelConsumptionRate = 1f;

		public float fuelCapacity = 2f;

		public float autoRefuelPercent = 0.3f;

		public float fuelConsumptionPerTickInRain;

		public ThingFilter fuelFilter;

		public bool destroyOnNoFuel;

		public bool consumeFuelOnlyWhenUsed;

		public CompProperties_Refuelable()
		{
			this.compClass = typeof(CompRefuelable);
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			this.fuelFilter.ResolveReferences();
		}
	}
}
