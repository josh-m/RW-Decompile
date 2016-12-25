using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public class ThinkNode_SubtreesByTag : ThinkNode
	{
		public string insertTag;

		[Unsaved]
		private List<ThinkTreeDef> matchedTrees;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_SubtreesByTag thinkNode_SubtreesByTag = (ThinkNode_SubtreesByTag)base.DeepCopy(resolve);
			thinkNode_SubtreesByTag.insertTag = this.insertTag;
			return thinkNode_SubtreesByTag;
		}

		protected override void ResolveSubnodes()
		{
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			if (this.matchedTrees == null)
			{
				this.matchedTrees = new List<ThinkTreeDef>();
				foreach (ThinkTreeDef current in DefDatabase<ThinkTreeDef>.AllDefs)
				{
					if (current.insertTag == this.insertTag)
					{
						this.matchedTrees.Add(current);
					}
				}
				this.matchedTrees = (from tDef in this.matchedTrees
				orderby tDef.insertPriority descending
				select tDef).ToList<ThinkTreeDef>();
			}
			for (int i = 0; i < this.matchedTrees.Count; i++)
			{
				ThinkResult result = this.matchedTrees[i].thinkRoot.TryIssueJobPackage(pawn);
				if (result.IsValid)
				{
					return result;
				}
			}
			return ThinkResult.NoJob;
		}
	}
}
