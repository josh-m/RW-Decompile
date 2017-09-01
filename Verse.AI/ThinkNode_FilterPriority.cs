using System;

namespace Verse.AI
{
	public class ThinkNode_FilterPriority : ThinkNode
	{
		public float minPriority = 0.5f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_FilterPriority thinkNode_FilterPriority = (ThinkNode_FilterPriority)base.DeepCopy(resolve);
			thinkNode_FilterPriority.minPriority = this.minPriority;
			return thinkNode_FilterPriority;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			int count = this.subNodes.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.subNodes[i].GetPriority(pawn) > this.minPriority)
				{
					ThinkResult result = this.subNodes[i].TryIssueJobPackage(pawn, jobParams);
					if (result.IsValid)
					{
						return result;
					}
				}
			}
			return ThinkResult.NoJob;
		}
	}
}
