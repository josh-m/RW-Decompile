using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TendPatient : JobDriver
	{
		private const int BaseTendDuration = 600;

		protected Thing Medicine
		{
			get
			{
				return base.CurJob.targetB.Thing;
			}
		}

		protected Pawn Deliveree
		{
			get
			{
				return (Pawn)base.CurJob.targetA.Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(delegate
			{
				if (!WorkGiver_Tend.GoodLayingStatusForTend(this.<>f__this.Deliveree))
				{
					return true;
				}
				if (this.<>f__this.Medicine != null)
				{
					if (this.<>f__this.Deliveree.playerSettings == null)
					{
						return true;
					}
					if (!this.<>f__this.Deliveree.playerSettings.medCare.AllowsMedicine(this.<>f__this.Medicine.def))
					{
						return true;
					}
				}
				return false;
			});
			this.AddEndCondition(delegate
			{
				if (HealthAIUtility.ShouldBeTendedNow(this.<>f__this.Deliveree))
				{
					return JobCondition.Ongoing;
				}
				return JobCondition.Incompletable;
			});
			this.FailOnAggroMentalState(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			if (base.TargetThingB != null)
			{
				Toil reserveTargetB = Toils_Reserve.Reserve(TargetIndex.B, 1);
				yield return reserveTargetB;
				yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B);
				yield return Toils_Tend.PickupMedicine(TargetIndex.B, this.Deliveree).FailOnDestroyedOrNull(TargetIndex.B);
				yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveTargetB, TargetIndex.B, TargetIndex.None, false, null);
			}
			Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return gotoToil;
			int duration = (int)(1f / this.pawn.GetStatValue(StatDefOf.HealingSpeed, true) * 600f);
			yield return Toils_General.Wait(duration).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
			yield return Toils_Tend.FinalizeTend(this.Deliveree);
			yield return Toils_Jump.Jump(gotoToil);
		}
	}
}
