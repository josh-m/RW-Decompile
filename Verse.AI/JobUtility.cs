using RimWorld;
using System;

namespace Verse.AI
{
	public static class JobUtility
	{
		private static bool startingErrorRecoverJob;

		public static void TryStartErrorRecoverJob(Pawn pawn, string message, Exception exception = null, JobDriver concreteDriver = null)
		{
			string text = message;
			JobUtility.AppendVarsInfoToDebugMessage(pawn, ref text, concreteDriver);
			if (exception != null)
			{
				text = text + "\n" + exception;
			}
			Log.Error(text, false);
			if (pawn.jobs != null)
			{
				if (pawn.jobs.curJob != null)
				{
					pawn.jobs.EndCurrentJob(JobCondition.Errored, false);
				}
				if (JobUtility.startingErrorRecoverJob)
				{
					Log.Error("An error occurred while starting an error recover job. We have to stop now to avoid infinite loops. This means that the pawn is now jobless which can cause further bugs. pawn=" + pawn.ToStringSafe<Pawn>(), false);
				}
				else
				{
					JobUtility.startingErrorRecoverJob = true;
					try
					{
						pawn.jobs.StartJob(new Job(JobDefOf.Wait, 150, false), JobCondition.None, null, false, true, null, null, false);
					}
					finally
					{
						JobUtility.startingErrorRecoverJob = false;
					}
				}
			}
		}

		private static void AppendVarsInfoToDebugMessage(Pawn pawn, ref string msg, JobDriver concreteDriver)
		{
			if (concreteDriver != null)
			{
				string text = msg;
				msg = string.Concat(new object[]
				{
					text,
					" driver=",
					concreteDriver.GetType().Name,
					" (toilIndex=",
					concreteDriver.CurToilIndex,
					")"
				});
				if (concreteDriver.job != null)
				{
					msg = msg + " driver.job=(" + concreteDriver.job.ToStringSafe<Job>() + ")";
				}
			}
			else if (pawn.jobs != null)
			{
				if (pawn.jobs.curDriver != null)
				{
					string text = msg;
					msg = string.Concat(new object[]
					{
						text,
						" curDriver=",
						pawn.jobs.curDriver.GetType().Name,
						" (toilIndex=",
						pawn.jobs.curDriver.CurToilIndex,
						")"
					});
				}
				if (pawn.jobs.curJob != null)
				{
					msg = msg + " curJob=(" + pawn.jobs.curJob.ToStringSafe<Job>() + ")";
				}
			}
			if (pawn.mindState != null)
			{
				msg = msg + " lastJobGiver=" + pawn.mindState.lastJobGiver.ToStringSafe<ThinkNode>();
			}
		}
	}
}
