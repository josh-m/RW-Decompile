using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Strip : JobDriver
	{
		private const int StripTicks = 60;

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnAggroMentalState(TargetIndex.A);
			this.FailOn(() => !StrippableUtility.CanBeStrippedByColony(this.$this.TargetThingA));
			Toil gotoThing = new Toil();
			gotoThing.initAction = delegate
			{
				this.$this.pawn.pather.StartPath(this.$this.TargetThingA, PathEndMode.ClosestTouch);
			};
			gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			gotoThing.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return gotoThing;
			yield return Toils_General.Wait(60).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate
				{
					Thing thing = this.$this.job.targetA.Thing;
					Designation designation = this.$this.Map.designationManager.DesignationOn(thing, DesignationDefOf.Strip);
					if (designation != null)
					{
						designation.Delete();
					}
					IStrippable strippable = thing as IStrippable;
					if (strippable != null)
					{
						strippable.Strip();
					}
					this.$this.pawn.records.Increment(RecordDefOf.BodiesStripped);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		public override object[] TaleParameters()
		{
			Corpse corpse = base.TargetA.Thing as Corpse;
			return new object[]
			{
				this.pawn,
				(corpse == null) ? base.TargetA.Thing : corpse.InnerPawn
			};
		}
	}
}
