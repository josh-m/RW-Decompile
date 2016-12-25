using System;
using System.Collections.Generic;

namespace Verse
{
	public static class EditTreeNodeDatabase
	{
		private static List<TreeNode_Editor> roots = new List<TreeNode_Editor>();

		public static TreeNode_Editor RootOf(object obj)
		{
			for (int i = 0; i < EditTreeNodeDatabase.roots.Count; i++)
			{
				if (EditTreeNodeDatabase.roots[i].obj == obj)
				{
					return EditTreeNodeDatabase.roots[i];
				}
			}
			TreeNode_Editor treeNode_Editor = TreeNode_Editor.NewRootNode(obj);
			EditTreeNodeDatabase.roots.Add(treeNode_Editor);
			return treeNode_Editor;
		}
	}
}
