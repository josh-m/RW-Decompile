using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RemoveApparel : JobDriver
	{
		private int duration;

		private const TargetIndex ApparelInd = TargetIndex.A;

		private Apparel Apparel
		{
			get
			{
				return (Apparel)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.duration, "duration", 0, false);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			this.duration = (int)(this.Apparel.GetStatValue(StatDefOf.EquipDelay, true) * 60f);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			yield return Toils_General.Wait(this.duration, TargetIndex.None).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return Toils_General.Do(delegate
			{
				if (this.$this.pawn.apparel.WornApparel.Contains(this.$this.Apparel))
				{
					Apparel apparel;
					if (this.$this.pawn.apparel.TryDrop(this.$this.Apparel, out apparel))
					{
						this.$this.job.targetA = apparel;
						if (this.$this.job.haulDroppedApparel)
						{
							apparel.SetForbidden(false, false);
							StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(apparel);
							IntVec3 c;
							if (StoreUtility.TryFindBestBetterStoreCellFor(apparel, this.$this.pawn, this.$this.Map, currentPriority, this.$this.pawn.Faction, out c, true))
							{
								this.$this.job.count = apparel.stackCount;
								this.$this.job.targetB = c;
							}
							else
							{
								this.$this.EndJobWith(JobCondition.Incompletable);
							}
						}
						else
						{
							this.$this.EndJobWith(JobCondition.Succeeded);
						}
					}
					else
					{
						this.$this.EndJobWith(JobCondition.Incompletable);
					}
				}
				else
				{
					this.$this.EndJobWith(JobCondition.Incompletable);
				}
			});
			if (this.job.haulDroppedApparel)
			{
				yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
				yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
				yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false).FailOn(() => !this.$this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
				Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
				yield return carryToCell;
				yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
			}
		}
	}
}
