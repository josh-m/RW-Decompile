using System;

namespace Verse.AI
{
	public class ThinkNode_FilterFlagDistance : ThinkNode_Priority
	{
		public float maxDistToSquadFlag = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_FilterFlagDistance thinkNode_FilterFlagDistance = (ThinkNode_FilterFlagDistance)base.DeepCopy(resolve);
			thinkNode_FilterFlagDistance.maxDistToSquadFlag = this.maxDistToSquadFlag;
			return thinkNode_FilterFlagDistance;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			ThinkResult result = base.TryIssueJobPackage(pawn);
			if (this.maxDistToSquadFlag > 0f && result.IsValid)
			{
				IntVec3 cell = result.Job.targetA.Cell;
				float lengthHorizontalSquared = (pawn.Position - cell).LengthHorizontalSquared;
				if (lengthHorizontalSquared > this.maxDistToSquadFlag * this.maxDistToSquadFlag)
				{
					return ThinkResult.NoJob;
				}
			}
			return result;
		}
	}
}
