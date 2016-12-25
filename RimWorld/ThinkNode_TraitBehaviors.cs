using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_TraitBehaviors : ThinkNode
	{
		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			List<Trait> allTraits = pawn.story.traits.allTraits;
			for (int i = 0; i < allTraits.Count; i++)
			{
				ThinkTreeDef thinkTree = allTraits[i].CurrentData.thinkTree;
				if (thinkTree != null)
				{
					return thinkTree.thinkRoot.TryIssueJobPackage(pawn);
				}
			}
			return ThinkResult.NoJob;
		}
	}
}
