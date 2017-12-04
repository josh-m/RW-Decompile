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

		private static float levelLabelWidth = -1f;

		private const float SkillWidth = 240f;

		public const float SkillHeight = 24f;

		public const float SkillYSpacing = 3f;

		private const float LeftEdgeMargin = 6f;

		private const float IncButX = 205f;

		private const float IncButSpacing = 10f;

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
				float x = Text.CalcSize(allDefsListForReading[i].skillLabel.CapitalizeFirst()).x;
				if (x > SkillUI.levelLabelWidth)
				{
					SkillUI.levelLabelWidth = x;
				}
			}
			for (int j = 0; j < p.skills.skills.Count; j++)
			{
				float y = (float)j * 27f + offset.y;
				SkillUI.DrawSkill(p.skills.skills[j], new Vector2(offset.x, y), mode, string.Empty);
			}
		}

		public static void DrawSkill(SkillRecord skill, Vector2 topLeft, SkillUI.SkillDrawMode mode, string tooltipPrefix = "")
		{
			SkillUI.DrawSkill(skill, new Rect(topLeft.x, topLeft.y, 240f, 24f), mode, string.Empty);
		}

		public static void DrawSkill(SkillRecord skill, Rect holdingRect, SkillUI.SkillDrawMode mode, string tooltipPrefix = "")
		{
			if (Mouse.IsOver(holdingRect))
			{
				GUI.DrawTexture(holdingRect, TexUI.HighlightTex);
			}
			GUI.BeginGroup(holdingRect);
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect = new Rect(6f, 0f, SkillUI.levelLabelWidth + 6f, holdingRect.height);
			Widgets.Label(rect, skill.def.skillLabel.CapitalizeFirst());
			Rect position = new Rect(rect.xMax, 0f, 24f, 24f);
			if (skill.passion > Passion.None)
			{
				Texture2D image = (skill.passion != Passion.Major) ? SkillUI.PassionMinorIcon : SkillUI.PassionMajorIcon;
				GUI.DrawTexture(position, image);
			}
			if (!skill.TotallyDisabled)
			{
				Rect rect2 = new Rect(position.xMax, 0f, holdingRect.width - position.xMax, holdingRect.height);
				float fillPercent = Mathf.Max(0.01f, (float)skill.Level / 20f);
				Widgets.FillableBar(rect2, fillPercent, SkillUI.SkillBarFillTex, null, false);
			}
			Rect rect3 = new Rect(position.xMax + 4f, 0f, 999f, holdingRect.height);
			rect3.yMin += 3f;
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
			Widgets.Label(rect3, label);
			GenUI.ResetLabelAlign();
			GUI.color = Color.white;
			GUI.EndGroup();
			string text = SkillUI.GetSkillDescription(skill);
			if (tooltipPrefix != string.Empty)
			{
				text = tooltipPrefix + "\n\n" + text;
			}
			TooltipHandler.TipRegion(holdingRect, new TipSignal(text, skill.def.GetHashCode() * 397945));
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
				Passion passion = sk.passion;
				if (passion != Passion.None)
				{
					if (passion != Passion.Minor)
					{
						if (passion == Passion.Major)
						{
							stringBuilder.Append("PassionMajor".Translate(new object[]
							{
								1.5f.ToStringPercent("F0")
							}));
						}
					}
					else
					{
						stringBuilder.Append("PassionMinor".Translate(new object[]
						{
							1f.ToStringPercent("F0")
						}));
					}
				}
				else
				{
					stringBuilder.Append("PassionNone".Translate(new object[]
					{
						0.35f.ToStringPercent("F0")
					}));
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
