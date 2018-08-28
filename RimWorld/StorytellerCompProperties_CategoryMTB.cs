using System;
using Verse;

namespace RimWorld
{
	public class StorytellerCompProperties_CategoryMTB : StorytellerCompProperties
	{
		public float mtbDays = -1f;

		public SimpleCurve mtbDaysFactorByDaysPassedCurve;

		public IncidentCategoryDef category;

		public StorytellerCompProperties_CategoryMTB()
		{
			this.compClass = typeof(StorytellerComp_CategoryMTB);
		}
	}
}
