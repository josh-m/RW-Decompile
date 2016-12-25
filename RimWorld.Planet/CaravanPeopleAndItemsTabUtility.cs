using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class CaravanPeopleAndItemsTabUtility
	{
		private const float PawnRowHeight = 50f;

		private const float NonPawnRowHeight = 30f;

		private const float PawnLabelHeight = 18f;

		private const float PawnLabelColumnWidth = 100f;

		private const float NonPawnLabelColumnWidth = 300f;

		private const float SpaceAroundIcon = 4f;

		private const float NeedWidth = 100f;

		private const float NeedMargin = 10f;

		public const float SpecificTabButtonSize = 24f;

		public const float AbandonButtonSize = 24f;

		private const float AbandonSpecificCountButtonSize = 24f;

		public static readonly Texture2D AbandonButtonTex = ContentFinder<Texture2D>.Get("UI/Buttons/Abandon", true);

		public static readonly Texture2D AbandonSpecificCountButtonTex = ContentFinder<Texture2D>.Get("UI/Buttons/AbandonSpecificCount", true);

		public static readonly Texture2D SpecificTabButtonTex = ContentFinder<Texture2D>.Get("UI/Buttons/OpenSpecificTab", true);

		public static readonly Color OpenedSpecificTabButtonColor = new Color(0f, 0.8f, 0f);

		public static readonly Color OpenedSpecificTabButtonMouseoverColor = new Color(0f, 0.5f, 0f);

		private static List<Need> tmpNeeds = new List<Need>();

		private static List<Thought> thoughtGroupsPresent = new List<Thought>();

		public static void DoRows(Vector2 size, List<Thing> things, Caravan caravan, ref Vector2 scrollPosition, ref float scrollViewHeight, bool alwaysShowItemsSection, ref Pawn specificNeedsTabForPawn, bool doNeeds = true)
		{
			if (specificNeedsTabForPawn != null && (!things.Contains(specificNeedsTabForPawn) || specificNeedsTabForPawn.Dead))
			{
				specificNeedsTabForPawn = null;
			}
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
			bool listingUsesAbandonSpecificCountButtons = CaravanPeopleAndItemsTabUtility.AnyItemOrEmpty(things);
			Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
			float num = 0f;
			bool flag = false;
			for (int i = 0; i < things.Count; i++)
			{
				Pawn pawn = things[i] as Pawn;
				if (pawn != null && pawn.IsColonist)
				{
					if (!flag)
					{
						Widgets.ListSeparator(ref num, viewRect.width, "CaravanColonists".Translate());
						flag = true;
					}
					CaravanPeopleAndItemsTabUtility.DoRow(ref num, viewRect, rect, scrollPosition, pawn, caravan, ref specificNeedsTabForPawn, doNeeds, listingUsesAbandonSpecificCountButtons);
				}
			}
			bool flag2 = false;
			for (int j = 0; j < things.Count; j++)
			{
				Pawn pawn2 = things[j] as Pawn;
				if (pawn2 != null && !pawn2.IsColonist)
				{
					if (!flag2)
					{
						Widgets.ListSeparator(ref num, viewRect.width, "CaravanPrisonersAndAnimals".Translate());
						flag2 = true;
					}
					CaravanPeopleAndItemsTabUtility.DoRow(ref num, viewRect, rect, scrollPosition, pawn2, caravan, ref specificNeedsTabForPawn, doNeeds, listingUsesAbandonSpecificCountButtons);
				}
			}
			bool flag3 = false;
			if (alwaysShowItemsSection)
			{
				Widgets.ListSeparator(ref num, viewRect.width, "CaravanItems".Translate());
			}
			for (int k = 0; k < things.Count; k++)
			{
				if (!(things[k] is Pawn))
				{
					if (!flag3)
					{
						if (!alwaysShowItemsSection)
						{
							Widgets.ListSeparator(ref num, viewRect.width, "CaravanItems".Translate());
						}
						flag3 = true;
					}
					CaravanPeopleAndItemsTabUtility.DoRow(ref num, viewRect, rect, scrollPosition, things[k], caravan, ref specificNeedsTabForPawn, doNeeds, listingUsesAbandonSpecificCountButtons);
				}
			}
			if (alwaysShowItemsSection && !flag3)
			{
				GUI.color = Color.gray;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(new Rect(0f, num, viewRect.width, 25f), "NoneBrackets".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				num += 25f;
				GUI.color = Color.white;
			}
			if (Event.current.type == EventType.Layout)
			{
				scrollViewHeight = num + 30f;
			}
			Widgets.EndScrollView();
		}

		public static Vector2 GetSize(List<Thing> things, float paneTopY, bool doNeeds = true)
		{
			float num = 0f;
			if (things.Any((Thing x) => x is Pawn))
			{
				num = 100f;
				if (doNeeds)
				{
					num += (float)CaravanPeopleAndItemsTabUtility.MaxNeedsCount(things) * 100f;
				}
			}
			float num2 = 0f;
			if (CaravanPeopleAndItemsTabUtility.AnyItemOrEmpty(things))
			{
				num2 = 300f;
				num2 += 24f;
			}
			Vector2 result;
			result.x = 127f + Mathf.Max(num, num2) + 16f;
			result.y = Mathf.Min(550f, paneTopY - 30f);
			return result;
		}

		private static bool AnyItemOrEmpty(List<Thing> things)
		{
			return things.Any((Thing x) => !(x is Pawn)) || !things.Any<Thing>();
		}

		public static void DoAbandonButton(Rect rowRect, Thing t, Caravan caravan)
		{
			Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
			if (Widgets.ButtonImage(rect, CaravanPeopleAndItemsTabUtility.AbandonButtonTex))
			{
				CaravanPawnsAndItemsAbandonUtility.TryAbandonViaInterface(t, caravan);
			}
			TooltipHandler.TipRegion(rect, () => CaravanPawnsAndItemsAbandonUtility.GetAbandonButtonTooltip(t, caravan, false), Gen.HashCombineInt(t.GetHashCode(), 1383004931));
		}

		private static void DoAbandonSpecificCountButton(Rect rowRect, Thing t, Caravan caravan)
		{
			Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
			if (Widgets.ButtonImage(rect, CaravanPeopleAndItemsTabUtility.AbandonSpecificCountButtonTex))
			{
				CaravanPawnsAndItemsAbandonUtility.TryAbandonSpecificCountViaInterface(t, caravan);
			}
			TooltipHandler.TipRegion(rect, () => CaravanPawnsAndItemsAbandonUtility.GetAbandonButtonTooltip(t, caravan, true), Gen.HashCombineInt(t.GetHashCode(), 1163428609));
		}

		public static void DoOpenSpecificTabButton(Rect rowRect, Pawn p, ref Pawn specificTabForPawn)
		{
			Color baseColor = (p != specificTabForPawn) ? Color.white : CaravanPeopleAndItemsTabUtility.OpenedSpecificTabButtonColor;
			Color mouseoverColor = (p != specificTabForPawn) ? GenUI.MouseoverColor : CaravanPeopleAndItemsTabUtility.OpenedSpecificTabButtonMouseoverColor;
			Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
			if (Widgets.ButtonImage(rect, CaravanPeopleAndItemsTabUtility.SpecificTabButtonTex, baseColor, mouseoverColor))
			{
				if (p == specificTabForPawn)
				{
					specificTabForPawn = null;
					SoundDefOf.TabClose.PlayOneShotOnCamera();
				}
				else
				{
					specificTabForPawn = p;
					SoundDefOf.TabOpen.PlayOneShotOnCamera();
				}
			}
			TooltipHandler.TipRegion(rect, "OpenSpecificTabButtonTip".Translate());
			GUI.color = Color.white;
		}

		private static int MaxNeedsCount(List<Thing> things)
		{
			int num = 0;
			for (int i = 0; i < things.Count; i++)
			{
				Pawn pawn = things[i] as Pawn;
				if (pawn != null)
				{
					CaravanPeopleAndItemsTabUtility.GetNeedsToDisplay(pawn, CaravanPeopleAndItemsTabUtility.tmpNeeds);
					num = Mathf.Max(num, CaravanPeopleAndItemsTabUtility.tmpNeeds.Count);
				}
			}
			return num;
		}

		private static void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, Vector2 scrollPosition, Thing thing, Caravan caravan, ref Pawn specificNeedsTabForPawn, bool doNeeds, bool listingUsesAbandonSpecificCountButtons)
		{
			float num = (!(thing is Pawn)) ? 30f : 50f;
			float num2 = scrollPosition.y - num;
			float num3 = scrollPosition.y + scrollOutRect.height;
			if (curY > num2 && curY < num3)
			{
				CaravanPeopleAndItemsTabUtility.DoRow(new Rect(0f, curY, viewRect.width, num), thing, caravan, ref specificNeedsTabForPawn, doNeeds, listingUsesAbandonSpecificCountButtons);
			}
			curY += num;
		}

		private static void DoRow(Rect rect, Thing thing, Caravan caravan, ref Pawn specificNeedsTabForPawn, bool doNeeds, bool listingUsesAbandonSpecificCountButtons)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = rect.AtZero();
			Pawn pawn = thing as Pawn;
			if (listingUsesAbandonSpecificCountButtons)
			{
				if (thing.stackCount != 1)
				{
					CaravanPeopleAndItemsTabUtility.DoAbandonSpecificCountButton(rect2, thing, caravan);
				}
				rect2.width -= 24f;
			}
			CaravanPeopleAndItemsTabUtility.DoAbandonButton(rect2, thing, caravan);
			rect2.width -= 24f;
			Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, thing);
			rect2.width -= 24f;
			if (pawn != null && !pawn.Dead)
			{
				CaravanPeopleAndItemsTabUtility.DoOpenSpecificTabButton(rect2, pawn, ref specificNeedsTabForPawn);
			}
			rect2.width -= 24f;
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
			}
			Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
			Widgets.ThingIcon(rect3, thing, 1f);
			if (pawn != null)
			{
				Rect bgRect = new Rect(rect3.xMax + 4f, 16f, 100f, 18f);
				GenMapUI.DrawPawnLabel(pawn, bgRect, 1f, 100f, null, GameFont.Small, false, false);
				if (doNeeds)
				{
					CaravanPeopleAndItemsTabUtility.GetNeedsToDisplay(pawn, CaravanPeopleAndItemsTabUtility.tmpNeeds);
					float xMax = bgRect.xMax;
					for (int i = 0; i < CaravanPeopleAndItemsTabUtility.tmpNeeds.Count; i++)
					{
						Need need = CaravanPeopleAndItemsTabUtility.tmpNeeds[i];
						int maxThresholdMarkers = 0;
						bool doTooltip = true;
						Rect rect4 = new Rect(xMax, 0f, 100f, 50f);
						Need_Mood mood = need as Need_Mood;
						if (mood != null)
						{
							maxThresholdMarkers = 1;
							doTooltip = false;
							TooltipHandler.TipRegion(rect4, new TipSignal(() => CaravanPeopleAndItemsTabUtility.CustomMoodNeedTooltip(mood), rect4.GetHashCode()));
						}
						need.DrawOnGUI(rect4, maxThresholdMarkers, 10f, false, doTooltip);
						xMax = rect4.xMax;
					}
				}
				if (pawn.Downed)
				{
					GUI.color = new Color(1f, 0f, 0f, 0.5f);
					Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
					GUI.color = Color.white;
				}
			}
			else
			{
				Rect rect5 = new Rect(rect3.xMax + 4f, 0f, 300f, 30f);
				Text.Anchor = TextAnchor.MiddleLeft;
				Text.WordWrap = false;
				Widgets.Label(rect5, thing.LabelCap);
				Text.Anchor = TextAnchor.UpperLeft;
				Text.WordWrap = true;
			}
			GUI.EndGroup();
		}

		private static void GetNeedsToDisplay(Pawn p, List<Need> outNeeds)
		{
			outNeeds.Clear();
			List<Need> allNeeds = p.needs.AllNeeds;
			for (int i = 0; i < allNeeds.Count; i++)
			{
				Need need = allNeeds[i];
				if (need.def.showForCaravanMembers)
				{
					outNeeds.Add(need);
				}
			}
			PawnNeedsUIUtility.SortInDisplayOrder(outNeeds);
		}

		private static string CustomMoodNeedTooltip(Need_Mood mood)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(mood.GetTipString());
			PawnNeedsUIUtility.GetThoughtGroupsInDisplayOrder(mood, CaravanPeopleAndItemsTabUtility.thoughtGroupsPresent);
			bool flag = false;
			for (int i = 0; i < CaravanPeopleAndItemsTabUtility.thoughtGroupsPresent.Count; i++)
			{
				Thought thought = CaravanPeopleAndItemsTabUtility.thoughtGroupsPresent[i];
				List<Thought> list = mood.thoughts.ThoughtsInGroup(thought).ToList<Thought>();
				Thought leadingThoughtInGroup = PawnNeedsUIUtility.GetLeadingThoughtInGroup(list);
				if (leadingThoughtInGroup.VisibleInNeedsTab)
				{
					if (!flag)
					{
						flag = true;
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(leadingThoughtInGroup.LabelCap);
					if (list.Count > 1)
					{
						stringBuilder.Append(" x");
						stringBuilder.Append(list.Count);
					}
					stringBuilder.Append(": ");
					stringBuilder.AppendLine(mood.thoughts.MoodOffsetOfThoughtGroup(thought).ToString("##0"));
				}
			}
			return stringBuilder.ToString();
		}
	}
}
