using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class Listing_TreeThingFilter : Listing_Tree
	{
		private ThingFilter filter;

		private ThingFilter parentFilter;

		private List<SpecialThingFilterDef> hiddenSpecialFilters;

		public Listing_TreeThingFilter(Rect rect, ThingFilter filter, ThingFilter parentFilter) : base(rect)
		{
			this.filter = filter;
			this.parentFilter = parentFilter;
		}

		public void DoCategoryChildren(TreeNode_ThingCategory node, int indentLevel, int openMask, bool isRoot = false)
		{
			if (isRoot)
			{
				foreach (SpecialThingFilterDef current in node.catDef.ParentsSpecialThingFilterDefs)
				{
					if ((this.parentFilter == null || this.parentFilter.Allowed(current)) && !this.IsSpecialFilterHidden(current))
					{
						this.DoSpecialFilter(current, indentLevel);
					}
				}
			}
			foreach (SpecialThingFilterDef current2 in node.catDef.childSpecialFilters)
			{
				if ((this.parentFilter == null || this.parentFilter.Allowed(current2)) && !this.IsSpecialFilterHidden(current2))
				{
					this.DoSpecialFilter(current2, indentLevel);
				}
			}
			foreach (TreeNode_ThingCategory current3 in node.ChildCategoryNodes)
			{
				if (this.parentFilter == null || this.parentFilter.AllowanceStateOf(current3) != MultiCheckboxState.Off)
				{
					this.DoCategory(current3, indentLevel, openMask);
				}
			}
			foreach (ThingDef current4 in from n in node.catDef.childThingDefs
			orderby n.label
			select n)
			{
				if (!current4.menuHidden && (this.parentFilter == null || this.parentFilter.Allows(current4)) && (this.parentFilter == null || !this.parentFilter.IsAlwaysDisallowedDueToSpecialFilters(current4)))
				{
					this.DoThingDef(current4, indentLevel);
				}
			}
		}

		private void DoSpecialFilter(SpecialThingFilterDef sfDef, int nestLevel)
		{
			if (!sfDef.configurable)
			{
				return;
			}
			base.LabelLeft("*" + sfDef.LabelCap, sfDef.description, nestLevel);
			bool flag = this.filter.Allowed(sfDef);
			bool flag2 = flag;
			Widgets.Checkbox(new Vector2(this.LabelWidth, this.curY), ref flag, this.lineHeight, false);
			if (flag != flag2)
			{
				this.filter.SetAllow(sfDef, flag);
			}
			base.EndLine();
		}

		public void DoCategory(TreeNode_ThingCategory node, int indentLevel, int openMask)
		{
			base.OpenCloseWidget(node, indentLevel, openMask);
			base.LabelLeft(node.LabelCap, node.catDef.description, indentLevel);
			MultiCheckboxState multiCheckboxState = this.filter.AllowanceStateOf(node);
			if (Widgets.CheckboxMulti(new Vector2(this.LabelWidth, this.curY), multiCheckboxState, this.lineHeight))
			{
				bool allow = multiCheckboxState == MultiCheckboxState.Off;
				this.filter.SetAllow(node.catDef, allow);
			}
			base.EndLine();
			if (node.IsOpen(openMask))
			{
				this.DoCategoryChildren(node, indentLevel + 1, openMask, false);
			}
		}

		private void DoThingDef(ThingDef tDef, int nestLevel)
		{
			bool flag = tDef.IsStuff && tDef.smallVolume;
			string text = tDef.description;
			if (flag)
			{
				text = text + "\n\n" + "ThisIsSmallVolume".Translate();
			}
			base.LabelLeft(tDef.LabelCap, text, nestLevel);
			if (flag)
			{
				Rect rect = new Rect(this.LabelWidth - 30f, this.curY, 30f, 30f);
				Text.Font = GameFont.Tiny;
				GUI.color = Color.gray;
				Widgets.Label(rect, "x20");
				Text.Font = GameFont.Small;
				GUI.color = Color.white;
			}
			bool flag2 = this.filter.Allows(tDef);
			bool flag3 = flag2;
			Widgets.Checkbox(new Vector2(this.LabelWidth, this.curY), ref flag2, this.lineHeight, false);
			if (flag2 != flag3)
			{
				this.filter.SetAllow(tDef, flag2);
			}
			base.EndLine();
		}

		private void CalculateHiddenSpecialFilters()
		{
			this.hiddenSpecialFilters = new List<SpecialThingFilterDef>();
			if (this.filter.DisplayRootCategory == null)
			{
				return;
			}
			IEnumerable<SpecialThingFilterDef> enumerable = this.filter.DisplayRootCategory.catDef.DescendantSpecialThingFilterDefs.Concat(this.filter.DisplayRootCategory.catDef.ParentsSpecialThingFilterDefs);
			IEnumerable<ThingDef> enumerable2 = this.filter.DisplayRootCategory.catDef.DescendantThingDefs;
			if (this.parentFilter != null)
			{
				enumerable2 = from x in enumerable2
				where this.parentFilter.Allows(x)
				select x;
			}
			foreach (SpecialThingFilterDef current in enumerable)
			{
				bool flag = false;
				foreach (ThingDef current2 in enumerable2)
				{
					if (current.Worker.PotentiallyMatches(current2))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.hiddenSpecialFilters.Add(current);
				}
			}
		}

		private bool IsSpecialFilterHidden(SpecialThingFilterDef filter)
		{
			if (this.hiddenSpecialFilters == null)
			{
				this.CalculateHiddenSpecialFilters();
			}
			for (int i = 0; i < this.hiddenSpecialFilters.Count; i++)
			{
				if (this.hiddenSpecialFilters[i] == filter)
				{
					return true;
				}
			}
			return false;
		}
	}
}
