using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	public class WITab_Caravan_Health : WITab
	{
		private const float RowHeight = 50f;

		private const float PawnLabelHeight = 18f;

		private const float PawnLabelColumnWidth = 100f;

		private const float SpaceAroundIcon = 4f;

		private const float PawnCapacityColumnWidth = 100f;

		private Vector2 scrollPosition;

		private float scrollViewHeight;

		private Pawn specificHealthTabForPawn;

		private bool compactMode;

		private static List<PawnCapacityDef> capacitiesToDisplay = new List<PawnCapacityDef>();

		private List<Pawn> Pawns
		{
			get
			{
				return base.SelCaravan.PawnsListForReading;
			}
		}

		private List<PawnCapacityDef> CapacitiesToDisplay
		{
			get
			{
				WITab_Caravan_Health.capacitiesToDisplay.Clear();
				List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (allDefsListForReading[i].showOnCaravanHealthTab)
					{
						WITab_Caravan_Health.capacitiesToDisplay.Add(allDefsListForReading[i]);
					}
				}
				WITab_Caravan_Health.capacitiesToDisplay.SortBy((PawnCapacityDef x) => x.listOrder);
				return WITab_Caravan_Health.capacitiesToDisplay;
			}
		}

		private float SpecificHealthTabWidth
		{
			get
			{
				if (this.specificHealthTabForPawn == null)
				{
					return 0f;
				}
				return 630f;
			}
		}

		public WITab_Caravan_Health()
		{
			this.labelKey = "TabCaravanHealth";
		}

		protected override void FillTab()
		{
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
			Rect rect2 = new Rect(0f, 0f, rect.width - 16f, this.scrollViewHeight);
			float num = 0f;
			Widgets.BeginScrollView(rect, ref this.scrollPosition, rect2);
			this.DoColumnHeaders(ref num);
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
			this.size = this.GetRawSize(false);
			if (this.size.x + this.SpecificHealthTabWidth > (float)UI.screenWidth)
			{
				this.compactMode = true;
				this.size = this.GetRawSize(true);
			}
			else
			{
				this.compactMode = false;
			}
		}

		protected override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			Pawn localSpecificHealthTabForPawn = this.specificHealthTabForPawn;
			if (localSpecificHealthTabForPawn != null)
			{
				Rect tabRect = base.TabRect;
				float specificHealthTabWidth = this.SpecificHealthTabWidth;
				Rect rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, specificHealthTabWidth, tabRect.height);
				Find.WindowStack.ImmediateWindow(1439870015, rect, WindowLayer.GameUI, delegate
				{
					if (localSpecificHealthTabForPawn.DestroyedOrNull())
					{
						return;
					}
					Rect outRect = new Rect(0f, 20f, rect.width, rect.height - 20f);
					HealthCardUtility.DrawPawnHealthCard(outRect, localSpecificHealthTabForPawn, false, true, localSpecificHealthTabForPawn);
					if (Widgets.CloseButtonFor(rect.AtZero()))
					{
						this.specificHealthTabForPawn = null;
						SoundDefOf.TabClose.PlayOneShotOnCamera();
					}
				}, true, false, 1f);
			}
		}

		private void DoColumnHeaders(ref float curY)
		{
			if (!this.compactMode)
			{
				float num = 135f;
				Text.Anchor = TextAnchor.UpperCenter;
				GUI.color = Widgets.SeparatorLabelColor;
				Widgets.Label(new Rect(num, 3f, 100f, 100f), "Pain".Translate());
				List<PawnCapacityDef> list = this.CapacitiesToDisplay;
				for (int i = 0; i < list.Count; i++)
				{
					num += 100f;
					Widgets.Label(new Rect(num, 3f, 100f, 100f), list[i].LabelCap);
				}
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
		}

		private void DoRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
		{
			List<Pawn> pawns = this.Pawns;
			if (this.specificHealthTabForPawn != null && !pawns.Contains(this.specificHealthTabForPawn))
			{
				this.specificHealthTabForPawn = null;
			}
			bool flag = false;
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
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
			bool flag2 = false;
			for (int j = 0; j < pawns.Count; j++)
			{
				Pawn pawn2 = pawns[j];
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

		private Vector2 GetRawSize(bool compactMode)
		{
			float num = 100f;
			if (!compactMode)
			{
				num += 100f;
				num += (float)this.CapacitiesToDisplay.Count * 100f;
			}
			Vector2 result;
			result.x = 127f + num + 16f;
			result.y = Mathf.Min(550f, this.PaneTopY - 30f);
			return result;
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
			CaravanPeopleAndItemsTabUtility.DoOpenSpecificTabButton(rect2, p, ref this.specificHealthTabForPawn);
			rect2.width -= 24f;
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
			}
			Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
			Widgets.ThingIcon(rect3, p, 1f);
			Rect bgRect = new Rect(rect3.xMax + 4f, 16f, 100f, 18f);
			GenMapUI.DrawPawnLabel(p, bgRect, 1f, 100f, null, GameFont.Small, false, false);
			if (!this.compactMode)
			{
				float num = bgRect.xMax;
				if (p.RaceProps.IsFlesh)
				{
					Rect rect4 = new Rect(num, 0f, 100f, 50f);
					this.DoPain(rect4, p);
				}
				List<PawnCapacityDef> list = this.CapacitiesToDisplay;
				for (int i = 0; i < list.Count; i++)
				{
					num += 100f;
					Rect rect5 = new Rect(num, 0f, 100f, 50f);
					if (!p.RaceProps.Humanlike || list[i].showOnHumanlikes)
					{
						if (!p.RaceProps.Animal || list[i].showOnAnimals)
						{
							if (!p.RaceProps.IsMechanoid || list[i].showOnMechanoids)
							{
								if (PawnCapacityUtility.BodyCanEverDoActivity(p.RaceProps.body, list[i]))
								{
									this.DoCapacity(rect5, p, list[i]);
								}
							}
						}
					}
				}
			}
			if (p.Downed)
			{
				GUI.color = new Color(1f, 0f, 0f, 0.5f);
				Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
				GUI.color = Color.white;
			}
			GUI.EndGroup();
		}

		private void DoPain(Rect rect, Pawn pawn)
		{
			Pair<string, Color> painLabel = HealthCardUtility.GetPainLabel(pawn);
			string painTip = HealthCardUtility.GetPainTip(pawn);
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			GUI.color = painLabel.Second;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, painLabel.First);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			TooltipHandler.TipRegion(rect, painTip);
		}

		private void DoCapacity(Rect rect, Pawn pawn, PawnCapacityDef capacity)
		{
			Pair<string, Color> efficiencyLabel = HealthCardUtility.GetEfficiencyLabel(pawn, capacity);
			string pawnCapacityTip = HealthCardUtility.GetPawnCapacityTip(pawn, capacity);
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			GUI.color = efficiencyLabel.Second;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, efficiencyLabel.First);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			TooltipHandler.TipRegion(rect, pawnCapacityTip);
		}
	}
}
