using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ResourceReadout
	{
		private const float LineHeightSimple = 24f;

		private const float LineHeightCategorized = 24f;

		private const float DistFromScreenBottom = 200f;

		private readonly List<ThingCategoryDef> RootThingCategories;

		public ResourceReadout()
		{
			this.RootThingCategories = (from cat in DefDatabase<ThingCategoryDef>.AllDefs
			where cat.resourceReadoutRoot
			select cat).ToList<ThingCategoryDef>();
		}

		public void ResourceReadoutOnGUI()
		{
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (Find.MainTabsRoot.OpenTab == MainTabDefOf.Menu)
			{
				return;
			}
			GenUI.DrawTextWinterShadow(new Rect(256f, 512f, -256f, -512f));
			Text.Font = GameFont.Small;
			if (!Prefs.ResourceReadoutCategorized)
			{
				Rect rect = new Rect(7f, 7f, 200f, (float)(UI.screenHeight - 7) - 200f);
				this.DoReadoutSimple(rect);
			}
			else
			{
				Rect rect2 = new Rect(2f, 7f, 150f, (float)(UI.screenHeight - 7) - 200f);
				this.DoReadoutCategorized(rect2);
			}
		}

		private void DoReadoutCategorized(Rect rect)
		{
			Listing_ResourceReadout listing_ResourceReadout = new Listing_ResourceReadout(rect, Find.VisibleMap);
			listing_ResourceReadout.nestIndentWidth = 7f;
			listing_ResourceReadout.lineHeight = 24f;
			listing_ResourceReadout.verticalSpacing = 0f;
			for (int i = 0; i < this.RootThingCategories.Count; i++)
			{
				listing_ResourceReadout.DoCategory(this.RootThingCategories[i].treeNode, 0, 32);
			}
			listing_ResourceReadout.End();
		}

		private void DoReadoutSimple(Rect rect)
		{
			GUI.BeginGroup(rect);
			Text.Anchor = TextAnchor.MiddleLeft;
			float num = 0f;
			foreach (KeyValuePair<ThingDef, int> current in Find.VisibleMap.resourceCounter.AllCountedAmounts)
			{
				if (current.Value > 0 || current.Key.resourceReadoutAlwaysShow)
				{
					Rect rect2 = new Rect(0f, num, 999f, 24f);
					this.DrawResourceSimple(rect2, current.Key);
					num += 24f;
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
		}

		public void DrawResourceSimple(Rect rect, ThingDef thingDef)
		{
			this.DrawIcon(rect.x, rect.y, thingDef);
			rect.y += 2f;
			int count = Find.VisibleMap.resourceCounter.GetCount(thingDef);
			Rect rect2 = new Rect(34f, rect.y, rect.width - 34f, rect.height);
			Widgets.Label(rect2, count.ToStringCached());
		}

		private void DrawIcon(float x, float y, ThingDef thingDef)
		{
			Rect rect = new Rect(x, y, 27f, 27f);
			Color color = GUI.color;
			GUI.color = thingDef.graphicData.color;
			GUI.DrawTexture(rect, thingDef.uiIcon);
			GUI.color = color;
			TooltipHandler.TipRegion(rect, new TipSignal(() => thingDef.LabelCap + ": " + thingDef.description, thingDef.GetHashCode()));
		}
	}
}
