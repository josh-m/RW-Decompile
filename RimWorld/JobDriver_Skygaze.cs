using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Skygaze : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			Toil gaze = new Toil();
			gaze.initAction = delegate
			{
				this.$this.pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
			};
			gaze.tickAction = delegate
			{
				float num = this.$this.pawn.Map.gameConditionManager.AggregateSkyGazeJoyGainFactor(this.$this.pawn.Map);
				Pawn pawn = this.$this.pawn;
				float extraJoyGainFactor = num;
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor, null);
			};
			gaze.defaultCompleteMode = ToilCompleteMode.Delay;
			gaze.defaultDuration = this.job.def.joyDuration;
			gaze.FailOn(() => this.$this.pawn.Position.Roofed(this.$this.pawn.Map));
			gaze.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.$this.pawn, null));
			yield return gaze;
		}

		public override string GetReport()
		{
			if (base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
			{
				return "WatchingEclipse".Translate();
			}
			if (base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Aurora))
			{
				return "WatchingAurora".Translate();
			}
			float num = GenCelestial.CurCelestialSunGlow(base.Map);
			if (num < 0.1f)
			{
				return "Stargazing".Translate();
			}
			if (num >= 0.65f)
			{
				return "CloudWatching".Translate();
			}
			if (GenLocalDate.DayPercent(this.pawn) < 0.5f)
			{
				return "WatchingSunrise".Translate();
			}
			return "WatchingSunset".Translate();
		}
	}
}
