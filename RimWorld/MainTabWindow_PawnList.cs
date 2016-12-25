using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class MainTabWindow_PawnList : MainTabWindow
	{
		public const float PawnRowHeight = 30f;

		protected const float NameColumnWidth = 165f;

		protected const float NameLeftMargin = 3f;

		protected Vector2 scrollPosition = Vector2.zero;

		protected List<Pawn> pawns = new List<Pawn>();

		private bool pawnListDirty;

		protected override float Margin
		{
			get
			{
				return 6f;
			}
		}

		protected int PawnsCount
		{
			get
			{
				return this.pawns.Count;
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			this.BuildPawnList();
		}

		public override void PostOpen()
		{
			base.PostOpen();
			this.windowRect.size = this.InitialSize;
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			this.windowRect.size = this.InitialSize;
		}

		protected virtual void BuildPawnList()
		{
			this.pawns.Clear();
			this.pawns.AddRange(Find.VisibleMap.mapPawns.FreeColonists);
			this.pawnListDirty = false;
		}

		public void Notify_PawnsChanged()
		{
			this.pawnListDirty = true;
		}

		protected void DrawRows(Rect rect)
		{
			if (this.pawnListDirty)
			{
				this.BuildPawnList();
			}
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, (float)this.pawns.Count * 30f);
			Widgets.BeginScrollView(rect, ref this.scrollPosition, viewRect);
			float num = 0f;
			for (int i = 0; i < this.pawns.Count; i++)
			{
				Pawn p = this.pawns[i];
				Rect rect2 = new Rect(0f, num, viewRect.width, 30f);
				if (num - this.scrollPosition.y + 30f >= 0f && num - this.scrollPosition.y <= rect.height)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.2f);
					Widgets.DrawLineHorizontal(0f, num, viewRect.width);
					GUI.color = Color.white;
					this.PreDrawPawnRow(rect2, p);
					this.DrawPawnRow(rect2, p);
					this.PostDrawPawnRow(rect2, p);
				}
				num += 30f;
			}
			Widgets.EndScrollView();
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void PreDrawPawnRow(Rect rect, Pawn p)
		{
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			Rect rect2 = new Rect(rect.x, rect.y, 165f, rect.height);
			if (p.health.summaryHealth.SummaryHealthPercent < 0.99f)
			{
				Rect rect3 = new Rect(rect2);
				rect3.xMin -= 4f;
				rect3.yMin += 4f;
				rect3.yMax -= 6f;
				Widgets.FillableBar(rect3, p.health.summaryHealth.SummaryHealthPercent, GenMapUI.OverlayHealthTex, BaseContent.ClearTex, false);
			}
			if (Mouse.IsOver(rect2))
			{
				GUI.DrawTexture(rect2, TexUI.HighlightTex);
			}
			string label;
			if (!p.RaceProps.Humanlike && p.Name != null && !p.Name.Numerical)
			{
				label = p.Name.ToStringShort.CapitalizeFirst() + ", " + p.KindLabel;
			}
			else
			{
				label = p.LabelCap;
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			Rect rect4 = rect2;
			rect4.xMin += 3f;
			Widgets.Label(rect4, label);
			Text.WordWrap = true;
			if (Widgets.ButtonInvisible(rect2, false))
			{
				Find.MainTabsRoot.EscapeCurrentTab(true);
				Find.Selector.ClearSelection();
				JumpToTargetUtility.TryJumpAndSelect(p);
				return;
			}
			TipSignal tooltip = p.GetTooltip();
			tooltip.text = "ClickToJumpTo".Translate() + "\n\n" + tooltip.text;
			TooltipHandler.TipRegion(rect2, tooltip);
		}

		protected abstract void DrawPawnRow(Rect rect, Pawn p);

		private void PostDrawPawnRow(Rect rect, Pawn p)
		{
			if (p.Downed)
			{
				GUI.color = new Color(1f, 0f, 0f, 0.5f);
				Widgets.DrawLineHorizontal(rect.x, rect.center.y, rect.width);
				GUI.color = Color.white;
			}
		}
	}
}
