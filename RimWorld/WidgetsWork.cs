using System;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class WidgetsWork
	{
		public const float WorkBoxSize = 25f;

		public static readonly Texture2D WorkBoxBGTex_Awful = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Awful", true);

		public static readonly Texture2D WorkBoxBGTex_Bad = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Bad", true);

		private const int AwfulBGMax = 4;

		public static readonly Texture2D WorkBoxBGTex_Mid = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Mid", true);

		private const int BadBGMax = 14;

		public static readonly Texture2D WorkBoxBGTex_Excellent = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Excellent", true);

		public static readonly Texture2D WorkBoxCheckTex = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxCheck", true);

		public static readonly Texture2D PassionWorkboxMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinorGray", true);

		public static readonly Texture2D PassionWorkboxMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajorGray", true);

		public static readonly Texture2D WorkBoxOverlay_Warning = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxOverlay_Warning", true);

		private const int WarnIfSelectedMax = 2;

		private const float PassionOpacity = 0.4f;

		private static Color ColorOfPriority(int prio)
		{
			switch (prio)
			{
			case 1:
				return new Color(0f, 1f, 0f);
			case 2:
				return new Color(1f, 0.9f, 0.5f);
			case 3:
				return new Color(0.8f, 0.7f, 0.5f);
			case 4:
				return new Color(0.74f, 0.74f, 0.74f);
			default:
				return Color.grey;
			}
		}

		public static void DrawWorkBoxFor(float x, float y, Pawn p, WorkTypeDef wType, bool incapableBecauseOfCapacities)
		{
			if (p.story == null || p.story.WorkTypeIsDisabled(wType))
			{
				return;
			}
			Rect rect = new Rect(x, y, 25f, 25f);
			if (incapableBecauseOfCapacities)
			{
				GUI.color = new Color(1f, 0.3f, 0.3f);
			}
			WidgetsWork.DrawWorkBoxBackground(rect, p, wType);
			GUI.color = Color.white;
			if (Find.PlaySettings.useWorkPriorities)
			{
				int priority = p.workSettings.GetPriority(wType);
				if (priority > 0)
				{
					Text.Anchor = TextAnchor.MiddleCenter;
					GUI.color = WidgetsWork.ColorOfPriority(priority);
					Rect rect2 = rect.ContractedBy(-3f);
					Widgets.Label(rect2, priority.ToStringCached());
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperLeft;
				}
				if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
				{
					bool flag = p.workSettings.WorkIsActive(wType);
					if (Event.current.button == 0)
					{
						int num = p.workSettings.GetPriority(wType) - 1;
						if (num < 0)
						{
							num = 4;
						}
						p.workSettings.SetPriority(wType, num);
						SoundDefOf.AmountIncrement.PlayOneShotOnCamera(null);
					}
					if (Event.current.button == 1)
					{
						int num2 = p.workSettings.GetPriority(wType) + 1;
						if (num2 > 4)
						{
							num2 = 0;
						}
						p.workSettings.SetPriority(wType, num2);
						SoundDefOf.AmountDecrement.PlayOneShotOnCamera(null);
					}
					if (!flag && p.workSettings.WorkIsActive(wType) && wType.relevantSkills.Any<SkillDef>() && p.skills.AverageOfRelevantSkillsFor(wType) <= 2f)
					{
						SoundDefOf.Crunch.PlayOneShotOnCamera(null);
					}
					Event.current.Use();
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorkTab, KnowledgeAmount.SpecificInteraction);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.ManualWorkPriorities, KnowledgeAmount.SmallInteraction);
				}
			}
			else
			{
				int priority2 = p.workSettings.GetPriority(wType);
				if (priority2 > 0)
				{
					GUI.DrawTexture(rect, WidgetsWork.WorkBoxCheckTex);
				}
				if (Widgets.ButtonInvisible(rect, false))
				{
					if (p.workSettings.GetPriority(wType) > 0)
					{
						p.workSettings.SetPriority(wType, 0);
						SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera(null);
					}
					else
					{
						p.workSettings.SetPriority(wType, 3);
						SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera(null);
						if (wType.relevantSkills.Any<SkillDef>() && p.skills.AverageOfRelevantSkillsFor(wType) <= 2f)
						{
							SoundDefOf.Crunch.PlayOneShotOnCamera(null);
						}
					}
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorkTab, KnowledgeAmount.SpecificInteraction);
				}
			}
		}

		public static string TipForPawnWorker(Pawn p, WorkTypeDef wDef, bool incapableBecauseOfCapacities)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(wDef.gerundLabel.CapitalizeFirst());
			if (p.story.WorkTypeIsDisabled(wDef))
			{
				stringBuilder.Append("CannotDoThisWork".Translate(new object[]
				{
					p.NameStringShort
				}));
			}
			else
			{
				float num = p.skills.AverageOfRelevantSkillsFor(wDef);
				if (wDef.relevantSkills.Any<SkillDef>())
				{
					string text = string.Empty;
					foreach (SkillDef current in wDef.relevantSkills)
					{
						text = text + current.skillLabel.CapitalizeFirst() + ", ";
					}
					text = text.Substring(0, text.Length - 2);
					stringBuilder.AppendLine("RelevantSkills".Translate(new object[]
					{
						text,
						num.ToString("0.#"),
						20
					}));
				}
				stringBuilder.AppendLine();
				stringBuilder.Append(wDef.description);
				if (incapableBecauseOfCapacities)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.Append("IncapableOfWorkTypeBecauseOfCapacities".Translate());
				}
				if (wDef.relevantSkills.Any<SkillDef>() && num <= 2f && p.workSettings.WorkIsActive(wDef))
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.Append("SelectedWorkTypeWithVeryBadSkill".Translate());
				}
			}
			return stringBuilder.ToString();
		}

		private static void DrawWorkBoxBackground(Rect rect, Pawn p, WorkTypeDef workDef)
		{
			float num = p.skills.AverageOfRelevantSkillsFor(workDef);
			Texture2D image;
			Texture2D image2;
			float a;
			if (num < 4f)
			{
				image = WidgetsWork.WorkBoxBGTex_Awful;
				image2 = WidgetsWork.WorkBoxBGTex_Bad;
				a = num / 4f;
			}
			else if (num <= 14f)
			{
				image = WidgetsWork.WorkBoxBGTex_Bad;
				image2 = WidgetsWork.WorkBoxBGTex_Mid;
				a = (num - 4f) / 10f;
			}
			else
			{
				image = WidgetsWork.WorkBoxBGTex_Mid;
				image2 = WidgetsWork.WorkBoxBGTex_Excellent;
				a = (num - 14f) / 6f;
			}
			GUI.DrawTexture(rect, image);
			GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, a);
			GUI.DrawTexture(rect, image2);
			if (workDef.relevantSkills.Any<SkillDef>() && num <= 2f && p.workSettings.WorkIsActive(workDef))
			{
				GUI.color = Color.white;
				GUI.DrawTexture(rect.ContractedBy(-2f), WidgetsWork.WorkBoxOverlay_Warning);
			}
			Passion passion = p.skills.MaxPassionOfRelevantSkillsFor(workDef);
			if (passion > Passion.None)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.4f);
				Rect position = rect;
				position.xMin = rect.center.x;
				position.yMin = rect.center.y;
				if (passion == Passion.Minor)
				{
					GUI.DrawTexture(position, WidgetsWork.PassionWorkboxMinorIcon);
				}
				else if (passion == Passion.Major)
				{
					GUI.DrawTexture(position, WidgetsWork.PassionWorkboxMajorIcon);
				}
			}
			GUI.color = Color.white;
		}
	}
}
