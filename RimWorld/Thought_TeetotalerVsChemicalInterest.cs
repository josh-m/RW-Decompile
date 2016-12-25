using System;

namespace RimWorld
{
	public class Thought_TeetotalerVsChemicalInterest : Thought_SituationalSocial
	{
		public override float OpinionOffset()
		{
			int num = this.otherPawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
			if (num <= 0)
			{
				return 0f;
			}
			if (num == 1)
			{
				return -20f;
			}
			return -30f;
		}
	}
}
