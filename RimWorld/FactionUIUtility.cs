using System;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class FactionUIUtility
	{
		private static bool showAll;

		private const float FactionColorRectSize = 15f;

		private const float FactionColorRectGap = 10f;

		private const float RowMinHeight = 80f;

		private const float LabelRowHeight = 50f;

		private const float TypeColumnWidth = 100f;

		private const float NameColumnWidth = 250f;

		private const float RelationsColumnWidth = 90f;

		private const float NameLeftMargin = 15f;

		public static void DoWindowContents(Rect fillRect, ref Vector2 scrollPosition, ref float scrollViewHeight)
		{
			Rect position = new Rect(0f, 0f, fillRect.width, fillRect.height);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			if (Prefs.DevMode)
			{
				Widgets.CheckboxLabeled(new Rect(position.width - 120f, 0f, 120f, 24f), "Dev: Show all", ref FactionUIUtility.showAll, false, null, null, false);
			}
			else
			{
				FactionUIUtility.showAll = false;
			}
			Rect outRect = new Rect(0f, 50f, position.width, position.height - 50f);
			Rect rect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect, true);
			float num = 0f;
			foreach (Faction current in Find.FactionManager.AllFactionsInViewOrder)
			{
				if ((!current.IsPlayer && !current.def.hidden) || FactionUIUtility.showAll)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.2f);
					Widgets.DrawLineHorizontal(0f, num, rect.width);
					GUI.color = Color.white;
					num += FactionUIUtility.DrawFactionRow(current, num, rect);
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				scrollViewHeight = num;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		private static float DrawFactionRow(Faction faction, float rowY, Rect fillRect)
		{
			Rect rect = new Rect(35f, rowY, 250f, 80f);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Faction current in Find.FactionManager.AllFactionsInViewOrder)
			{
				if (current != faction)
				{
					if ((!current.IsPlayer && !current.def.hidden) || FactionUIUtility.showAll)
					{
						if (faction.HostileTo(current))
						{
							stringBuilder.Append("HostileTo".Translate(current.Name));
							if (FactionUIUtility.showAll)
							{
								if (current.IsPlayer)
								{
									stringBuilder.Append(" (player)");
								}
								else if (current.def.hidden)
								{
									stringBuilder.Append(" (hidden)");
								}
							}
							stringBuilder.AppendLine();
						}
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
				faction.Name.CapitalizeFirst(),
				"\n",
				faction.def.LabelCap,
				"\n",
				(faction.leader == null) ? string.Empty : (faction.def.leaderTitle.CapitalizeFirst() + ": " + faction.leader.Name.ToStringFull)
			});
			Widgets.Label(rect, label);
			Rect rect3 = new Rect(rect.xMax, rowY, 60f, 80f);
			Widgets.InfoCardButton(rect3.x, rect3.y, faction.def);
			Rect rect4 = new Rect(rect3.xMax, rowY, 250f, 80f);
			if (!faction.IsPlayer)
			{
				string text2 = faction.PlayerGoodwill.ToStringWithSign();
				text2 = text2 + "\n" + faction.PlayerRelationKind.GetLabel();
				if (faction.defeated)
				{
					text2 = text2 + "\n(" + "DefeatedLower".Translate() + ")";
				}
				GUI.color = faction.PlayerRelationKind.GetColor();
				Widgets.Label(rect4, text2);
				GUI.color = Color.white;
				string text3 = "CurrentGoodwillTip".Translate();
				if (faction.def.permanentEnemy)
				{
					text3 = text3 + "\n\n" + "CurrentGoodwillTip_PermanentEnemy".Translate();
				}
				else
				{
					text3 += "\n\n";
					FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
					if (playerRelationKind != FactionRelationKind.Ally)
					{
						if (playerRelationKind != FactionRelationKind.Neutral)
						{
							if (playerRelationKind == FactionRelationKind.Hostile)
							{
								text3 += "CurrentGoodwillTip_Hostile".Translate(0.ToString("F0"));
							}
						}
						else
						{
							text3 += "CurrentGoodwillTip_Neutral".Translate(0.ToString("F0"), 75.ToString("F0"));
						}
					}
					else
					{
						text3 += "CurrentGoodwillTip_Ally".Translate(0.ToString("F0"));
					}
					if (faction.def.goodwillDailyGain > 0f || faction.def.goodwillDailyFall > 0f)
					{
						text3 = text3 + "\n\n" + "CurrentGoodwillTip_NaturalGoodwill".Translate(faction.def.naturalColonyGoodwill.min.ToString("F0"), faction.def.naturalColonyGoodwill.max.ToString("F0"), faction.def.goodwillDailyGain.ToString("0.#"), faction.def.goodwillDailyFall.ToString("0.#"));
					}
				}
				TooltipHandler.TipRegion(rect4, text3);
			}
			Rect rect5 = new Rect(rect4.xMax, rowY, width, num);
			Widgets.Label(rect5, text);
			Text.Anchor = TextAnchor.UpperLeft;
			return num2;
		}
	}
}
