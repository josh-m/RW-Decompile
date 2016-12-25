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

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				initAction = delegate
				{
					this.<>f__this.pawn.pather.StopDead();
				},
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 60
			};
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.<>f__this.pawn.apparel.WornApparel.Contains(this.<>f__this.TargetApparel))
					{
						Apparel apparel;
						if (this.<>f__this.pawn.apparel.TryDrop(this.<>f__this.TargetApparel, out apparel))
						{
							this.<>f__this.CurJob.targetA = apparel;
							if (this.<>f__this.CurJob.haulDroppedApparel)
							{
								apparel.SetForbidden(false, false);
								StoragePriority currentPriority = HaulAIUtility.StoragePriorityAtFor(apparel.Position, apparel);
								IntVec3 c;
								if (StoreUtility.TryFindBestBetterStoreCellFor(apparel, this.<>f__this.pawn, this.<>f__this.Map, currentPriority, this.<>f__this.pawn.Faction, out c, true))
								{
									this.<>f__this.CurJob.count = apparel.stackCount;
									this.<>f__this.CurJob.targetB = c;
								}
								else
								{
									this.<>f__this.EndJobWith(JobCondition.Incompletable);
								}
							}
							else
							{
								this.<>f__this.EndJobWith(JobCondition.Succeeded);
							}
						}
						else
						{
							this.<>f__this.EndJobWith(JobCondition.Incompletable);
						}
					}
					else
					{
						this.<>f__this.EndJobWith(JobCondition.Incompletable);
					}
				}
			};
			if (base.CurJob.haulDroppedApparel)
			{
				yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
				yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
				yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false).FailOn(() => !this.<>f__this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
				Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
				yield return carryToCell;
				yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
			}
		}
	}
}
