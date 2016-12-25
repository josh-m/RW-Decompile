using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class SocialCardUtility
	{
		private class CachedSocialTabEntry
		{
			public Pawn otherPawn;

			public int opinionOfOtherPawn;

			public int opinionOfMe;

			public List<PawnRelationDef> relations = new List<PawnRelationDef>();
		}

		private class CachedSocialTabEntryComparer : IComparer<SocialCardUtility.CachedSocialTabEntry>
		{
			public int Compare(SocialCardUtility.CachedSocialTabEntry a, SocialCardUtility.CachedSocialTabEntry b)
			{
				bool flag = a.relations.Any<PawnRelationDef>();
				bool flag2 = b.relations.Any<PawnRelationDef>();
				if (flag != flag2)
				{
					return flag2.CompareTo(flag);
				}
				if (flag && flag2)
				{
					float num = -3.40282347E+38f;
					for (int i = 0; i < a.relations.Count; i++)
					{
						if (a.relations[i].importance > num)
						{
							num = a.relations[i].importance;
						}
					}
					float num2 = -3.40282347E+38f;
					for (int j = 0; j < b.relations.Count; j++)
					{
						if (b.relations[j].importance > num2)
						{
							num2 = b.relations[j].importance;
						}
					}
					if (num != num2)
					{
						return num2.CompareTo(num);
					}
				}
				if (a.opinionOfOtherPawn != b.opinionOfOtherPawn)
				{
					return b.opinionOfOtherPawn.CompareTo(a.opinionOfOtherPawn);
				}
				return a.otherPawn.thingIDNumber.CompareTo(b.otherPawn.thingIDNumber);
			}
		}

		private const float TopPadding = 20f;

		private const float RowTopPadding = 3f;

		private const float RowLeftRightPadding = 5f;

		private static Vector2 listScrollPosition = Vector2.zero;

		private static float listScrollViewHeight = 0f;

		private static Vector2 logScrollPosition = Vector2.zero;

		private static bool showAllRelations;

		private static List<SocialCardUtility.CachedSocialTabEntry> cachedEntries = new List<SocialCardUtility.CachedSocialTabEntry>();

		private static Pawn cachedForPawn;

		private static readonly Color RelationLabelColor = new Color(0.6f, 0.6f, 0.6f);

		private static readonly Color PawnLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

		private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		private static SocialCardUtility.CachedSocialTabEntryComparer CachedEntriesComparer = new SocialCardUtility.CachedSocialTabEntryComparer();

		private static HashSet<Pawn> tmpCached = new HashSet<Pawn>();

		private static HashSet<Pawn> tmpToCache = new HashSet<Pawn>();

		private static List<Pair<string, int>> logStrings = new List<Pair<string, int>>();

		public static void DrawSocialCard(Rect rect, Pawn pawn)
		{
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Small;
			Rect rect2 = new Rect(0f, 20f, rect.width, rect.height - 20f);
			Rect rect3 = rect2.ContractedBy(10f);
			Rect rect4 = rect3;
			Rect rect5 = rect3;
			rect4.height *= 0.63f;
			rect5.y = rect4.yMax + 17f;
			rect5.yMax = rect3.yMax;
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Widgets.DrawLineHorizontal(0f, (rect4.yMax + rect5.y) / 2f, rect.width);
			GUI.color = Color.white;
			if (Prefs.DevMode)
			{
				Rect rect6 = new Rect(5f, 5f, rect.width, 22f);
				SocialCardUtility.DrawDebugOptions(rect6, pawn);
			}
			SocialCardUtility.DrawRelationsAndOpinions(rect4, pawn);
			SocialCardUtility.DrawInteractionsLog(rect5, pawn);
			GUI.EndGroup();
		}

		private static void CheckRecache(Pawn selPawnForSocialInfo)
		{
			if (SocialCardUtility.cachedForPawn != selPawnForSocialInfo || Time.frameCount % 20 == 0)
			{
				SocialCardUtility.Recache(selPawnForSocialInfo);
			}
		}

		private static void Recache(Pawn selPawnForSocialInfo)
		{
			SocialCardUtility.cachedForPawn = selPawnForSocialInfo;
			SocialCardUtility.tmpToCache.Clear();
			foreach (Pawn current in selPawnForSocialInfo.relations.RelatedPawns)
			{
				if (SocialCardUtility.ShouldShowPawnRelations(current, selPawnForSocialInfo))
				{
					SocialCardUtility.tmpToCache.Add(current);
				}
			}
			List<Pawn> list = null;
			if (selPawnForSocialInfo.MapHeld != null)
			{
				list = selPawnForSocialInfo.MapHeld.mapPawns.AllPawnsSpawned;
			}
			else if (selPawnForSocialInfo.IsCaravanMember())
			{
				list = selPawnForSocialInfo.GetCaravan().PawnsListForReading;
			}
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					Pawn pawn = list[i];
					if (pawn.RaceProps.Humanlike && pawn != selPawnForSocialInfo && SocialCardUtility.ShouldShowPawnRelations(pawn, selPawnForSocialInfo) && !SocialCardUtility.tmpToCache.Contains(pawn))
					{
						if (selPawnForSocialInfo.relations.OpinionOf(pawn) != 0 || pawn.relations.OpinionOf(selPawnForSocialInfo) != 0)
						{
							SocialCardUtility.tmpToCache.Add(pawn);
						}
					}
				}
			}
			SocialCardUtility.cachedEntries.RemoveAll((SocialCardUtility.CachedSocialTabEntry x) => !SocialCardUtility.tmpToCache.Contains(x.otherPawn));
			SocialCardUtility.tmpCached.Clear();
			for (int j = 0; j < SocialCardUtility.cachedEntries.Count; j++)
			{
				SocialCardUtility.tmpCached.Add(SocialCardUtility.cachedEntries[j].otherPawn);
			}
			foreach (Pawn current2 in SocialCardUtility.tmpToCache)
			{
				if (!SocialCardUtility.tmpCached.Contains(current2))
				{
					SocialCardUtility.CachedSocialTabEntry cachedSocialTabEntry = new SocialCardUtility.CachedSocialTabEntry();
					cachedSocialTabEntry.otherPawn = current2;
					SocialCardUtility.cachedEntries.Add(cachedSocialTabEntry);
				}
			}
			SocialCardUtility.tmpCached.Clear();
			SocialCardUtility.tmpToCache.Clear();
			for (int k = 0; k < SocialCardUtility.cachedEntries.Count; k++)
			{
				SocialCardUtility.RecacheEntry(SocialCardUtility.cachedEntries[k], selPawnForSocialInfo);
			}
			SocialCardUtility.cachedEntries.Sort(SocialCardUtility.CachedEntriesComparer);
		}

		private static bool ShouldShowPawnRelations(Pawn pawn, Pawn selPawnForSocialInfo)
		{
			return SocialCardUtility.showAllRelations || pawn.relations.everSeenByPlayer;
		}

		private static void RecacheEntry(SocialCardUtility.CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
		{
			entry.opinionOfMe = entry.otherPawn.relations.OpinionOf(selPawnForSocialInfo);
			entry.opinionOfOtherPawn = selPawnForSocialInfo.relations.OpinionOf(entry.otherPawn);
			entry.relations.Clear();
			foreach (PawnRelationDef current in selPawnForSocialInfo.GetRelations(entry.otherPawn))
			{
				entry.relations.Add(current);
			}
			entry.relations.Sort((PawnRelationDef a, PawnRelationDef b) => b.importance.CompareTo(a.importance));
		}

		public static void DrawRelationsAndOpinions(Rect rect, Pawn selPawnForSocialInfo)
		{
			SocialCardUtility.CheckRecache(selPawnForSocialInfo);
			if (Current.ProgramState != ProgramState.Playing)
			{
				SocialCardUtility.showAllRelations = false;
			}
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, SocialCardUtility.listScrollViewHeight);
			Widgets.BeginScrollView(outRect, ref SocialCardUtility.listScrollPosition, viewRect);
			float num = 0f;
			float y = SocialCardUtility.listScrollPosition.y;
			float num2 = SocialCardUtility.listScrollPosition.y + outRect.height;
			for (int i = 0; i < SocialCardUtility.cachedEntries.Count; i++)
			{
				float rowHeight = SocialCardUtility.GetRowHeight(SocialCardUtility.cachedEntries[i], viewRect.width, selPawnForSocialInfo);
				if (num > y - rowHeight && num < num2)
				{
					SocialCardUtility.DrawPawnRow(num, viewRect.width, SocialCardUtility.cachedEntries[i], selPawnForSocialInfo);
				}
				num += rowHeight;
			}
			if (!SocialCardUtility.cachedEntries.Any<SocialCardUtility.CachedSocialTabEntry>())
			{
				GUI.color = Color.gray;
				Text.Anchor = TextAnchor.UpperCenter;
				Rect rect2 = new Rect(0f, 0f, viewRect.width, 30f);
				Widgets.Label(rect2, "NoRelationships".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
			}
			if (Event.current.type == EventType.Layout)
			{
				SocialCardUtility.listScrollViewHeight = num + 30f;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
			GUI.color = Color.white;
		}

		private static void DrawPawnRow(float y, float width, SocialCardUtility.CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
		{
			float rowHeight = SocialCardUtility.GetRowHeight(entry, width, selPawnForSocialInfo);
			Rect rect = new Rect(0f, y, width, rowHeight);
			Pawn otherPawn = entry.otherPawn;
			if (Mouse.IsOver(rect))
			{
				GUI.color = SocialCardUtility.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			TooltipHandler.TipRegion(rect, () => SocialCardUtility.GetPawnRowTooltip(entry, selPawnForSocialInfo), entry.otherPawn.thingIDNumber * 13 + selPawnForSocialInfo.thingIDNumber);
			if (Widgets.ButtonInvisible(rect, false))
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					if (otherPawn.Dead)
					{
						Messages.Message("MessageCantSelectDeadPawn".Translate(new object[]
						{
							otherPawn.LabelShort
						}).CapitalizeFirst(), MessageSound.RejectInput);
					}
					else if (otherPawn.MapHeld != null || otherPawn.IsCaravanMember())
					{
						JumpToTargetUtility.TryJumpAndSelect(otherPawn);
					}
					else
					{
						Messages.Message("MessageCantSelectOffMapPawn".Translate(new object[]
						{
							otherPawn.LabelShort
						}).CapitalizeFirst(), MessageSound.RejectInput);
					}
				}
				else if (Find.GameInitData.startingPawns.Contains(otherPawn))
				{
					Page_ConfigureStartingPawns page_ConfigureStartingPawns = Find.WindowStack.WindowOfType<Page_ConfigureStartingPawns>();
					if (page_ConfigureStartingPawns != null)
					{
						page_ConfigureStartingPawns.SelectPawn(otherPawn);
						SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
					}
				}
			}
			float width2;
			float width3;
			float width4;
			float width5;
			float width6;
			SocialCardUtility.CalculateColumnsWidths(width, out width2, out width3, out width4, out width5, out width6);
			Rect rect2 = new Rect(5f, y + 3f, width2, rowHeight - 3f);
			SocialCardUtility.DrawRelationLabel(entry, rect2, selPawnForSocialInfo);
			Rect rect3 = new Rect(rect2.xMax, y + 3f, width3, rowHeight - 3f);
			SocialCardUtility.DrawPawnLabel(otherPawn, rect3);
			Rect rect4 = new Rect(rect3.xMax, y + 3f, width4, rowHeight - 3f);
			SocialCardUtility.DrawMyOpinion(entry, rect4, selPawnForSocialInfo);
			Rect rect5 = new Rect(rect4.xMax, y + 3f, width5, rowHeight - 3f);
			SocialCardUtility.DrawHisOpinion(entry, rect5, selPawnForSocialInfo);
			Rect rect6 = new Rect(rect5.xMax, y + 3f, width6, rowHeight - 3f);
			SocialCardUtility.DrawPawnSituationLabel(entry.otherPawn, rect6, selPawnForSocialInfo);
		}

		private static float GetRowHeight(SocialCardUtility.CachedSocialTabEntry entry, float rowWidth, Pawn selPawnForSocialInfo)
		{
			float width;
			float width2;
			float num;
			float num2;
			float num3;
			SocialCardUtility.CalculateColumnsWidths(rowWidth, out width, out width2, out num, out num2, out num3);
			float num4 = 0f;
			num4 = Mathf.Max(num4, Text.CalcHeight(SocialCardUtility.GetRelationsString(entry, selPawnForSocialInfo), width));
			num4 = Mathf.Max(num4, Text.CalcHeight(SocialCardUtility.GetPawnLabel(entry.otherPawn), width2));
			return num4 + 3f;
		}

		private static void CalculateColumnsWidths(float rowWidth, out float relationsWidth, out float pawnLabelWidth, out float myOpinionWidth, out float hisOpinionWidth, out float pawnSituationLabelWidth)
		{
			float num = rowWidth - 10f;
			relationsWidth = num * 0.23f;
			pawnLabelWidth = num * 0.41f;
			myOpinionWidth = num * 0.075f;
			hisOpinionWidth = num * 0.085f;
			pawnSituationLabelWidth = num * 0.2f;
			if (myOpinionWidth < 25f)
			{
				pawnLabelWidth -= 25f - myOpinionWidth;
				myOpinionWidth = 25f;
			}
			if (hisOpinionWidth < 35f)
			{
				pawnLabelWidth -= 35f - hisOpinionWidth;
				hisOpinionWidth = 35f;
			}
		}

		private static void DrawRelationLabel(SocialCardUtility.CachedSocialTabEntry entry, Rect rect, Pawn selPawnForSocialInfo)
		{
			string relationsString = SocialCardUtility.GetRelationsString(entry, selPawnForSocialInfo);
			if (!relationsString.NullOrEmpty())
			{
				GUI.color = SocialCardUtility.RelationLabelColor;
				Widgets.Label(rect, relationsString);
			}
		}

		private static void DrawPawnLabel(Pawn pawn, Rect rect)
		{
			GUI.color = SocialCardUtility.PawnLabelColor;
			Widgets.Label(rect, SocialCardUtility.GetPawnLabel(pawn));
		}

		private static void DrawMyOpinion(SocialCardUtility.CachedSocialTabEntry entry, Rect rect, Pawn selPawnForSocialInfo)
		{
			if (!entry.otherPawn.RaceProps.Humanlike || !selPawnForSocialInfo.RaceProps.Humanlike)
			{
				return;
			}
			int opinionOfOtherPawn = entry.opinionOfOtherPawn;
			GUI.color = SocialCardUtility.OpinionLabelColor(opinionOfOtherPawn);
			Widgets.Label(rect, opinionOfOtherPawn.ToStringWithSign());
		}

		private static void DrawHisOpinion(SocialCardUtility.CachedSocialTabEntry entry, Rect rect, Pawn selPawnForSocialInfo)
		{
			if (!entry.otherPawn.RaceProps.Humanlike || !selPawnForSocialInfo.RaceProps.Humanlike)
			{
				return;
			}
			int opinionOfMe = entry.opinionOfMe;
			Color color = SocialCardUtility.OpinionLabelColor(opinionOfMe);
			GUI.color = new Color(color.r, color.g, color.b, 0.4f);
			Widgets.Label(rect, "(" + opinionOfMe.ToStringWithSign() + ")");
		}

		private static void DrawPawnSituationLabel(Pawn pawn, Rect rect, Pawn selPawnForSocialInfo)
		{
			GUI.color = Color.gray;
			string label = SocialCardUtility.GetPawnSituationLabel(pawn, selPawnForSocialInfo).Truncate(rect.width, null);
			Widgets.Label(rect, label);
		}

		private static Color OpinionLabelColor(int opinion)
		{
			if (Mathf.Abs(opinion) < 10)
			{
				return Color.gray;
			}
			if (opinion < 0)
			{
				return Color.red;
			}
			return Color.green;
		}

		private static string GetPawnLabel(Pawn pawn)
		{
			if (pawn.Name != null)
			{
				return pawn.Name.ToStringFull;
			}
			return pawn.LabelCapNoCount;
		}

		public static string GetPawnSituationLabel(Pawn pawn, Pawn fromPOV)
		{
			if (pawn.Dead || pawn.Destroyed)
			{
				return "Dead".Translate();
			}
			if (PawnUtility.IsKidnappedPawn(pawn))
			{
				return "Kidnapped".Translate();
			}
			if (pawn.kindDef == PawnKindDefOf.Slave)
			{
				return "Slave".Translate();
			}
			if (PawnUtility.IsFactionLeader(pawn))
			{
				return "FactionLeader".Translate();
			}
			Faction faction = pawn.Faction;
			if (faction == fromPOV.Faction)
			{
				return string.Empty;
			}
			if (faction == null || fromPOV.Faction == null)
			{
				return "Neutral".Translate();
			}
			if (!faction.HostileTo(fromPOV.Faction))
			{
				return "Neutral".Translate() + ", " + faction.Name;
			}
			return "Hostile".Translate() + ", " + faction.Name;
		}

		private static string GetRelationsString(SocialCardUtility.CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
		{
			string text = string.Empty;
			if (entry.relations.Count != 0)
			{
				for (int i = 0; i < entry.relations.Count; i++)
				{
					PawnRelationDef pawnRelationDef = entry.relations[i];
					if (!text.NullOrEmpty())
					{
						text = text + ", " + pawnRelationDef.GetGenderSpecificLabel(entry.otherPawn);
					}
					else
					{
						text = pawnRelationDef.GetGenderSpecificLabelCap(entry.otherPawn);
					}
				}
				return text;
			}
			if (entry.opinionOfOtherPawn < -20)
			{
				return "Rival".Translate();
			}
			if (entry.opinionOfOtherPawn > 20)
			{
				return "Friend".Translate();
			}
			return "Acquaintance".Translate();
		}

		private static string GetPawnRowTooltip(SocialCardUtility.CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (entry.otherPawn.RaceProps.Humanlike && selPawnForSocialInfo.RaceProps.Humanlike)
			{
				stringBuilder.AppendLine(selPawnForSocialInfo.relations.OpinionExplanation(entry.otherPawn));
				stringBuilder.AppendLine();
				stringBuilder.Append("SomeonesOpinionOfMe".Translate(new object[]
				{
					entry.otherPawn.LabelShort
				}));
				stringBuilder.Append(": ");
				stringBuilder.Append(entry.opinionOfMe.ToStringWithSign());
			}
			else
			{
				stringBuilder.AppendLine(entry.otherPawn.LabelCapNoCount);
				string pawnSituationLabel = SocialCardUtility.GetPawnSituationLabel(entry.otherPawn, selPawnForSocialInfo);
				if (!pawnSituationLabel.NullOrEmpty())
				{
					stringBuilder.AppendLine(pawnSituationLabel);
				}
				stringBuilder.AppendLine("--------------");
				stringBuilder.Append(SocialCardUtility.GetRelationsString(entry, selPawnForSocialInfo));
			}
			if (Prefs.DevMode)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("(debug) Compatibility: " + selPawnForSocialInfo.relations.CompatibilityWith(entry.otherPawn).ToString("F2"));
				stringBuilder.Append("(debug) RomanceChanceFactor: " + selPawnForSocialInfo.relations.SecondaryRomanceChanceFactor(entry.otherPawn).ToString("F2"));
			}
			return stringBuilder.ToString();
		}

		private static void DrawInteractionsLog(Rect rect, Pawn pawn)
		{
			float width = rect.width - 26f - 3f;
			List<PlayLogEntry> allEntries = Find.PlayLog.AllEntries;
			SocialCardUtility.logStrings.Clear();
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < allEntries.Count; i++)
			{
				if (allEntries[i].Concerns(pawn))
				{
					string text = allEntries[i].ToGameStringFromPOV(pawn);
					SocialCardUtility.logStrings.Add(new Pair<string, int>(text, i));
					num += Mathf.Max(26f, Text.CalcHeight(text, width));
					num2++;
					if (num2 >= 12)
					{
						break;
					}
				}
			}
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, num);
			Widgets.BeginScrollView(rect, ref SocialCardUtility.logScrollPosition, viewRect);
			float num3 = 0f;
			for (int j = 0; j < SocialCardUtility.logStrings.Count; j++)
			{
				string first = SocialCardUtility.logStrings[j].First;
				PlayLogEntry entry = allEntries[SocialCardUtility.logStrings[j].Second];
				if (entry.Age > 7500)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.5f);
				}
				float num4 = Mathf.Max(26f, Text.CalcHeight(first, width));
				if (entry.Icon != null)
				{
					Rect position = new Rect(0f, num3, 26f, 26f);
					GUI.DrawTexture(position, entry.Icon);
				}
				Rect rect2 = new Rect(29f, num3, width, num4);
				Widgets.DrawHighlightIfMouseover(rect2);
				Widgets.Label(rect2, first);
				TooltipHandler.TipRegion(rect2, () => entry.GetTipString(), 613261 + j * 611);
				if (Widgets.ButtonInvisible(rect2, false))
				{
					entry.ClickedFromPOV(pawn);
				}
				GUI.color = Color.white;
				num3 += num4;
			}
			GUI.EndScrollView();
		}

		private static void DrawDebugOptions(Rect rect, Pawn pawn)
		{
			GUI.BeginGroup(rect);
			Widgets.CheckboxLabeled(new Rect(0f, 0f, 145f, 22f), "Dev: AllRelations", ref SocialCardUtility.showAllRelations, false);
			if (Widgets.ButtonText(new Rect(150f, 0f, 115f, 22f), "Debug info", true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				list.Add(new FloatMenuOption("AttractionTo", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("My gender: " + pawn.gender);
					stringBuilder.AppendLine("My age: " + pawn.ageTracker.AgeBiologicalYears);
					stringBuilder.AppendLine();
					IOrderedEnumerable<Pawn> orderedEnumerable = from x in pawn.Map.mapPawns.AllPawnsSpawned
					where x.def == pawn.def
					orderby pawn.relations.SecondaryRomanceChanceFactor(x) descending
					select x;
					foreach (Pawn current in orderedEnumerable)
					{
						if (current != pawn)
						{
							stringBuilder.AppendLine(string.Concat(new object[]
							{
								current.LabelShort,
								" (",
								current.gender,
								", age: ",
								current.ageTracker.AgeBiologicalYears,
								", compat: ",
								pawn.relations.CompatibilityWith(current).ToString("F2"),
								"): ",
								pawn.relations.SecondaryRomanceChanceFactor(current).ToStringPercent("F0"),
								"        [vs ",
								current.relations.SecondaryRomanceChanceFactor(pawn).ToStringPercent("F0"),
								"]"
							}));
						}
					}
					Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder.ToString(), null, null, null, null, null, false));
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
				list.Add(new FloatMenuOption("CompatibilityTo", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("My age: " + pawn.ageTracker.AgeBiologicalYears);
					stringBuilder.AppendLine();
					IOrderedEnumerable<Pawn> orderedEnumerable = from x in pawn.Map.mapPawns.AllPawnsSpawned
					where x.def == pawn.def
					orderby pawn.relations.CompatibilityWith(x) descending
					select x;
					foreach (Pawn current in orderedEnumerable)
					{
						if (current != pawn)
						{
							stringBuilder.AppendLine(string.Concat(new object[]
							{
								current.LabelShort,
								" (",
								current.KindLabel,
								", age: ",
								current.ageTracker.AgeBiologicalYears,
								"): ",
								pawn.relations.CompatibilityWith(current).ToString("0.##")
							}));
						}
					}
					Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder.ToString(), null, null, null, null, null, false));
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
				if (pawn.RaceProps.Humanlike)
				{
					list.Add(new FloatMenuOption("Interaction chance", delegate
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendLine("(selected pawn is the initiator)");
						stringBuilder.AppendLine("(\"fight chance\" is the chance that the receiver will start social fight)");
						stringBuilder.AppendLine("Interaction chance (real chance, not just weights):");
						IOrderedEnumerable<Pawn> orderedEnumerable = from x in pawn.Map.mapPawns.AllPawnsSpawned
						where x.RaceProps.Humanlike
						orderby (x.Faction != null) ? x.Faction.loadID : -1
						select x;
						foreach (Pawn c in orderedEnumerable)
						{
							if (c != pawn)
							{
								stringBuilder.AppendLine();
								stringBuilder.AppendLine(string.Concat(new object[]
								{
									c.LabelShort,
									" (",
									c.KindLabel,
									", ",
									c.gender,
									", age: ",
									c.ageTracker.AgeBiologicalYears,
									", compat: ",
									pawn.relations.CompatibilityWith(c).ToString("F2"),
									", attr: ",
									pawn.relations.SecondaryRomanceChanceFactor(c).ToStringPercent("F0"),
									"):"
								}));
								List<InteractionDef> list2 = (from x in DefDatabase<InteractionDef>.AllDefs
								where x.Worker.RandomSelectionWeight(pawn, c) > 0f
								orderby x.Worker.RandomSelectionWeight(pawn, c) descending
								select x).ToList<InteractionDef>();
								float num = list2.Sum((InteractionDef x) => x.Worker.RandomSelectionWeight(pawn, c));
								foreach (InteractionDef current in list2)
								{
									float f = c.interactions.SocialFightChance(current, pawn);
									float f2 = current.Worker.RandomSelectionWeight(pawn, c) / num;
									stringBuilder.AppendLine(string.Concat(new string[]
									{
										"  ",
										current.defName,
										": ",
										f2.ToStringPercent(),
										" (fight chance: ",
										f.ToStringPercent("F2"),
										")"
									}));
									if (current == InteractionDefOf.RomanceAttempt)
									{
										stringBuilder.AppendLine("    success chance: " + ((InteractionWorker_RomanceAttempt)current.Worker).SuccessChance(pawn, c).ToStringPercent());
									}
									else if (current == InteractionDefOf.MarriageProposal)
									{
										stringBuilder.AppendLine("    acceptance chance: " + ((InteractionWorker_MarriageProposal)current.Worker).AcceptanceChance(pawn, c).ToStringPercent());
									}
								}
							}
						}
						Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder.ToString(), null, null, null, null, null, false));
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
					list.Add(new FloatMenuOption("Lovin' MTB", delegate
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendLine("Lovin' MTB hours with pawn X.");
						stringBuilder.AppendLine("Assuming both pawns are in bed and are partners.");
						stringBuilder.AppendLine();
						IOrderedEnumerable<Pawn> orderedEnumerable = from x in pawn.Map.mapPawns.AllPawnsSpawned
						where x.def == pawn.def
						orderby pawn.relations.SecondaryRomanceChanceFactor(x) descending
						select x;
						foreach (Pawn current in orderedEnumerable)
						{
							if (current != pawn)
							{
								stringBuilder.AppendLine(string.Concat(new object[]
								{
									current.LabelShort,
									" (",
									current.KindLabel,
									", age: ",
									current.ageTracker.AgeBiologicalYears,
									"): ",
									LovePartnerRelationUtility.GetLovinMtbHours(pawn, current).ToString("F1"),
									" h"
								}));
							}
						}
						Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder.ToString(), null, null, null, null, null, false));
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				list.Add(new FloatMenuOption("Test per pawns pair compatibility factor probability", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					int num = 0;
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					int num5 = 0;
					int num6 = 0;
					int num7 = 0;
					int num8 = 0;
					float num9 = -999999f;
					float num10 = 999999f;
					for (int i = 0; i < 10000; i++)
					{
						int otherPawnID = Rand.RangeInclusive(0, 30000);
						float num11 = pawn.relations.ConstantPerPawnsPairCompatibilityOffset(otherPawnID);
						if (num11 < -3f)
						{
							num++;
						}
						else if (num11 < -2f)
						{
							num2++;
						}
						else if (num11 < -1f)
						{
							num3++;
						}
						else if (num11 < 0f)
						{
							num4++;
						}
						else if (num11 < 1f)
						{
							num5++;
						}
						else if (num11 < 2f)
						{
							num6++;
						}
						else if (num11 < 3f)
						{
							num7++;
						}
						else
						{
							num8++;
						}
						if (num11 > num9)
						{
							num9 = num11;
						}
						else if (num11 < num10)
						{
							num10 = num11;
						}
					}
					stringBuilder.AppendLine("< -3: " + ((float)num / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< -2: " + ((float)num2 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< -1: " + ((float)num3 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< 0: " + ((float)num4 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< 1: " + ((float)num5 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< 2: " + ((float)num6 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("< 3: " + ((float)num7 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine("> 3: " + ((float)num8 / 10000f).ToStringPercent("F2"));
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("trials: " + 10000);
					stringBuilder.AppendLine("min: " + num10);
					stringBuilder.AppendLine("max: " + num9);
					Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder.ToString(), null, null, null, null, null, false));
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
				Find.WindowStack.Add(new FloatMenu(list));
			}
			GUI.EndGroup();
		}
	}
}
