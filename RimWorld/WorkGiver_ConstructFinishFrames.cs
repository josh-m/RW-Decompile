using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ConstructFinishFrames : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.BuildingFrame);
			}
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
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
			if (frame.MaterialsNeeded().Count > 0)
			{
				return null;
			}
			if (GenConstruct.FirstBlockingThing(frame, pawn) != null)
			{
				return GenConstruct.HandleBlockingThingJob(frame, pawn, forced);
			}
			if (!GenConstruct.CanConstruct(frame, pawn, forced))
			{
				return null;
			}
			return new Job(JobDefOf.FinishFrame, frame);
		}
	}
}
