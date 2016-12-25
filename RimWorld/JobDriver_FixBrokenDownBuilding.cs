using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FixBrokenDownBuilding : JobDriver
	{
		private const TargetIndex BuildingIdx = TargetIndex.A;

		private const TargetIndex ComponentsIdx = TargetIndex.B;

		private const int TicksDuration = 1000;

		private Building Building
		{
			get
			{
				return (Building)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Thing Components
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.B).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil repair = new Toil().FailOnDespawnedOrNull(TargetIndex.A);
			repair.defaultDuration = 1000;
			repair.WithEffect(this.Building.def.repairEffect, TargetIndex.A);
			repair.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			repair.defaultCompleteMode = ToilCompleteMode.Delay;
			yield return repair;
			yield return new Toil
			{
				initAction = delegate
				{
					this.<>f__this.Components.Destroy(DestroyMode.Vanish);
					float statValue = this.<>f__this.pawn.GetStatValue(StatDefOf.FixBrokenDownBuildingFailChance, true);
					if (Rand.Value < statValue)
					{
						Vector3 loc = (this.<>f__this.pawn.DrawPos + this.<>f__this.Building.DrawPos) / 2f;
						MoteMaker.ThrowText(loc, "TextMote_FixBrokenDownBuildingFail".Translate(), 3.65f);
					}
					else
					{
						this.<>f__this.Building.GetComp<CompBreakdownable>().Notify_Repaired();
					}
				}
			};
		}
	}
}
