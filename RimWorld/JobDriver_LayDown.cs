using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_LayDown : JobDriver
	{
		private const TargetIndex BedOrRestSpotIndex = TargetIndex.A;

		public Building_Bed Bed
		{
			get
			{
				return (Building_Bed)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			bool hasBed = this.pawn.CurJob.GetTarget(TargetIndex.A).HasThing;
			if (hasBed)
			{
				yield return Toils_Reserve.Reserve(TargetIndex.A, this.Bed.SleepingSlotsCount, 0, null);
				yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A, TargetIndex.None);
				yield return Toils_Bed.GotoBed(TargetIndex.A);
			}
			else
			{
				yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			}
			yield return Toils_LayDown.LayDown(TargetIndex.A, hasBed, true, true, true);
		}

		public override string GetReport()
		{
			if (this.asleep)
			{
				return "ReportSleeping".Translate();
			}
			return "ReportResting".Translate();
		}
	}
}
