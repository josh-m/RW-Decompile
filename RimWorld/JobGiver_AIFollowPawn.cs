using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobGiver_AIFollowPawn : ThinkNode_JobGiver
	{
		protected virtual int FollowJobExpireInterval
		{
			get
			{
				return 200;
			}
		}

		protected abstract Pawn GetFollowee(Pawn pawn);

		protected abstract float GetRadius(Pawn pawn);

		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn followee = this.GetFollowee(pawn);
			if (followee == null)
			{
				Log.Error(base.GetType() + "has null followee.");
				return null;
			}
			if (!GenAI.CanInteractPawn(pawn, followee))
			{
				return null;
			}
			float radius = this.GetRadius(pawn);
			if ((followee.pather.Moving && followee.pather.Destination.Cell.DistanceToSquared(pawn.Position) > radius * radius) || followee.Position.GetRoom() != pawn.GetRoom() || followee.Position.DistanceToSquared(pawn.Position) > radius * radius)
			{
				IntVec3 root;
				if (followee.pather.Moving && followee.pather.curPath != null)
				{
					root = followee.pather.curPath.FinalWalkableNonDoorCell();
				}
				else
				{
					root = followee.Position;
				}
				IntVec3 vec = CellFinder.RandomClosewalkCellNear(root, Mathf.RoundToInt(radius * 0.7f));
				Job job = new Job(JobDefOf.Goto, vec);
				job.expiryInterval = this.FollowJobExpireInterval;
				job.checkOverrideOnExpire = true;
				if (pawn.mindState.duty != null && pawn.mindState.duty.locomotion != LocomotionUrgency.None)
				{
					job.locomotionUrgency = pawn.mindState.duty.locomotion;
				}
				return job;
			}
			return null;
		}
	}
}
