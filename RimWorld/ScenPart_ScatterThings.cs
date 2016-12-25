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

		public override void GenerateIntoMap(Map map)
		{
			new GenStep_ScatterThings
			{
				nearPlayerStart = this.NearPlayerStart,
				thingDef = this.thingDef,
				stuff = this.stuff,
				count = this.count,
				spotMustBeStandable = true,
				minSpacing = 5f,
				clusterSize = ((this.thingDef.category != ThingCategory.Building) ? 4 : 1)
			}.Generate(map);
		}
	}
}
