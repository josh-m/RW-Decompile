using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_LoadTransporters : ThinkNode_JobGiver
	{
		private static List<CompTransporter> tmpTransporters = new List<CompTransporter>();

		protected override Job TryGiveJob(Pawn pawn)
		{
			int transportersGroup = pawn.mindState.duty.transportersGroup;
			TransporterUtility.GetTransportersInGroup(transportersGroup, pawn.Map, JobGiver_LoadTransporters.tmpTransporters);
			for (int i = 0; i < JobGiver_LoadTransporters.tmpTransporters.Count; i++)
			{
				CompTransporter transporter = JobGiver_LoadTransporters.tmpTransporters[i];
				if (LoadTransportersJobUtility.HasJobOnTransporter(pawn, transporter))
				{
					return LoadTransportersJobUtility.JobOnTransporter(pawn, transporter);
				}
			}
			return null;
		}
	}
}
