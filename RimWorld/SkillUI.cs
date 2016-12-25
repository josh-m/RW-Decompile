using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class SkillUI
	{
		public enum SkillDrawMode : byte
		{
			Gameplay,
			Menu
		}

		private const float SkillWidth = 240f;

		private const float SkillHeight = 24f;

		private const float SkillYSpacing = 3f;

		private const float LeftEdgeMargin = 6f;

		private const float IncButX = 205f;

		private const float IncButSpacing = 10f;

		private static float levelLabelWidth = -1f;

		private static readonly Color DisabledSkillColor = new Color(1f, 1f, 1f, 0.5f);

		private static Texture2D PassionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor", true);

		private static Texture2D PassionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor", true);

		private static Texture2D SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

		public static void DrawSkillsOf(Pawn p, Vector2 offset, SkillUI.SkillDrawMode mode)
		{
			Text.Font = GameFont.Small;
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				float x = Text.CalcSize(allDefsListForReading[i].skillLabel).x;
				if (x > SkillUI.levelLabelWidth)
				{
					SkillUI.levelLabelWidth = x;
				}
			}
			for (int j = 0; j < p.skills.skills.Count; j++)
			{
				float y = (float)j * 27f + offset.y;
				SkillUI.DrawSkill(p.skills.skills[j], new Vector2(offset.x, y), mode);
			}
		}

		private static void DrawSkill(SkillRecord skill, Vector2 topLeft, SkillUI.SkillDrawMode mode)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, 240f, 24f);
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			GUI.BeginGroup(rect);
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect2 = new Rect(6f, 0f, SkillUI.levelLabelWidth + 6f, rect.height);
			Widgets.Label(rect2, skill.def.skillLabel);
			Rect position = new Rect(rect2.xMax, 0f, 24f, 24f);
			if (skill.passion > Passion.None)
			{
				Texture2D image = (skill.passion != Passion.Major) ? SkillUI.PassionMinorIcon : SkillUI.PassionMajorIcon;
				GUI.DrawTexture(position, image);
			}
			if (!skill.TotallyDisabled)
			{
				Rect rect3 = new Rect(position.xMax, 0f, rect.width - position.xMax, rect.height);
				float fillPercent = Mathf.Max(0.01f, (float)skill.Level / 20f);
				Widgets.FillableBar(rect3, fillPercent, SkillUI.SkillBarFillTex, null, false);
			}
			Rect rect4 = new Rect(position.xMax + 4f, 0f, 999f, rect.height);
			rect4.yMin += 3f;
			string label;
			if (skill.TotallyDisabled)
			{
				GUI.color = SkillUI.DisabledSkillColor;
				label = "-";
			}
			else
			{
				label = skill.Level.ToStringCached();
			}
			GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
			Widgets.Label(rect4, label);
			GenUI.ResetLabelAlign();
			GUI.color = Color.white;
			GUI.EndGroup();
			TooltipHandler.TipRegion(rect, new TipSignal(SkillUI.GetSkillDescription(skill), skill.def.GetHashCode() * 397945));
		}

		private static string GetSkillDescription(SkillRecord sk)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (sk.TotallyDisabled)
			{
				stringBuilder.Append("DisabledLower".Translate().CapitalizeFirst());
			}
			else
			{
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"Level".Translate(),
					" ",
					sk.Level,
					": ",
					sk.LevelDescriptor
				}));
				if (Current.ProgramState == ProgramState.Playing)
				{
					string text = (sk.Level != 20) ? "ProgressToNextLevel".Translate() : "Experience".Translate();
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						text,
						": ",
						sk.xpSinceLastLevel.ToString("F0"),
						" / ",
						sk.XpRequiredForLevelUp
					}));
				}
				stringBuilder.Append("Passion".Translate() + ": ");
				switch (sk.passion)
				{
				case Passion.None:
					stringBuilder.Append("PassionNone".Translate(new object[]
					{
						0.333f.ToStringPercent("F0")
					}));
					break;
				case Passion.Minor:
					stringBuilder.Append("PassionMinor".Translate(new object[]
					{
						1f.ToStringPercent("F0")
					}));
					break;
				case Passion.Major:
					stringBuilder.Append("PassionMajor".Translate(new object[]
					{
						1.5f.ToStringPercent("F0")
					}));
					break;
				}
				if (sk.LearningSaturatedToday)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("LearnedMaxToday".Translate(new object[]
					{
						sk.xpSinceMidnight,
						4000,
						0.2f.ToStringPercent("F0")
					}));
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append(sk.def.description);
			return stringBuilder.ToString();
		}
	}
}
