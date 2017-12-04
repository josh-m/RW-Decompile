using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Page_ConfigureStartingPawns : Page
	{
		private Pawn curPawn;

		private const float TabAreaWidth = 140f;

		private const float RightRectLeftPadding = 5f;

		private const float PawnEntryHeight = 60f;

		private const float SkillSummaryHeight = 141f;

		private const int SkillSummaryColumns = 4;

		private const int TeamSkillExtraInset = 10;

		private static readonly Vector2 PawnPortraitSize = new Vector2(100f, 140f);

		private static readonly Vector2 PawnSelectorPortraitSize = new Vector2(70f, 110f);

		private int SkillsPerColumn = -1;

		public override string PageTitle
		{
			get
			{
				return "CreateCharacters".Translate();
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			if (Find.GameInitData.startingPawns.Count > 0)
			{
				this.curPawn = Find.GameInitData.startingPawns[0];
			}
		}

		public override void PostOpen()
		{
			base.PostOpen();
			TutorSystem.Notify_Event("PageStart-ConfigureStartingPawns");
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DrawPageTitle(rect);
			rect.yMin += 45f;
			base.DoBottomButtons(rect, "Start".Translate(), null, null, true);
			rect.yMax -= 38f;
			Rect rect2 = rect;
			rect2.width = 140f;
			this.DrawPawnList(rect2);
			UIHighlighter.HighlightOpportunity(rect2, "ReorderPawn");
			Rect rect3 = rect;
			rect3.xMin += 140f;
			Rect rect4 = rect3.BottomPartPixels(141f);
			rect3.yMax = rect4.yMin;
			rect3 = rect3.ContractedBy(4f);
			rect4 = rect4.ContractedBy(4f);
			this.DrawPortraitArea(rect3);
			this.DrawSkillSummaries(rect4);
		}

		private void DrawPawnList(Rect rect)
		{
			Rect rect2 = rect;
			rect2.height = 60f;
			rect2 = rect2.ContractedBy(4f);
			int groupID = ReorderableWidget.NewGroup(delegate(int from, int to)
			{
				if (!TutorSystem.AllowAction("ReorderPawn"))
				{
					return;
				}
				Pawn item = Find.GameInitData.startingPawns[from];
				Find.GameInitData.startingPawns.RemoveAt(from);
				Find.GameInitData.startingPawns.Insert(to, item);
				TutorSystem.Notify_Event("ReorderPawn");
			});
			rect2.y += 15f;
			this.DrawPawnListLabelAbove(rect2, "StartingPawnsSelected".Translate());
			for (int i = 0; i < Find.GameInitData.startingPawns.Count; i++)
			{
				if (i == Find.GameInitData.startingPawnCount)
				{
					rect2.y += 30f;
					this.DrawPawnListLabelAbove(rect2, "StartingPawnsLeftBehind".Translate());
				}
				Pawn pawn = Find.GameInitData.startingPawns[i];
				GUI.BeginGroup(rect2);
				Rect rect3 = new Rect(Vector2.zero, rect2.size);
				Widgets.DrawOptionBackground(rect3, this.curPawn == pawn);
				MouseoverSounds.DoRegion(rect3);
				GUI.color = new Color(1f, 1f, 1f, 0.2f);
				GUI.DrawTexture(new Rect(110f - Page_ConfigureStartingPawns.PawnSelectorPortraitSize.x / 2f, 40f - Page_ConfigureStartingPawns.PawnSelectorPortraitSize.y / 2f, Page_ConfigureStartingPawns.PawnSelectorPortraitSize.x, Page_ConfigureStartingPawns.PawnSelectorPortraitSize.y), PortraitsCache.Get(pawn, Page_ConfigureStartingPawns.PawnSelectorPortraitSize, default(Vector3), 1f));
				GUI.color = Color.white;
				Rect rect4 = rect3.ContractedBy(4f).Rounded();
				NameTriple nameTriple = pawn.Name as NameTriple;
				string label;
				if (nameTriple != null)
				{
					label = ((!string.IsNullOrEmpty(nameTriple.Nick)) ? nameTriple.Nick : nameTriple.First);
				}
				else
				{
					label = pawn.LabelShort;
				}
				Widgets.Label(rect4.TopPart(0.5f).Rounded(), label);
				if (Text.CalcSize(pawn.story.Title).x > rect4.width)
				{
					Widgets.Label(rect4.BottomPart(0.5f).Rounded(), pawn.story.TitleShort);
				}
				else
				{
					Widgets.Label(rect4.BottomPart(0.5f).Rounded(), pawn.story.Title);
				}
				if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect3))
				{
					this.curPawn = pawn;
					SoundDefOf.TickTiny.PlayOneShotOnCamera(null);
				}
				GUI.EndGroup();
				if (ReorderableWidget.Reorderable(groupID, rect2.ExpandedBy(4f)))
				{
					Widgets.DrawRectFast(rect2, Widgets.WindowBGFillColor * new Color(1f, 1f, 1f, 0.5f), null);
				}
				TooltipHandler.TipRegion(rect2, new TipSignal("DragToReorder".Translate(), pawn.GetHashCode() * 3499));
				rect2.y += 60f;
			}
		}

		private void DrawPawnListLabelAbove(Rect rect, string label)
		{
			rect.yMax = rect.yMin;
			rect.yMin -= 30f;
			rect.xMin -= 4f;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.LowerLeft;
			Widgets.Label(rect, label);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}

		private void DrawPortraitArea(Rect rect)
		{
			Widgets.DrawMenuSection(rect);
			rect = rect.ContractedBy(17f);
			GUI.DrawTexture(new Rect(rect.center.x - Page_ConfigureStartingPawns.PawnPortraitSize.x / 2f, rect.yMin - 20f, Page_ConfigureStartingPawns.PawnPortraitSize.x, Page_ConfigureStartingPawns.PawnPortraitSize.y), PortraitsCache.Get(this.curPawn, Page_ConfigureStartingPawns.PawnPortraitSize, default(Vector3), 1f));
			Rect rect2 = rect;
			rect2.width = 500f;
			CharacterCardUtility.DrawCharacterCard(rect2, this.curPawn, new Action(this.RandomizeCurPawn), rect);
			Rect rect3 = rect;
			rect3.yMin += 100f;
			rect3.xMin = rect2.xMax + 5f;
			rect3.height = 200f;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect3, "Health".Translate());
			Text.Font = GameFont.Small;
			rect3.yMin += 35f;
			HealthCardUtility.DrawHediffListing(rect3, this.curPawn, true);
			Rect rect4 = new Rect(rect3.x, rect3.yMax, rect3.width, 200f);
			Text.Font = GameFont.Medium;
			Widgets.Label(rect4, "Relations".Translate());
			Text.Font = GameFont.Small;
			rect4.yMin += 35f;
			SocialCardUtility.DrawRelationsAndOpinions(rect4, this.curPawn);
		}

		private void DrawSkillSummaries(Rect rect)
		{
			rect.xMin += 10f;
			rect.xMax -= 10f;
			Widgets.DrawMenuSection(rect);
			rect = rect.ContractedBy(17f);
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(rect.min, new Vector2(rect.width, 45f)), "TeamSkills".Translate());
			Text.Font = GameFont.Small;
			rect.yMin += 45f;
			rect = rect.LeftPart(0.25f);
			rect.height = 27f;
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			if (this.SkillsPerColumn < 0)
			{
				this.SkillsPerColumn = Mathf.CeilToInt((float)(from sd in allDefsListForReading
				where sd.pawnCreatorSummaryVisible
				select sd).Count<SkillDef>() / 4f);
			}
			int num = 0;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				SkillDef skillDef = allDefsListForReading[i];
				if (skillDef.pawnCreatorSummaryVisible)
				{
					Rect r = rect;
					r.x = rect.x + rect.width * (float)(num / this.SkillsPerColumn);
					r.y = rect.y + rect.height * (float)(num % this.SkillsPerColumn);
					r.height = 24f;
					r.width -= 4f;
					Pawn pawn = this.FindBestSkillOwner(skillDef);
					SkillUI.DrawSkill(pawn.skills.GetSkill(skillDef), r.Rounded(), SkillUI.SkillDrawMode.Menu, pawn.Name.ToString());
					num++;
				}
			}
		}

		private Pawn FindBestSkillOwner(SkillDef skill)
		{
			Pawn pawn = Find.GameInitData.startingPawns[0];
			SkillRecord skillRecord = pawn.skills.GetSkill(skill);
			for (int i = 1; i < Find.GameInitData.startingPawnCount; i++)
			{
				SkillRecord skill2 = Find.GameInitData.startingPawns[i].skills.GetSkill(skill);
				if (skillRecord.TotallyDisabled || skill2.Level > skillRecord.Level || (skill2.Level == skillRecord.Level && skill2.passion > skillRecord.passion))
				{
					pawn = Find.GameInitData.startingPawns[i];
					skillRecord = skill2;
				}
			}
			return pawn;
		}

		private void RandomizeCurPawn()
		{
			if (!TutorSystem.AllowAction("RandomizePawn"))
			{
				return;
			}
			this.curPawn = StartingPawnUtility.RandomizeInPlace(this.curPawn);
			TutorSystem.Notify_Event("RandomizePawn");
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			foreach (Pawn current in Find.GameInitData.startingPawns)
			{
				if (!current.Name.IsValid)
				{
					Messages.Message("EveryoneNeedsValidName".Translate(), MessageTypeDefOf.RejectInput);
					return false;
				}
			}
			PortraitsCache.Clear();
			return true;
		}

		protected override void DoNext()
		{
			foreach (Pawn current in Find.GameInitData.startingPawns)
			{
				NameTriple nameTriple = current.Name as NameTriple;
				if (nameTriple != null && string.IsNullOrEmpty(nameTriple.Nick))
				{
					current.Name = new NameTriple(nameTriple.First, nameTriple.First, nameTriple.Last);
				}
			}
			base.DoNext();
		}

		public void SelectPawn(Pawn c)
		{
			if (c != this.curPawn)
			{
				this.curPawn = c;
			}
		}
	}
}
