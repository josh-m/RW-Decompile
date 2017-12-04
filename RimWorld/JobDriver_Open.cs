using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Open : JobDriver
	{
		public const int OpenTicks = 300;

		private IOpenable Openable
		{
			get
			{
				return (IOpenable)this.job.targetA.Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				initAction = delegate
				{
					if (!this.$this.Openable.CanOpen)
					{
						Designation designation = this.$this.Map.designationManager.DesignationOn(this.$this.job.targetA.Thing, DesignationDefOf.Open);
						if (designation != null)
						{
							designation.Delete();
						}
					}
				}
			}.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.Open).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_General.Wait(300).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).FailOnDespawnedOrNull(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_General.Open(TargetIndex.A);
		}
	}
}
