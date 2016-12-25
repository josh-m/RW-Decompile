using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_UnloadCarriers : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			Pawn pawn2 = t as Pawn;
			return pawn2 != null && pawn2 != pawn && (!pawn2.IsColonist || pawn2.HostFaction != null) && (pawn2.inventory.UnloadEverything && pawn2.inventory.innerContainer.Count != 0 && (pawn2.Faction == pawn.Faction || pawn2.HostFaction == pawn.HostFaction)) && !t.IsForbidden(pawn) && !t.IsBurning() && pawn.CanReserve(t, 1);
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			return new Job(JobDefOf.UnloadInventory, t);
		}
	}
}
