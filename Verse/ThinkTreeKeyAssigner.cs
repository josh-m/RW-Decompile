using System;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public static class ThinkTreeKeyAssigner
	{
		private static HashSet<int> assignedKeys = new HashSet<int>();

		internal static void Reset()
		{
			ThinkTreeKeyAssigner.assignedKeys.Clear();
		}

		public static void AssignKeys(ThinkNode rootNode, int startHash)
		{
			Rand.PushState();
			Rand.Seed = startHash;
			foreach (ThinkNode current in rootNode.ThisAndChildrenRecursive)
			{
				current.SetUniqueSaveKey(ThinkTreeKeyAssigner.NextUnusedKey());
			}
			Rand.PopState();
		}

		public static void AssignSingleKey(ThinkNode node, int startHash)
		{
			Rand.PushState();
			Rand.Seed = startHash;
			node.SetUniqueSaveKey(ThinkTreeKeyAssigner.NextUnusedKey());
			Rand.PopState();
		}

		private static int NextUnusedKey()
		{
			int num;
			do
			{
				num = Rand.Range(1, 2147483647);
			}
			while (ThinkTreeKeyAssigner.assignedKeys.Contains(num));
			ThinkTreeKeyAssigner.assignedKeys.Add(num);
			return num;
		}
	}
}
