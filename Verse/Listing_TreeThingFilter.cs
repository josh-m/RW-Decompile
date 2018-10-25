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

		private List<ThingDef> forceHiddenDefs;

		private List<SpecialThingFilterDef> tempForceHiddenSpecialFilters;

		private List<ThingDef> suppressSmallVolumeTags;

		public Listing_TreeThingFilter(ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, IEnumerable<SpecialThingFilterDef> forceHiddenFilters, List<ThingDef> suppressSmallVolumeTags)
		{
			this.filter = filter;
			this.parentFilter = parentFilter;
			if (forceHiddenDefs != null)
			{
				this.forceHiddenDefs = forceHiddenDefs.ToList<ThingDef>();
			}
			if (forceHiddenFilters != null)
			{
				this.tempForceHiddenSpecialFilters = forceHiddenFilters.ToList<SpecialThingFilterDef>();
			}
			this.suppressSmallVolumeTags = suppressSmallVolumeTags;
		}

		public void DoCategoryChildren(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map, bool isRoot = false)
		{
			if (isRoot)
			{
				foreach (SpecialThingFilterDef current in node.catDef.ParentsSpecialThingFilterDefs)
				{
					if (this.Visible(current))
					{
						this.DoSpecialFilter(current, indentLevel);
					}
				}
			}
			List<SpecialThingFilterDef> childSpecialFilters = node.catDef.childSpecialFilters;
			for (int i = 0; i < childSpecialFilters.Count; i++)
			{
				if (this.Visible(childSpecialFilters[i]))
				{
					this.DoSpecialFilter(childSpecialFilters[i], indentLevel);
				}
			}
			foreach (TreeNode_ThingCategory current2 in node.ChildCategoryNodes)
			{
				if (this.Visible(current2))
				{
					this.DoCategory(current2, indentLevel, openMask, map);
				}
			}
			foreach (ThingDef current3 in from n in node.catDef.childThingDefs
			orderby n.label
			select n)
			{
				if (this.Visible(current3))
				{
					this.DoThingDef(current3, indentLevel, map);
				}
			}
		}

		private void DoSpecialFilter(SpecialThingFilterDef sfDef, int nestLevel)
		{
			if (!sfDef.configurable)
			{
				return;
			}
			base.LabelLeft("*" + sfDef.LabelCap, sfDef.description, nestLevel, 0f);
			bool flag = this.filter.Allows(sfDef);
			bool flag2 = flag;
			Widgets.Checkbox(new Vector2(this.LabelWidth, this.curY), ref flag, this.lineHeight, false, true, null, null);
			if (flag != flag2)
			{
				this.filter.SetAllow(sfDef, flag);
			}
			base.EndLine();
		}

		public void DoCategory(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map)
		{
			base.OpenCloseWidget(node, indentLevel, openMask);
			base.LabelLeft(node.LabelCap, node.catDef.description, indentLevel, 0f);
			MultiCheckboxState multiCheckboxState = this.AllowanceStateOf(node);
			MultiCheckboxState multiCheckboxState2 = Widgets.CheckboxMulti(new Rect(this.LabelWidth, this.curY, this.lineHeight, this.lineHeight), multiCheckboxState, true);
			if (multiCheckboxState != multiCheckboxState2)
			{
				this.filter.SetAllow(node.catDef, multiCheckboxState2 == MultiCheckboxState.On, this.forceHiddenDefs, this.hiddenSpecialFilters);
			}
			base.EndLine();
			if (node.IsOpen(openMask))
			{
				this.DoCategoryChildren(node, indentLevel + 1, openMask, map, false);
			}
		}

		private void DoThingDef(ThingDef tDef, int nestLevel, Map map)
		{
			bool flag = (this.suppressSmallVolumeTags == null || !this.suppressSmallVolumeTags.Contains(tDef)) && tDef.IsStuff && tDef.smallVolume;
			string text = tDef.DescriptionDetailed;
			if (flag)
			{
				text = text + "\n\n" + "ThisIsSmallVolume".Translate(10.ToStringCached());
			}
			float num = -4f;
			if (flag)
			{
				Rect rect = new Rect(this.LabelWidth - 19f, this.curY, 19f, 20f);
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.UpperRight;
				GUI.color = Color.gray;
				Widgets.Label(rect, "/" + 10.ToStringCached());
				Text.Font = GameFont.Small;
				GenUI.ResetLabelAlign();
				GUI.color = Color.white;
			}
			num -= 19f;
			if (map != null)
			{
				int count = map.resourceCounter.GetCount(tDef);
				if (count > 0)
				{
					string text2 = count.ToStringCached();
					Rect rect2 = new Rect(0f, this.curY, this.LabelWidth + num, 40f);
					Text.Font = GameFont.Tiny;
					Text.Anchor = TextAnchor.UpperRight;
					GUI.color = new Color(0.5f, 0.5f, 0.1f);
					Widgets.Label(rect2, text2);
					num -= Text.CalcSize(text2).x;
					GenUI.ResetLabelAlign();
					Text.Font = GameFont.Small;
					GUI.color = Color.white;
				}
			}
			base.LabelLeft(tDef.LabelCap, text, nestLevel, num);
			bool flag2 = this.filter.Allows(tDef);
			bool flag3 = flag2;
			Widgets.Checkbox(new Vector2(this.LabelWidth, this.curY), ref flag2, this.lineHeight, false, true, null, null);
			if (flag2 != flag3)
			{
				this.filter.SetAllow(tDef, flag2);
			}
			base.EndLine();
		}

		public MultiCheckboxState AllowanceStateOf(TreeNode_ThingCategory cat)
		{
			int num = 0;
			int num2 = 0;
			foreach (ThingDef current in cat.catDef.DescendantThingDefs)
			{
				if (this.Visible(current))
				{
					num++;
					if (this.filter.Allows(current))
					{
						num2++;
					}
				}
			}
			bool flag = false;
			foreach (SpecialThingFilterDef current2 in cat.catDef.DescendantSpecialThingFilterDefs)
			{
				if (this.Visible(current2) && !this.filter.Allows(current2))
				{
					flag = true;
					break;
				}
			}
			if (num2 == 0)
			{
				return MultiCheckboxState.Off;
			}
			if (num == num2 && !flag)
			{
				return MultiCheckboxState.On;
			}
			return MultiCheckboxState.Partial;
		}

		private bool Visible(ThingDef td)
		{
			if (td.menuHidden)
			{
				return false;
			}
			if (this.forceHiddenDefs != null && this.forceHiddenDefs.Contains(td))
			{
				return false;
			}
			if (this.parentFilter != null)
			{
				if (!this.parentFilter.Allows(td))
				{
					return false;
				}
				if (this.parentFilter.IsAlwaysDisallowedDueToSpecialFilters(td))
				{
					return false;
				}
			}
			return true;
		}

		private bool Visible(TreeNode_ThingCategory node)
		{
			return node.catDef.DescendantThingDefs.Any(new Func<ThingDef, bool>(this.Visible));
		}

		private bool Visible(SpecialThingFilterDef filter)
		{
			if (this.parentFilter != null && !this.parentFilter.Allows(filter))
			{
				return false;
			}
			if (this.hiddenSpecialFilters == null)
			{
				this.CalculateHiddenSpecialFilters();
			}
			for (int i = 0; i < this.hiddenSpecialFilters.Count; i++)
			{
				if (this.hiddenSpecialFilters[i] == filter)
				{
					return false;
				}
			}
			return true;
		}

		private void CalculateHiddenSpecialFilters()
		{
			this.hiddenSpecialFilters = new List<SpecialThingFilterDef>();
			if (this.tempForceHiddenSpecialFilters != null)
			{
				this.hiddenSpecialFilters.AddRange(this.tempForceHiddenSpecialFilters);
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
					if (current.Worker.CanEverMatch(current2))
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
	}
}
