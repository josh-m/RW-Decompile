using System;
using Verse;

namespace RimWorld
{
	public abstract class ScenPart_ScatterThings : ScenPart_ThingCount
	{
		protected abstract bool NearPlayerStart
		{
			get;
		}

		public override void GenerateIntoMap()
		{
			new GenStep_ScatterThings
			{
				nearPlayerStart = this.NearPlayerStart,
				thingDef = this.thingDef,
				stuff = this.stuff,
				clusterSize = 4,
				count = this.count,
				spotMustBeStandable = true,
				minSpacing = 5f
			}.Generate();
		}
	}
}
