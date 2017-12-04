using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_LoadTransporters : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Transporter);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			CompTransporter transporter = t.TryGetComp<CompTransporter>();
			return LoadTransportersJobUtility.HasJobOnTransporter(pawn, transporter);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			CompTransporter transporter = t.TryGetComp<CompTransporter>();
			return LoadTransportersJobUtility.JobOnTransporter(pawn, transporter);
		}
	}
}
