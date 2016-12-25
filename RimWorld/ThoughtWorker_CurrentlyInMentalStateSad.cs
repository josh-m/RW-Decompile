using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_CurrentlyInMentalStateSad : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			MentalStateDef mentalStateDef = p.MentalStateDef;
			return mentalStateDef != null && mentalStateDef.category == MentalStateCategory.Sad;
		}
	}
}
