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

		public override bool TryMakePreToilReservations()
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			this.gaze = new Toil();
			this.gaze.tickAction = delegate
			{
				float num = 1f;
				List<GameCondition> activeConditions = this.$this.pawn.Map.gameConditionManager.ActiveConditions;
				for (int i = 0; i < activeConditions.Count; i++)
				{
					num *= activeConditions[i].SkyGazeJoyGainFactor;
				}
				activeConditions = Find.World.gameConditionManager.ActiveConditions;
				for (int j = 0; j < activeConditions.Count; j++)
				{
					num *= activeConditions[j].SkyGazeJoyGainFactor;
				}
				Pawn pawn = this.$this.pawn;
				float extraJoyGainFactor = num;
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
			};
			this.gaze.defaultCompleteMode = ToilCompleteMode.Delay;
			this.gaze.defaultDuration = this.job.def.joyDuration;
			this.gaze.FailOn(() => this.$this.pawn.Position.Roofed(this.$this.pawn.Map));
			this.gaze.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.$this.pawn, null));
			yield return this.gaze;
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
