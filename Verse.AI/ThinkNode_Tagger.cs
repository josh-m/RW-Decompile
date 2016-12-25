using System;

namespace Verse.AI
{
	public class ThinkNode_Tagger : ThinkNode_Priority
	{
		private JobTag tagToGive;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_Tagger thinkNode_Tagger = (ThinkNode_Tagger)base.DeepCopy(resolve);
			thinkNode_Tagger.tagToGive = this.tagToGive;
			return thinkNode_Tagger;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			ThinkResult result = base.TryIssueJobPackage(pawn);
			if (result.IsValid)
			{
				pawn.mindState.lastJobTag = this.tagToGive;
			}
			return result;
		}
	}
}
