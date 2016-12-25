using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class ThinkNode_Duty : ThinkNode
	{
		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			if (pawn.GetLord() == null)
			{
				Log.Error(pawn + " doing ThinkNode_Duty with no Lord.");
				return ThinkResult.NoJob;
			}
			if (pawn.mindState.duty == null)
			{
				Log.Error(pawn + " doing ThinkNode_Duty with no duty.");
				return ThinkResult.NoJob;
			}
			return this.subNodes[(int)pawn.mindState.duty.def.index].TryIssueJobPackage(pawn);
		}

		protected override void ResolveSubnodes()
		{
			foreach (DutyDef current in DefDatabase<DutyDef>.AllDefs)
			{
				current.thinkNode.ResolveSubnodesAndRecur();
				this.subNodes.Add(current.thinkNode.DeepCopy(true));
			}
		}
	}
}
