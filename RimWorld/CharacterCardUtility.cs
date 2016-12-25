using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class CharacterCardUtility
	{
		public const int MainRectsY = 100;

		private const float MainRectsHeight = 450f;

		private const int ConfigRectTitlesHeight = 40;

		private const int MaxNameLength = 12;

		private const int MaxNickLength = 9;

		public static Vector2 PawnCardSize = new Vector2(570f, 470f);

		private static Regex validNameRegex = new Regex("^[a-zA-Z0-9 '\\-]*$");

		public static void DrawCharacterCard(Rect rect, Pawn pawn)
		{
			CharacterCardUtility.DrawCharacterCard(rect, pawn, null);
		}

		public static void DrawCharacterCard(Rect rect, Pawn pawn, Action randomizeCallback)
		{
			GUI.BeginGroup(rect);
			bool flag = randomizeCallback != null;
			Rect rect2 = new Rect(0f, 0f, 300f, 30f);
			NameTriple nameTriple = pawn.Name as NameTriple;
			if (flag && nameTriple != null)
			{
				Rect rect3 = new Rect(rect2);
				rect3.width *= 0.333f;
				Rect rect4 = new Rect(rect2);
				rect4.width *= 0.333f;
				rect4.x += rect4.width;
				Rect rect5 = new Rect(rect2);
				rect5.width *= 0.333f;
				rect5.x += rect4.width * 2f;
				string first = nameTriple.First;
				string nick = nameTriple.Nick;
				string last = nameTriple.Last;
				CharacterCardUtility.DoNameInputRect(rect3, ref first, 12);
				if (nameTriple.Nick == nameTriple.First || nameTriple.Nick == nameTriple.Last)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.5f);
				}
				CharacterCardUtility.DoNameInputRect(rect4, ref nick, 9);
				GUI.color = Color.white;
				CharacterCardUtility.DoNameInputRect(rect5, ref last, 12);
				if (nameTriple.First != first || nameTriple.Nick != nick || nameTriple.Last != last)
				{
					pawn.Name = new NameTriple(first, nick, last);
				}
				TooltipHandler.TipRegion(rect3, "FirstNameDesc".Translate());
				TooltipHandler.TipRegion(rect4, "ShortIdentifierDesc".Translate());
				TooltipHandler.TipRegion(rect5, "LastNameDesc".Translate());
			}
			else
			{
				rect2.width = 999f;
				Text.Font = GameFont.Medium;
				Widgets.Label(rect2, pawn.Name.ToStringFull);
				Text.Font = GameFont.Small;
			}
			if (randomizeCallback != null)
			{
				Rect rect6 = new Rect(rect2.xMax + 6f, 0f, 100f, rect2.height);
				if (Widgets.ButtonText(rect6, "Randomize".Translate(), true, false, true))
				{
					SoundDefOf.TickTiny.PlayOneShotOnCamera();
					randomizeCallback();
				}
				UIHighlighter.HighlightOpportunity(rect6, "RandomizePawn");
			}
			if (!flag && pawn.IsColonist && !pawn.health.Dead)
			{
				Rect rect7 = new Rect(CharacterCardUtility.PawnCardSize.x - 85f, 0f, 30f, 30f);
				TooltipHandler.TipRegion(rect7, new TipSignal("RenameColonist".Translate()));
				if (Widgets.ButtonImage(rect7, TexButton.Rename))
				{
					Find.WindowStack.Add(new Dialog_ChangeNameTriple(pawn));
				}
			}
			if (flag)
			{
				Widgets.InfoCardButton(CharacterCardUtility.PawnCardSize.x - 115f, 0f, pawn);
			}
			string label = pawn.MainDesc(true);
			Rect rect8 = new Rect(0f, 45f, rect.width, 60f);
			Widgets.Label(rect8, label);
			TooltipHandler.TipRegion(rect8, () => pawn.ageTracker.AgeTooltipString, 6873641);
			Rect position = new Rect(0f, 100f, 250f, 450f);
			Rect position2 = new Rect(position.xMax, 100f, 258f, 450f);
			GUI.BeginGroup(position);
			float num = 0f;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, 0f, 200f, 30f), "Backstory".Translate());
			num += 30f;
			Text.Font = GameFont.Small;
			using (IEnumerator enumerator = Enum.GetValues(typeof(BackstorySlot)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BackstorySlot backstorySlot = (BackstorySlot)((byte)enumerator.Current);
					Backstory backstory = pawn.story.GetBackstory(backstorySlot);
					if (backstory != null)
					{
						Rect rect9 = new Rect(0f, num, position.width, 24f);
						if (Mouse.IsOver(rect9))
						{
							Widgets.DrawHighlight(rect9);
						}
						TooltipHandler.TipRegion(rect9, backstory.FullDescriptionFor(pawn));
						Text.Anchor = TextAnchor.MiddleLeft;
						string str = (backstorySlot != BackstorySlot.Adulthood) ? "Childhood".Translate() : "Adulthood".Translate();
						Widgets.Label(rect9, str + ":");
						Text.Anchor = TextAnchor.UpperLeft;
						Rect rect10 = new Rect(rect9);
						rect10.x += 90f;
						rect10.width -= 90f;
						string title = backstory.Title;
						Widgets.Label(rect10, title);
						num += rect9.height + 2f;
					}
				}
			}
			num += 25f;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, num, 200f, 30f), "IncapableOf".Translate());
			num += 30f;
			Text.Font = GameFont.Small;
			StringBuilder stringBuilder = new StringBuilder();
			List<WorkTags> list = pawn.story.DisabledWorkTags.ToList<WorkTags>();
			if (list.Count == 0)
			{
				stringBuilder.Append("(" + "NoneLower".Translate() + "), ");
			}
			else
			{
				bool flag2 = true;
				foreach (WorkTags current in list)
				{
					if (!flag2)
					{
						stringBuilder.Append(current.LabelTranslated().ToLower());
					}
					else
					{
						stringBuilder.Append(current.LabelTranslated());
					}
					stringBuilder.Append(", ");
					flag2 = false;
				}
			}
			string text = stringBuilder.ToString();
			text = text.Substring(0, text.Length - 2);
			Rect rect11 = new Rect(0f, num, position.width, 999f);
			Widgets.Label(rect11, text);
			num += 100f;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, num, 200f, 30f), "Traits".Translate());
			num += 30f;
			Text.Font = GameFont.Small;
			for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
			{
				Trait trait = pawn.story.traits.allTraits[i];
				Rect rect12 = new Rect(0f, num, position.width, 24f);
				if (Mouse.IsOver(rect12))
				{
					Widgets.DrawHighlight(rect12);
				}
				Widgets.Label(rect12, trait.LabelCap);
				num += rect12.height + 2f;
				Trait trLocal = trait;
				TipSignal tip = new TipSignal(() => trLocal.TipString(pawn), (int)num * 37);
				TooltipHandler.TipRegion(rect12, tip);
			}
			GUI.EndGroup();
			GUI.BeginGroup(position2);
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, 0f, 200f, 30f), "Skills".Translate());
			SkillUI.SkillDrawMode mode;
			if (Current.ProgramState == ProgramState.Playing)
			{
				mode = SkillUI.SkillDrawMode.Gameplay;
			}
			else
			{
				mode = SkillUI.SkillDrawMode.Menu;
			}
			SkillUI.DrawSkillsOf(pawn, new Vector2(0f, 35f), mode);
			GUI.EndGroup();
			GUI.EndGroup();
		}

		public static void DoNameInputRect(Rect rect, ref string name, int maxLength)
		{
			string text = Widgets.TextField(rect, name);
			if (text.Length <= maxLength && CharacterCardUtility.validNameRegex.IsMatch(text))
			{
				name = text;
			}
		}
	}
}
