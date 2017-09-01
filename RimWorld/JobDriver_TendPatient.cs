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

		private bool usesMedicine;

		protected Thing MedicineUsed
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

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.usesMedicine, "usesMedicine", false, false);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			this.usesMedicine = (this.MedicineUsed != null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(delegate
			{
				if (!WorkGiver_Tend.GoodLayingStatusForTend(this.<>f__this.Deliveree, this.<>f__this.pawn))
				{
					return true;
				}
				if (this.<>f__this.MedicineUsed != null)
				{
					if (this.<>f__this.Deliveree.playerSettings == null)
					{
						return true;
					}
					if (!this.<>f__this.Deliveree.playerSettings.medCare.AllowsMedicine(this.<>f__this.MedicineUsed.def))
					{
						return true;
					}
				}
				return this.<>f__this.pawn == this.<>f__this.Deliveree && (this.<>f__this.pawn.playerSettings == null || !this.<>f__this.pawn.playerSettings.selfTend);
			});
			this.AddEndCondition(delegate
			{
				if (HealthAIUtility.ShouldBeTendedNow(this.<>f__this.Deliveree))
				{
					return JobCondition.Ongoing;
				}
				return JobCondition.Succeeded;
			});
			this.FailOnAggroMentalState(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			Toil reserveMedicine = null;
			if (this.usesMedicine)
			{
				reserveMedicine = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null).FailOnDespawnedNullOrForbidden(TargetIndex.B);
				yield return reserveMedicine;
				yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B);
				yield return Toils_Tend.PickupMedicine(TargetIndex.B, this.Deliveree).FailOnDestroyedOrNull(TargetIndex.B);
				yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveMedicine, TargetIndex.B, TargetIndex.None, true, null);
			}
			PathEndMode interactionCell = (this.Deliveree != this.pawn) ? PathEndMode.InteractionCell : PathEndMode.OnCell;
			Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, interactionCell);
			yield return gotoToil;
			int duration = (int)(1f / this.pawn.GetStatValue(StatDefOf.MedicalTendSpeed, true) * 600f);
			yield return Toils_General.Wait(duration).FailOnCannotTouch(TargetIndex.A, interactionCell).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
			yield return Toils_Tend.FinalizeTend(this.Deliveree);
			if (this.usesMedicine)
			{
				yield return new Toil
				{
					initAction = delegate
					{
						if (this.<>f__this.MedicineUsed.DestroyedOrNull() && Medicine.GetMedicineCountToFullyHeal(this.<>f__this.Deliveree) > 0)
						{
							Thing thing = HealthAIUtility.FindBestMedicine(this.<>f__this.pawn, this.<>f__this.Deliveree);
							if (thing != null)
							{
								this.<>f__this.CurJob.targetB = thing;
								this.<>f__this.JumpToToil(this.<reserveMedicine>__0);
							}
						}
					}
				};
			}
			yield return Toils_Jump.Jump(gotoToil);
		}
	}
}
