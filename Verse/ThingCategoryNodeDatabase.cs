using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class ThingCategoryNodeDatabase
	{
		public static bool initialized;

		private static TreeNode_ThingCategory rootNode;

		public static IEnumerable<TreeNode_ThingCategory> AllThingCategoryNodes
		{
			get
			{
				return ThingCategoryNodeDatabase.rootNode.ChildCategoryNodesAndThis;
			}
		}

		public static TreeNode_ThingCategory RootNode
		{
			get
			{
				return ThingCategoryNodeDatabase.rootNode;
			}
		}

		public static void Clear()
		{
			ThingCategoryNodeDatabase.rootNode = null;
			ThingCategoryNodeDatabase.initialized = false;
		}

		public static void FinalizeInit()
		{
			ThingCategoryNodeDatabase.rootNode = ThingCategoryDefOf.Root.treeNode;
			foreach (ThingCategoryDef current in DefDatabase<ThingCategoryDef>.AllDefs)
			{
				if (current.parent != null)
				{
					current.parent.childCategories.Add(current);
				}
			}
			ThingCategoryNodeDatabase.SetNestLevelRecursive(ThingCategoryNodeDatabase.rootNode, 0);
			foreach (ThingDef current2 in DefDatabase<ThingDef>.AllDefs)
			{
				if (current2.thingCategories != null)
				{
					foreach (ThingCategoryDef current3 in current2.thingCategories)
					{
						current3.childThingDefs.Add(current2);
					}
				}
			}
			foreach (SpecialThingFilterDef current4 in DefDatabase<SpecialThingFilterDef>.AllDefs)
			{
				current4.parentCategory.childSpecialFilters.Add(current4);
			}
			ThingCategoryNodeDatabase.rootNode.catDef.childCategories[0].treeNode.SetOpen(-1, true);
			ThingCategoryNodeDatabase.initialized = true;
		}

		private static void SetNestLevelRecursive(TreeNode_ThingCategory node, int nestDepth)
		{
			foreach (ThingCategoryDef current in node.catDef.childCategories)
			{
				current.treeNode.nestDepth = nestDepth;
				ThingCategoryNodeDatabase.SetNestLevelRecursive(current.treeNode, nestDepth + 1);
			}
		}
	}
}
