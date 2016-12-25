using System;

namespace RimWorld
{
	public class StorytellerCompProperties_CategoryMTB : StorytellerCompProperties
	{
		public float mtbDays;

		public IncidentCategory category;

		public StorytellerCompProperties_CategoryMTB()
		{
			this.compClass = typeof(StorytellerComp_CategoryMTB);
		}
	}
}
