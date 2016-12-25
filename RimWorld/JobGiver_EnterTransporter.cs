using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_EnterTransporter : ThinkNode_JobGiver
	{
		private static List<CompTransporter> tmpTransporters = new List<CompTransporter>();

		protected override Job TryGiveJob(Pawn pawn)
		{
			int transportersGroup = pawn.mindState.duty.transportersGroup;
			TransporterUtility.GetTransportersInGroup(transportersGroup, pawn.Map, JobGiver_EnterTransporter.tmpTransporters);
			CompTransporter compTransporter = this.FindMyTransporter(JobGiver_EnterTransporter.tmpTransporters, pawn);
			if (compTransporter == null || !pawn.CanReserveAndReach(compTransporter.parent, PathEndMode.Touch, Danger.Deadly, 1))
			{
				return null;
			}
			return new Job(JobDefOf.EnterTransporter, compTransporter.parent);
		}

		private CompTransporter FindMyTransporter(List<CompTransporter> transporters, Pawn me)
		{
			for (int i = 0; i < transporters.Count; i++)
			{
				List<TransferableOneWay> leftToLoad = transporters[i].leftToLoad;
				if (leftToLoad != null)
				{
					for (int j = 0; j < leftToLoad.Count; j++)
					{
						if (leftToLoad[j].AnyThing is Pawn)
						{
							List<Thing> things = leftToLoad[j].things;
							for (int k = 0; k < things.Count; k++)
							{
								if (things[k] == me)
								{
									return transporters[i];
								}
							}
						}
					}
				}
			}
			return null;
		}
	}
}
