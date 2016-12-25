using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SocialRelax : JobDriver
	{
		private const TargetIndex GatherSpotParentInd = TargetIndex.A;

		private const TargetIndex ChairOrSpotInd = TargetIndex.B;

		private const TargetIndex OptionalIngestibleInd = TargetIndex.C;

		private Thing GatherSpotParent
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		private bool HasChair
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.B).HasThing;
			}
		}

		private bool HasDrink
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.C).HasThing;
			}
		}

		private IntVec3 ClosestGatherSpotParentCell
		{
			get
			{
				return this.GatherSpotParent.OccupiedRect().ClosestCellTo(this.pawn.Position);
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			if (this.HasChair)
			{
				this.EndOnDespawnedOrNull(TargetIndex.B, JobCondition.Incompletable);
			}
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			if (this.HasDrink)
			{
				this.FailOnDestroyedNullOrForbidden(TargetIndex.C);
				yield return Toils_Reserve.Reserve(TargetIndex.C, 1);
				yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.C);
				yield return Toils_Haul.StartCarryThing(TargetIndex.C);
			}
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			Toil chew = new Toil();
			chew.tickAction = delegate
			{
				this.<>f__this.pawn.Drawer.rotator.FaceCell(this.<>f__this.ClosestGatherSpotParentCell);
				this.<>f__this.pawn.GainComfortFromCellIfPossible();
				JoyUtility.JoyTickCheckEnd(this.<>f__this.pawn, JoyTickFullJoyAction.GoToNextToil, 1f);
			};
			chew.defaultCompleteMode = ToilCompleteMode.Delay;
			chew.defaultDuration = base.CurJob.def.joyDuration;
			chew.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(this.<>f__this.pawn);
			});
			chew.socialMode = RandomSocialMode.SuperActive;
			Toils_Ingest.AddIngestionEffects(chew, this.pawn, TargetIndex.C, TargetIndex.None);
			yield return chew;
			if (this.HasDrink)
			{
				yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.C);
			}
		}

		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos)
		{
			IntVec3 closestGatherSpotParentCell = this.ClosestGatherSpotParentCell;
			return JobDriver_Ingest.ModifyCarriedThingDrawPosWorker(ref drawPos, closestGatherSpotParentCell, this.pawn);
		}
	}
}
