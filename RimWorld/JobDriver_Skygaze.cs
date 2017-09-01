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
			this.gaze.FailOn(() => this.<>f__this.pawn.Position.Roofed(this.<>f__this.pawn.Map));
			this.gaze.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.<>f__this.pawn, null));
			yield return this.gaze;
		}

		public override string GetReport()
		{
			if (base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
			{
				return "WatchingEclipse".Translate();
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
