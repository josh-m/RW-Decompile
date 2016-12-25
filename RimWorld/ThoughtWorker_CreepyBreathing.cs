using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_CreepyBreathing : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
		{
			if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
			{
				return false;
			}
			if (!other.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
			{
				return false;
			}
			return true;
		}
	}
}
