using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	internal class WorkGiver_CleanFilth : WorkGiver_Scanner
	{
		private int MinTicksSinceThickened = 600;

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Filth);
			}
		}

		public override int LocalRegionsToScanFirst
		{
			get
			{
				return 4;
			}
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerFilthInHomeArea.FilthInHomeArea;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			if (pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			Filth filth = t as Filth;
			return filth != null && filth.Map.areaManager.Home[filth.Position] && pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, pawn.NormalMaxDanger(), 1) && filth.TicksSinceThickened >= this.MinTicksSinceThickened;
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			Job job = new Job(JobDefOf.Clean);
			job.AddQueuedTarget(TargetIndex.A, t);
			int num = 15;
			Map map = t.Map;
			Room room = t.Position.GetRoom(map);
			for (int i = 0; i < 100; i++)
			{
				IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map) && intVec.GetRoom(map) == room)
				{
					List<Thing> thingList = intVec.GetThingList(map);
					for (int j = 0; j < thingList.Count; j++)
					{
						Thing thing = thingList[j];
						if (this.HasJobOnThing(pawn, thing) && thing != t)
						{
							job.AddQueuedTarget(TargetIndex.A, thing);
						}
					}
					if (job.GetTargetQueue(TargetIndex.A).Count >= num)
					{
						break;
					}
				}
			}
			if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
			{
				job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
			}
			return job;
		}
	}
}
