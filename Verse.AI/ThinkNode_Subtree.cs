using System;

namespace Verse.AI
{
	public class ThinkNode_Subtree : ThinkNode
	{
		private ThinkTreeDef treeDef;

		[Unsaved]
		public ThinkNode subtreeNode;

		public override ThinkNode DeepCopy()
		{
			ThinkNode_Subtree thinkNode_Subtree = (ThinkNode_Subtree)base.DeepCopy();
			thinkNode_Subtree.treeDef = this.treeDef;
			thinkNode_Subtree.subtreeNode = thinkNode_Subtree.subNodes[this.subNodes.IndexOf(this.subtreeNode)];
			return thinkNode_Subtree;
		}

		protected override void ResolveSubnodes()
		{
			this.subtreeNode = this.treeDef.thinkRoot.DeepCopy();
			this.subNodes.Add(this.subtreeNode);
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			return this.subtreeNode.TryIssueJobPackage(pawn);
		}
	}
}
