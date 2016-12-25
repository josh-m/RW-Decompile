using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public static class TabDrawer
	{
		private const float MaxTabWidth = 200f;

		public const float TabHeight = 32f;

		public const float TabHoriztonalOverlap = 10f;

		private static List<TabRecord> tabList = new List<TabRecord>();

		public static TabRecord DrawTabs(Rect baseRect, IEnumerable<TabRecord> tabsEnum)
		{
			TabDrawer.tabList.Clear();
			foreach (TabRecord current in tabsEnum)
			{
				TabDrawer.tabList.Add(current);
			}
			TabRecord tabRecord = null;
			TabRecord tabRecord2 = (from t in TabDrawer.tabList
			where t.selected
			select t).FirstOrDefault<TabRecord>();
			if (tabRecord2 == null)
			{
				Log.ErrorOnce("Drew tabs without any being selected.", 5509712);
				return TabDrawer.tabList[0];
			}
			float num = baseRect.width + (float)(TabDrawer.tabList.Count - 1) * 10f;
			float tabWidth = num / (float)TabDrawer.tabList.Count;
			if (tabWidth > 200f)
			{
				tabWidth = 200f;
			}
			Rect position = new Rect(baseRect);
			position.y -= 32f;
			position.height = 9999f;
			GUI.BeginGroup(position);
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Small;
			Func<TabRecord, Rect> func = delegate(TabRecord tab)
			{
				int num2 = TabDrawer.tabList.IndexOf(tab);
				float x = (float)num2 * (tabWidth - 10f);
				return new Rect(x, 1f, tabWidth, 32f);
			};
			List<TabRecord> list = TabDrawer.tabList.ListFullCopy<TabRecord>();
			list.Remove(tabRecord2);
			list.Add(tabRecord2);
			TabRecord tabRecord3 = null;
			List<TabRecord> list2 = list.ListFullCopy<TabRecord>();
			list2.Reverse();
			for (int i = 0; i < list2.Count; i++)
			{
				TabRecord tabRecord4 = list2[i];
				Rect rect = func(tabRecord4);
				if (tabRecord3 == null && Mouse.IsOver(rect))
				{
					tabRecord3 = tabRecord4;
				}
				MouseoverSounds.DoRegion(rect, SoundDefOf.MouseoverTab);
				if (Widgets.ButtonInvisible(rect, false))
				{
					tabRecord = tabRecord4;
				}
			}
			foreach (TabRecord current2 in list)
			{
				Rect rect2 = func(current2);
				current2.Draw(rect2);
			}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
			if (tabRecord != null && tabRecord != tabRecord2)
			{
				SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
				if (tabRecord.clickedAction != null)
				{
					tabRecord.clickedAction();
				}
			}
			return tabRecord;
		}
	}
}
