using System;

namespace Verse
{
	public class HediffCompProperties_Infecter : HediffCompProperties
	{
		public float infectionChance = 0.1f;

		public HediffCompProperties_Infecter()
		{
			this.compClass = typeof(HediffComp_Infecter);
		}
	}
}
