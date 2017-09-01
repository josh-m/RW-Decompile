using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_BuildSnowman : JobDriver
	{
		protected const int BaseWorkAmount = 2300;

		private float workLeft = -1000f;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil doWork = new Toil();
			doWork.initAction = delegate
			{
				this.<>f__this.workLeft = 2300f;
			};
			doWork.tickAction = delegate
			{
				this.<>f__this.workLeft -= this.<doWork>__0.actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
				if (this.<>f__this.workLeft <= 0f)
				{
					Thing thing = ThingMaker.MakeThing(ThingDefOf.Snowman, null);
					thing.SetFaction(this.<>f__this.pawn.Faction, null);
					GenSpawn.Spawn(thing, this.<>f__this.TargetLocA, this.<>f__this.Map);
					this.<>f__this.ReadyForNextToil();
					return;
				}
				JoyUtility.JoyTickCheckEnd(this.<>f__this.pawn, JoyTickFullJoyAction.EndJob, 1f);
			};
			doWork.defaultCompleteMode = ToilCompleteMode.Never;
			doWork.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.<>f__this.pawn, null));
			doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return doWork;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
		}
	}
}
