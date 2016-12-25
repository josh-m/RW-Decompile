using System;

namespace RimWorld
{
	public class StorytellerCompProperties_CategoryIndividualMTBByBiome : StorytellerCompProperties
	{
		public IncidentCategory category;

		public StorytellerCompProperties_CategoryIndividualMTBByBiome()
		{
			this.compClass = typeof(StorytellerComp_CategoryIndividualMTBByBiome);
		}
	}
}
