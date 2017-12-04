using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TendPatient : JobDriver
	{
		private bool usesMedicine;

		private const int BaseTendDuration = 600;

		protected Thing MedicineUsed
		{
			get
			{
				return this.job.targetB.Thing;
			}
		}

		protected Pawn Deliveree
		{
			get
			{
				return (Pawn)this.job.targetA.Thing;
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

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.Deliveree, this.job, 1, -1, null) && (!this.usesMedicine || this.pawn.Reserve(this.MedicineUsed, this.job, 1, -1, null));
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(delegate
			{
				if (!WorkGiver_Tend.GoodLayingStatusForTend(this.$this.Deliveree, this.$this.pawn))
				{
					return true;
				}
				if (this.$this.MedicineUsed != null)
				{
					if (this.$this.Deliveree.playerSettings == null)
					{
						return true;
					}
					if (!this.$this.Deliveree.playerSettings.medCare.AllowsMedicine(this.$this.MedicineUsed.def))
					{
						return true;
					}
				}
				return this.$this.pawn == this.$this.Deliveree && (this.$this.pawn.playerSettings == null || !this.$this.pawn.playerSettings.selfTend);
			});
			base.AddEndCondition(delegate
			{
				if (HealthAIUtility.ShouldBeTendedNow(this.$this.Deliveree))
				{
					return JobCondition.Ongoing;
				}
				return JobCondition.Succeeded;
			});
			this.FailOnAggroMentalState(TargetIndex.A);
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
						if (this.$this.MedicineUsed.DestroyedOrNull() && Medicine.GetMedicineCountToFullyHeal(this.$this.Deliveree) > 0)
						{
							Thing thing = HealthAIUtility.FindBestMedicine(this.$this.pawn, this.$this.Deliveree);
							if (thing != null)
							{
								this.$this.job.targetB = thing;
								this.$this.JumpToToil(reserveMedicine);
							}
						}
					}
				};
			}
			yield return Toils_Jump.Jump(gotoToil);
		}
	}
}
