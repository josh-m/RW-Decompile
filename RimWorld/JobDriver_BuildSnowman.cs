using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_BuildSnowman : JobDriver
	{
		private float workLeft = -1000f;

		protected const int BaseWorkAmount = 2300;

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil doWork = new Toil();
			doWork.initAction = delegate
			{
				this.$this.workLeft = 2300f;
			};
			doWork.tickAction = delegate
			{
				this.$this.workLeft -= doWork.actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
				if (this.$this.workLeft <= 0f)
				{
					Thing thing = ThingMaker.MakeThing(ThingDefOf.Snowman, null);
					thing.SetFaction(this.$this.pawn.Faction, null);
					GenSpawn.Spawn(thing, this.$this.TargetLocA, this.$this.Map);
					this.$this.ReadyForNextToil();
					return;
				}
				JoyUtility.JoyTickCheckEnd(this.$this.pawn, JoyTickFullJoyAction.EndJob, 1f);
			};
			doWork.defaultCompleteMode = ToilCompleteMode.Never;
			doWork.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.$this.pawn, null));
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
