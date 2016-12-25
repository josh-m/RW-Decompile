using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_ChemicalInterestVsTeetotaler : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
		{
			if (!p.RaceProps.Humanlike)
			{
				return false;
			}
			if (p.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) <= 0)
			{
				return false;
			}
			if (!other.RaceProps.Humanlike)
			{
				return false;
			}
			if (!RelationsUtility.PawnsKnowEachOther(p, other))
			{
				return false;
			}
			int num = other.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
			if (num >= 0)
			{
				return false;
			}
			return true;
		}
	}
}
