using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Skygaze : JobDriver
	{
		private Toil gaze;

		public override PawnPosture Posture
		{
			get
			{
				return (base.CurToil != this.gaze) ? PawnPosture.Standing : PawnPosture.LayingFaceUp;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			this.gaze = new Toil();
			this.gaze.tickAction = delegate
			{
				JoyUtility.JoyTickCheckEnd(this.<>f__this.pawn, JoyTickFullJoyAction.EndJob, 1f);
			};
			this.gaze.defaultCompleteMode = ToilCompleteMode.Delay;
			this.gaze.defaultDuration = base.CurJob.def.joyDuration;
			this.gaze.FailOn(() => this.<>f__this.pawn.Position.Roofed());
			this.gaze.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.<>f__this.pawn, null));
			yield return this.gaze;
		}

		public override string GetReport()
		{
			if (Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.Eclipse))
			{
				return "WatchingEclipse".Translate();
			}
			float num = GenCelestial.CurCelestialSunGlow();
			if (num < 0.1f)
			{
				return "Stargazing".Translate();
			}
			if (num >= 0.65f)
			{
				return "CloudWatching".Translate();
			}
			if (GenDate.CurrentDayPercent < 0.5f)
			{
				return "WatchingSunrise".Translate();
			}
			return "WatchingSunset".Translate();
		}
	}
}
