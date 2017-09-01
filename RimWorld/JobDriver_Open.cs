using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Open : JobDriver
	{
		private const int OpenTicks = 300;

		private IOpenable Openable
		{
			get
			{
				return (IOpenable)base.CurJob.targetA.Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				initAction = delegate
				{
					if (!this.<>f__this.Openable.CanOpen)
					{
						Designation designation = this.<>f__this.Map.designationManager.DesignationOn(this.<>f__this.CurJob.targetA.Thing, DesignationDefOf.Open);
						if (designation != null)
						{
							designation.Delete();
						}
					}
				}
			}.FailOnDestroyedOrNull(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.Open).FailOnDestroyedOrNull(TargetIndex.A);
			yield return Toils_General.Wait(300).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).FailOnDestroyedOrNull(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			yield return new Toil
			{
				initAction = delegate
				{
					Thing thing = this.<>f__this.CurJob.targetA.Thing;
					Designation designation = this.<>f__this.Map.designationManager.DesignationOn(thing, DesignationDefOf.Open);
					if (designation != null)
					{
						designation.Delete();
					}
					if (this.<>f__this.Openable.CanOpen)
					{
						this.<>f__this.Openable.Open();
						this.<>f__this.pawn.records.Increment(RecordDefOf.ContainersOpened);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
