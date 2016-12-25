using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_Trade : Window
	{
		private const float TitleAreaHeight = 45f;

		private const float TopAreaHeight = 55f;

		private const float ColumnWidth = 120f;

		private const float FirstCommodityY = 6f;

		private const float RowInterval = 30f;

		private Vector2 scrollPosition = Vector2.zero;

		public static float lastCurrencyFlashTime = -100f;

		private List<Tradeable> cachedTradeables;

		private Tradeable cachedCurrencyTradeable;

		private TradeDialogSorterDef sorter1;

		private TradeDialogSorterDef sorter2;

		protected readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);

		protected readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(1024f, (float)Screen.height);
			}
		}

		public Dialog_Trade(Pawn playerNegotiator, ITrader trader)
		{
			TradeSession.SetupWith(trader, playerNegotiator);
			this.closeOnEscapeKey = true;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.soundAppear = SoundDefOf.CommsWindow_Open;
			this.soundClose = SoundDefOf.CommsWindow_Close;
			if (!(trader is Pawn))
			{
				this.soundAmbient = SoundDefOf.RadioComms_Ambience;
			}
			this.sorter1 = TradeDialogSorterDefOf.Category;
			this.sorter2 = TradeDialogSorterDefOf.MarketValue;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			if (TradeSession.playerNegotiator.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking) < 0.99f)
			{
				Find.WindowStack.Add(Dialog_NodeTree.SimpleNotifyDialog("NegotiatorTalkingImpaired".Translate(new object[]
				{
					TradeSession.playerNegotiator.LabelShort
				}), true));
			}
			this.CacheTradeables();
		}

		private void CacheTradeables()
		{
			this.cachedCurrencyTradeable = (from x in TradeSession.deal.AllTradeables
			where x.IsCurrency
			select x).FirstOrDefault<Tradeable>();
			this.cachedTradeables = (from tr in TradeSession.deal.AllTradeables
			where !tr.IsCurrency
			orderby (!tr.TraderWillTrade) ? -1 : 0 descending
			select tr).ThenBy((Tradeable tr) => tr, this.sorter1.Comparer).ThenBy((Tradeable tr) => tr, this.sorter2.Comparer).ThenBy((Tradeable tr) => tr.ListOrderPriority).ThenBy((Tradeable tr) => tr.ThingDef.label).ThenBy(delegate(Tradeable tr)
			{
				QualityCategory result;
				if (tr.AnyThing.TryGetQuality(out result))
				{
					return (int)result;
				}
				return -1;
			}).ThenBy((Tradeable tr) => tr.AnyThing.HitPoints).ToList<Tradeable>();
		}

		public override void DoWindowContents(Rect inRect)
		{
			TradeSession.deal.UpdateCurrencyCount();
			Rect position = new Rect(0f, 0f, 350f, 27f);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Tiny;
			Rect rect = new Rect(0f, 0f, 60f, 27f);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, "SortBy".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect2 = new Rect(rect.xMax + 10f, 0f, 130f, 27f);
			if (Widgets.ButtonText(rect2, this.sorter1.LabelCap, true, false, true))
			{
				this.OpenSorterChangeFloatMenu(0);
			}
			Rect rect3 = new Rect(rect2.xMax + 10f, 0f, 130f, 27f);
			if (Widgets.ButtonText(rect3, this.sorter2.LabelCap, true, false, true))
			{
				this.OpenSorterChangeFloatMenu(1);
			}
			GUI.EndGroup();
			float num = inRect.width - 590f;
			Rect position2 = new Rect(num, 0f, inRect.width - num, 55f);
			GUI.BeginGroup(position2);
			Text.Font = GameFont.Medium;
			Rect rect4 = new Rect(0f, 0f, position2.width / 2f, position2.height);
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Label(rect4, Faction.OfPlayer.Name);
			Rect rect5 = new Rect(position2.width / 2f, 0f, position2.width / 2f, position2.height);
			Text.Anchor = TextAnchor.UpperRight;
			Widgets.Label(rect5, TradeSession.trader.TraderName);
			Text.Font = GameFont.Small;
			Rect rect6 = new Rect(position2.width / 2f, position2.height / 2f, position2.width / 2f, position2.height / 2f);
			Widgets.Label(rect6, TradeSession.trader.TraderKind.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = new Color(1f, 1f, 1f, 0.6f);
			Text.Font = GameFont.Tiny;
			Rect rect7 = new Rect(position2.width / 2f - 100f - 30f, 0f, 200f, position2.height);
			Text.Anchor = TextAnchor.LowerCenter;
			Widgets.Label(rect7, "PositiveBuysNegativeSells".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			GUI.EndGroup();
			float num2 = 0f;
			if (this.cachedCurrencyTradeable != null)
			{
				float num3 = inRect.width - 16f;
				Rect rect8 = new Rect(0f, 55f, num3, 30f);
				TradeUI.DrawTradeableRow(rect8, this.cachedCurrencyTradeable, 1);
				GUI.color = Color.gray;
				Widgets.DrawLineHorizontal(0f, 84f, num3);
				GUI.color = Color.white;
				num2 = 30f;
			}
			Rect mainRect = new Rect(0f, 55f + num2, inRect.width, inRect.height - 55f - 38f - num2 - 20f);
			this.FillMainRect(mainRect);
			Rect rect9 = new Rect(inRect.width / 2f - this.AcceptButtonSize.x / 2f, inRect.height - 55f, this.AcceptButtonSize.x, this.AcceptButtonSize.y);
			if (Widgets.ButtonText(rect9, "AcceptButton".Translate(), true, false, true))
			{
				Action action = delegate
				{
					bool flag;
					if (TradeSession.deal.TryExecute(out flag))
					{
						if (flag)
						{
							SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
							Pawn pawn = TradeSession.trader as Pawn;
							if (pawn != null)
							{
								TaleRecorder.RecordTale(TaleDefOf.TradedWith, new object[]
								{
									TradeSession.playerNegotiator,
									pawn
								});
							}
							this.Close(false);
						}
						else
						{
							this.Close(true);
						}
					}
				};
				if (TradeSession.deal.DoesTraderHaveEnoughSilver())
				{
					action();
				}
				else
				{
					this.FlashSilver();
					SoundDefOf.ClickReject.PlayOneShotOnCamera();
					Find.WindowStack.Add(new Dialog_Confirm("ConfirmTraderShortFunds".Translate(), action, false, null, true));
				}
				Event.current.Use();
			}
			Rect rect10 = new Rect(rect9.x - 10f - this.OtherBottomButtonSize.x, rect9.y, this.OtherBottomButtonSize.x, this.OtherBottomButtonSize.y);
			if (Widgets.ButtonText(rect10, "ResetButton".Translate(), true, false, true))
			{
				SoundDefOf.TickLow.PlayOneShotOnCamera();
				TradeSession.deal.Reset();
				this.CacheTradeables();
				Event.current.Use();
			}
			Rect rect11 = new Rect(rect9.xMax + 10f, rect9.y, this.OtherBottomButtonSize.x, this.OtherBottomButtonSize.y);
			if (Widgets.ButtonText(rect11, "CancelButton".Translate(), true, false, true))
			{
				this.Close(true);
				Event.current.Use();
			}
		}

		public override void Close(bool doCloseSound = true)
		{
			DragSliderManager.ForceStop();
			base.Close(doCloseSound);
		}

		private void FillMainRect(Rect mainRect)
		{
			Text.Font = GameFont.Small;
			float height = 6f + (float)this.cachedTradeables.Count * 30f;
			Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
			Widgets.BeginScrollView(mainRect, ref this.scrollPosition, viewRect);
			float num = 6f;
			float num2 = this.scrollPosition.y - 30f;
			float num3 = this.scrollPosition.y + mainRect.height;
			int num4 = 0;
			for (int i = 0; i < this.cachedTradeables.Count; i++)
			{
				if (num > num2 && num < num3)
				{
					Rect rect = new Rect(0f, num, viewRect.width, 30f);
					TradeUI.DrawTradeableRow(rect, this.cachedTradeables[i], num4);
				}
				num += 30f;
				num4++;
			}
			Widgets.EndScrollView();
		}

		private void OpenSorterChangeFloatMenu(int sorterIndex)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<TradeDialogSorterDef> allDefsListForReading = DefDatabase<TradeDialogSorterDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				TradeDialogSorterDef def = allDefsListForReading[i];
				list.Add(new FloatMenuOption(def.LabelCap, delegate
				{
					if (sorterIndex == 0)
					{
						this.sorter1 = def;
					}
					else
					{
						this.sorter2 = def;
					}
					this.CacheTradeables();
				}, MenuOptionPriority.Medium, null, null, 0f, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public void FlashSilver()
		{
			Dialog_Trade.lastCurrencyFlashTime = Time.time;
		}

		public override bool CausesMessageBackground()
		{
			return true;
		}
	}
}
