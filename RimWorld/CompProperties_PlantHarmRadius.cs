using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_PlantHarmRadius : CompProperties
	{
		public SimpleCurve radiusPerDayCurve;

		public float harmFrequencyPerArea = 1f;

		public CompProperties_PlantHarmRadius()
		{
			this.compClass = typeof(CompPlantHarmRadius);
		}
	}
}
