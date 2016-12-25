using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_CurrentlyInMentalStatePanic : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			MentalStateDef mentalStateDef = p.MentalStateDef;
			return mentalStateDef != null && mentalStateDef.category == MentalStateCategory.Panic;
		}
	}
}
