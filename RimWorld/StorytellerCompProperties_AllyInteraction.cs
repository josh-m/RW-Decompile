using System;

namespace RimWorld
{
	public class StorytellerCompProperties_AllyInteraction : StorytellerCompProperties
	{
		public float baseMtb = 99999f;

		public StorytellerCompProperties_AllyInteraction()
		{
			this.compClass = typeof(StorytellerComp_AllyInteraction);
		}
	}
}
