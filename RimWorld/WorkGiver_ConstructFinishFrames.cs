using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ConstructFinishFrames : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.BuildingFrame);
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			if (t.Faction != pawn.Faction)
			{
				return null;
			}
			Frame frame = t as Frame;
			if (frame == null)
			{
				return null;
			}
			if (!GenConstruct.CanConstruct(frame, pawn))
			{
				return null;
			}
			if (frame.MaterialsNeeded().Count > 0)
			{
				return null;
			}
			return new Job(JobDefOf.FinishFrame, frame);
		}
	}
}
