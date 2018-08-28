using System;

namespace RimWorld
{
	public class StorytellerCompProperties_DeepDrillInfestation : StorytellerCompProperties
	{
		public float baseMtbDaysPerDrill;

		public StorytellerCompProperties_DeepDrillInfestation()
		{
			this.compClass = typeof(StorytellerComp_DeepDrillInfestation);
		}
	}
}
