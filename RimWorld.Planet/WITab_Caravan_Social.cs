using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	public class WITab_Caravan_Social : WITab
	{
		private const float RowHeight = 50f;

		private const float PawnLabelHeight = 18f;

		private const float PawnLabelColumnWidth = 100f;

		private const float SpaceAroundIcon = 4f;

		private Vector2 scrollPosition;

		private float scrollViewHeight;

		private Pawn specificSocialTabForPawn;

		private List<Pawn> Pawns
		{
			get
			{
				return base.SelCaravan.PawnsListForReading;
			}
		}

		private float SpecificSocialTabWidth
		{
			get
			{
				if (this.specificSocialTabForPawn == null)
				{
					return 0f;
				}
				return 540f;
			}
		}

		public WITab_Caravan_Social()
		{
			this.labelKey = "TabCaravanSocial";
		}

		protected override void FillTab()
		{
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
			Rect rect2 = new Rect(0f, 0f, rect.width - 16f, this.scrollViewHeight);
			float num = 0f;
			Widgets.BeginScrollView(rect, ref this.scrollPosition, rect2);
			this.DoRows(ref num, rect2, rect);
			if (Event.current.type == EventType.Layout)
			{
				this.scrollViewHeight = num + 30f;
			}
			Widgets.EndScrollView();
		}

		protected override void UpdateSize()
		{
			base.UpdateSize();
			this.size.x = 243f;
			this.size.y = Mathf.Min(550f, this.PaneTopY - 30f);
		}

		protected override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			Pawn localSpecificSocialTabForPawn = this.specificSocialTabForPawn;
			if (localSpecificSocialTabForPawn != null)
			{
				Rect tabRect = base.TabRect;
				float specificSocialTabWidth = this.SpecificSocialTabWidth;
				Rect rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, specificSocialTabWidth, tabRect.height);
				Find.WindowStack.ImmediateWindow(1439870015, rect, WindowLayer.GameUI, delegate
				{
					if (localSpecificSocialTabForPawn.DestroyedOrNull())
					{
						return;
					}
					SocialCardUtility.DrawSocialCard(rect.AtZero(), localSpecificSocialTabForPawn);
					if (Widgets.CloseButtonFor(rect.AtZero()))
					{
						this.specificSocialTabForPawn = null;
						SoundDefOf.TabClose.PlayOneShotOnCamera();
					}
				}, true, false, 1f);
			}
		}

		public override void OnOpen()
		{
			base.OnOpen();
			if ((this.specificSocialTabForPawn == null || !this.Pawns.Contains(this.specificSocialTabForPawn)) && this.Pawns.Any<Pawn>())
			{
				this.specificSocialTabForPawn = this.Pawns[0];
			}
		}

		private void DoRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
		{
			List<Pawn> pawns = this.Pawns;
			if (this.specificSocialTabForPawn != null && !pawns.Contains(this.specificSocialTabForPawn))
			{
				this.specificSocialTabForPawn = null;
			}
			bool flag = false;
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (pawn.RaceProps.IsFlesh)
				{
					if (pawn.IsColonist)
					{
						if (!flag)
						{
							Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanColonists".Translate());
							flag = true;
						}
						this.DoRow(ref curY, scrollViewRect, scrollOutRect, pawn);
					}
				}
			}
			bool flag2 = false;
			for (int j = 0; j < pawns.Count; j++)
			{
				Pawn pawn2 = pawns[j];
				if (pawn2.RaceProps.IsFlesh)
				{
					if (!pawn2.IsColonist)
					{
						if (!flag2)
						{
							Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanPrisonersAndAnimals".Translate());
							flag2 = true;
						}
						this.DoRow(ref curY, scrollViewRect, scrollOutRect, pawn2);
					}
				}
			}
		}

		private void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn p)
		{
			float num = this.scrollPosition.y - 50f;
			float num2 = this.scrollPosition.y + scrollOutRect.height;
			if (curY > num && curY < num2)
			{
				this.DoRow(new Rect(0f, curY, viewRect.width, 50f), p);
			}
			curY += 50f;
		}

		private void DoRow(Rect rect, Pawn p)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = rect.AtZero();
			CaravanPeopleAndItemsTabUtility.DoAbandonButton(rect2, p, base.SelCaravan);
			rect2.width -= 24f;
			Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, p);
			rect2.width -= 24f;
			CaravanPeopleAndItemsTabUtility.DoOpenSpecificTabButton(rect2, p, ref this.specificSocialTabForPawn);
			rect2.width -= 24f;
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
			}
			Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
			Widgets.ThingIcon(rect3, p, 1f);
			Rect bgRect = new Rect(rect3.xMax + 4f, 16f, 100f, 18f);
			GenMapUI.DrawPawnLabel(p, bgRect, 1f, 100f, null, GameFont.Small, false, false);
			if (p.Downed)
			{
				GUI.color = new Color(1f, 0f, 0f, 0.5f);
				Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
				GUI.color = Color.white;
			}
			GUI.EndGroup();
		}
	}
}
