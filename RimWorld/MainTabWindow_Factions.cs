using System;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_Factions : MainTabWindow
	{
		private const float FactionColorRectSize = 15f;

		private const float FactionColorRectGap = 10f;

		private const float RowMinHeight = 80f;

		private const float LabelRowHeight = 50f;

		private const float TypeColumnWidth = 100f;

		private const float NameColumnWidth = 220f;

		private const float RelationsColumnWidth = 100f;

		private const float NameLeftMargin = 15f;

		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;

		public override void DoWindowContents(Rect fillRect)
		{
			base.DoWindowContents(fillRect);
			Rect position = new Rect(0f, 0f, fillRect.width, fillRect.height);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Rect outRect = new Rect(0f, 50f, position.width, position.height - 50f);
			Rect rect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect);
			float num = 0f;
			foreach (Faction current in Find.FactionManager.AllFactionsInViewOrder)
			{
				if (!current.IsPlayer)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.2f);
					Widgets.DrawLineHorizontal(0f, num, rect.width);
					GUI.color = Color.white;
					num += this.DrawFactionRow(current, num, rect);
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				this.scrollViewHeight = num;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		private float DrawFactionRow(Faction faction, float rowY, Rect fillRect)
		{
			Rect rect = new Rect(35f, rowY, 220f, 80f);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Faction current in Find.FactionManager.AllFactionsVisible)
			{
				if (current != faction && !current.IsPlayer && !current.def.hidden)
				{
					if (faction.HostileTo(current))
					{
						stringBuilder.AppendLine("HostileTo".Translate(new object[]
						{
							current.Name
						}));
					}
				}
			}
			string text = stringBuilder.ToString();
			float width = fillRect.width - rect.xMax;
			float num = Text.CalcHeight(text, width);
			float num2 = Mathf.Max(80f, num);
			Rect position = new Rect(10f, rowY + 10f, 15f, 15f);
			Rect rect2 = new Rect(0f, rowY, fillRect.width, num2);
			if (Mouse.IsOver(rect2))
			{
				GUI.DrawTexture(rect2, TexUI.HighlightTex);
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.DrawRectFast(position, faction.Color, null);
			string label = string.Concat(new string[]
			{
				faction.Name,
				"\n",
				faction.def.LabelCap,
				"\n",
				(faction.leader == null) ? string.Empty : (faction.def.leaderTitle + ": " + faction.leader.Name.ToStringFull)
			});
			Widgets.Label(rect, label);
			Rect rect3 = new Rect(rect.xMax, rowY, 60f, 80f);
			Widgets.InfoCardButton(rect3.x, rect3.y, faction.def);
			Rect rect4 = new Rect(rect3.xMax, rowY, 220f, 80f);
			string text2 = Mathf.RoundToInt(faction.GoodwillWith(Faction.OfPlayer)).ToStringCached();
			if (Faction.OfPlayer.HostileTo(faction))
			{
				text2 = text2 + "\n" + "Hostile".Translate();
			}
			if (faction.defeated)
			{
				text2 = text2 + "\n(" + "DefeatedLower".Translate() + ")";
			}
			if (faction.PlayerGoodwill < 0f)
			{
				GUI.color = Color.red;
			}
			else if (faction.PlayerGoodwill == 0f)
			{
				GUI.color = Color.yellow;
			}
			else
			{
				GUI.color = Color.green;
			}
			Widgets.Label(rect4, text2);
			GUI.color = Color.white;
			TooltipHandler.TipRegion(rect4, "CurrentGoodwill".Translate());
			Rect rect5 = new Rect(rect4.xMax, rowY, width, num);
			Widgets.Label(rect5, text);
			Text.Anchor = TextAnchor.UpperLeft;
			return num2;
		}
	}
}
