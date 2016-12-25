using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_PatientGoToBedRecuperate : WorkGiver
	{
		private static JobGiver_PatientGoToBed jgp = new JobGiver_PatientGoToBed();

		public override Job NonScanJob(Pawn pawn)
		{
			ThinkResult thinkResult = WorkGiver_PatientGoToBedRecuperate.jgp.TryIssueJobPackage(pawn);
			if (thinkResult.IsValid)
			{
				return thinkResult.Job;
			}
			return null;
		}
	}
}
