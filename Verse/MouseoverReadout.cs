using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class MouseoverReadout
	{
		private const float YInterval = 19f;

		private TerrainDef cachedTerrain;

		private string cachedTerrainString;

		private string[] glowStrings;

		private static readonly Vector2 BotLeft = new Vector2(15f, 65f);

		public MouseoverReadout()
		{
			this.MakePermaCache();
		}

		private void MakePermaCache()
		{
			this.glowStrings = new string[101];
			for (int i = 0; i <= 100; i++)
			{
				this.glowStrings[i] = GlowGrid.PsychGlowAtGlow((float)i / 100f).GetLabel() + " (" + ((float)i / 100f).ToStringPercent() + ")";
			}
		}

		public void MouseoverReadoutOnGUI()
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			if (Find.MainTabsRoot.OpenTab != null)
			{
				return;
			}
			GenUI.DrawTextWinterShadow(new Rect(256f, (float)(UI.screenHeight - 256), -256f, 256f));
			Text.Font = GameFont.Small;
			GUI.color = new Color(1f, 1f, 1f, 0.8f);
			IntVec3 c = UI.MouseCell();
			if (!c.InBounds(Find.VisibleMap))
			{
				return;
			}
			float num = 0f;
			Rect rect;
			if (c.Fogged(Find.VisibleMap))
			{
				rect = new Rect(MouseoverReadout.BotLeft.x, (float)UI.screenHeight - MouseoverReadout.BotLeft.y - num, 999f, 999f);
				Widgets.Label(rect, "Undiscovered".Translate());
				GUI.color = Color.white;
				return;
			}
			rect = new Rect(MouseoverReadout.BotLeft.x, (float)UI.screenHeight - MouseoverReadout.BotLeft.y - num, 999f, 999f);
			int num2 = Mathf.RoundToInt(Find.VisibleMap.glowGrid.GameGlowAt(c) * 100f);
			Widgets.Label(rect, this.glowStrings[num2]);
			num += 19f;
			rect = new Rect(MouseoverReadout.BotLeft.x, (float)UI.screenHeight - MouseoverReadout.BotLeft.y - num, 999f, 999f);
			TerrainDef terrain = c.GetTerrain(Find.VisibleMap);
			if (terrain != this.cachedTerrain)
			{
				string str = ((double)terrain.fertility <= 0.0001) ? string.Empty : (" " + "FertShort".Translate() + " " + terrain.fertility.ToStringPercent());
				this.cachedTerrainString = terrain.LabelCap + ((terrain.passability == Traversability.Impassable) ? null : (" (" + "WalkSpeed".Translate(new object[]
				{
					this.SpeedPercentString((float)terrain.pathCost)
				}) + str + ")"));
				this.cachedTerrain = terrain;
			}
			Widgets.Label(rect, this.cachedTerrainString);
			num += 19f;
			Zone zone = c.GetZone(Find.VisibleMap);
			if (zone != null)
			{
				rect = new Rect(MouseoverReadout.BotLeft.x, (float)UI.screenHeight - MouseoverReadout.BotLeft.y - num, 999f, 999f);
				string label = zone.label;
				Widgets.Label(rect, label);
				num += 19f;
			}
			float depth = Find.VisibleMap.snowGrid.GetDepth(c);
			if (depth > 0.03f)
			{
				rect = new Rect(MouseoverReadout.BotLeft.x, (float)UI.screenHeight - MouseoverReadout.BotLeft.y - num, 999f, 999f);
				SnowCategory snowCategory = SnowUtility.GetSnowCategory(depth);
				string label2 = SnowUtility.GetDescription(snowCategory) + " (" + "WalkSpeed".Translate(new object[]
				{
					this.SpeedPercentString((float)SnowUtility.MovementTicksAddOn(snowCategory))
				}) + ")";
				Widgets.Label(rect, label2);
				num += 19f;
			}
			List<Thing> thingList = c.GetThingList(Find.VisibleMap);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def.category != ThingCategory.Mote)
				{
					rect = new Rect(MouseoverReadout.BotLeft.x, (float)UI.screenHeight - MouseoverReadout.BotLeft.y - num, 999f, 999f);
					string labelMouseover = thing.LabelMouseover;
					Widgets.Label(rect, labelMouseover);
					num += 19f;
				}
			}
			RoofDef roof = c.GetRoof(Find.VisibleMap);
			if (roof != null)
			{
				rect = new Rect(MouseoverReadout.BotLeft.x, (float)UI.screenHeight - MouseoverReadout.BotLeft.y - num, 999f, 999f);
				Widgets.Label(rect, roof.LabelCap);
				num += 19f;
			}
			GUI.color = Color.white;
		}

		private string SpeedPercentString(float extraPathTicks)
		{
			float f = 13f / (extraPathTicks + 13f);
			return f.ToStringPercent();
		}
	}
}
