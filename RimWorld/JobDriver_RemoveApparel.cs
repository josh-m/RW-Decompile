using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RemoveApparel : JobDriver
	{
		private const int DurationTicks = 60;

		private Apparel TargetApparel
		{
			get
			{
				return (Apparel)base.TargetA.Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					this.$this.pawn.pather.StopDead();
				},
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 60
			};
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.$this.pawn.apparel.WornApparel.Contains(this.$this.TargetApparel))
					{
						Apparel apparel;
						if (this.$this.pawn.apparel.TryDrop(this.$this.TargetApparel, out apparel))
						{
							this.$this.job.targetA = apparel;
							if (this.$this.job.haulDroppedApparel)
							{
								apparel.SetForbidden(false, false);
								StoragePriority currentPriority = HaulAIUtility.StoragePriorityAtFor(apparel.Position, apparel);
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
				}
			};
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
