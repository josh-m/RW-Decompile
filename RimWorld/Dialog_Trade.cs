using RimWorld.Planet;
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

		private const float BaseTopAreaHeight = 55f;

		private const float ColumnWidth = 120f;

		private const float FirstCommodityY = 6f;

		private const float RowInterval = 30f;

		private const float SpaceBetweenTraderNameAndTraderKind = 27f;

		private Vector2 scrollPosition = Vector2.zero;

		public static float lastCurrencyFlashTime = -100f;

		private List<Tradeable> cachedTradeables;

		private Tradeable cachedCurrencyTradeable;

		private TransferableSorterDef sorter1;

		private TransferableSorterDef sorter2;

		private bool playerIsCaravan;

		private List<Thing> playerCaravanAllPawnsAndItems;

		private bool massUsageDirty = true;

		private float cachedMassUsage;

		private bool massCapacityDirty = true;

		private float cachedMassCapacity;

		private bool daysWorthOfFoodDirty = true;

		private float cachedDaysWorthOfFood;

		protected readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);

		protected readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(1024f, (float)UI.screenHeight);
			}
		}

		private float TopAreaHeight
		{
			get
			{
				float num = 55f;
				if (this.playerIsCaravan)
				{
					num += 28f;
				}
				return num;
			}
		}

		private float MassUsage
		{
			get
			{
				if (this.massUsageDirty)
				{
					this.massUsageDirty = false;
					if (this.cachedCurrencyTradeable != null)
					{
						this.cachedTradeables.Add(this.cachedCurrencyTradeable);
					}
					this.cachedMassUsage = CollectionsMassCalculator.MassUsageLeftAfterTradeableTransfer(this.playerCaravanAllPawnsAndItems, this.cachedTradeables, true, false, false);
					if (this.cachedCurrencyTradeable != null)
					{
						this.cachedTradeables.RemoveLast<Tradeable>();
					}
				}
				return this.cachedMassUsage;
			}
		}

		private float MassCapacity
		{
			get
			{
				if (this.massCapacityDirty)
				{
					this.massCapacityDirty = false;
					if (this.cachedCurrencyTradeable != null)
					{
						this.cachedTradeables.Add(this.cachedCurrencyTradeable);
					}
					this.cachedMassCapacity = CollectionsMassCalculator.CapacityLeftAfterTradeableTransfer(this.playerCaravanAllPawnsAndItems, this.cachedTradeables);
					if (this.cachedCurrencyTradeable != null)
					{
						this.cachedTradeables.RemoveLast<Tradeable>();
					}
				}
				return this.cachedMassCapacity;
			}
		}

		private float DaysWorthOfFood
		{
			get
			{
				if (this.daysWorthOfFoodDirty)
				{
					this.daysWorthOfFoodDirty = false;
					this.cachedDaysWorthOfFood = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(this.playerCaravanAllPawnsAndItems, this.cachedTradeables);
				}
				return this.cachedDaysWorthOfFood;
			}
		}

		public Dialog_Trade(Pawn playerNegotiator, ITrader trader)
		{
			TradeSession.SetupWith(trader, playerNegotiator);
			this.SetupPlayerCaravanVariables();
			this.closeOnEscapeKey = true;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.soundAppear = SoundDefOf.CommsWindow_Open;
			this.soundClose = SoundDefOf.CommsWindow_Close;
			if (!(trader is Pawn))
			{
				this.soundAmbient = SoundDefOf.RadioComms_Ambience;
			}
			this.sorter1 = TransferableSorterDefOf.Category;
			this.sorter2 = TransferableSorterDefOf.MarketValue;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			if (TradeSession.playerNegotiator.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking) < 0.99f)
			{
				Find.WindowStack.Add(new Dialog_MessageBox("NegotiatorTalkingImpaired".Translate(new object[]
				{
					TradeSession.playerNegotiator.LabelShort
				}), null, null, null, null, null, false));
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
			select tr).ThenBy((Tradeable tr) => tr, this.sorter1.Comparer).ThenBy((Tradeable tr) => tr, this.sorter2.Comparer).ThenBy((Tradeable tr) => TransferableUIUtility.DefaultListOrderPriority(tr)).ThenBy((Tradeable tr) => tr.ThingDef.label).ThenBy(delegate(Tradeable tr)
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
			TransferableUIUtility.DoTransferableSorters(this.sorter1, this.sorter2, delegate(TransferableSorterDef x)
			{
				this.sorter1 = x;
				this.CacheTradeables();
			}, delegate(TransferableSorterDef x)
			{
				this.sorter2 = x;
				this.CacheTradeables();
			});
			float num = inRect.width - 590f;
			Rect rect = new Rect(num, 0f, inRect.width - num, this.TopAreaHeight);
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Medium;
			Rect rect2 = new Rect(0f, 0f, rect.width / 2f, rect.height);
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Label(rect2, Faction.OfPlayer.Name);
			Rect rect3 = new Rect(rect.width / 2f, 0f, rect.width / 2f, rect.height);
			Text.Anchor = TextAnchor.UpperRight;
			string text = TradeSession.trader.TraderName;
			if (Text.CalcSize(text).x > rect3.width)
			{
				Text.Font = GameFont.Small;
				text = text.Truncate(rect3.width, null);
			}
			Widgets.Label(rect3, text);
			Text.Font = GameFont.Small;
			Rect rect4 = new Rect(rect.width / 2f, 27f, rect.width / 2f, rect.height / 2f);
			Widgets.Label(rect4, TradeSession.trader.TraderKind.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = new Color(1f, 1f, 1f, 0.6f);
			Text.Font = GameFont.Tiny;
			Rect rect5 = new Rect(rect.width / 2f - 100f - 30f, 0f, 200f, rect.height);
			Text.Anchor = TextAnchor.LowerCenter;
			Widgets.Label(rect5, "PositiveBuysNegativeSells".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			if (this.playerIsCaravan)
			{
				Text.Font = GameFont.Small;
				float massUsage = this.MassUsage;
				float massCapacity = this.MassCapacity;
				Rect rect6 = rect.AtZero();
				rect6.y = 27f;
				TransferableUIUtility.DrawMassInfo(rect6, massUsage, massCapacity, "TradeMassUsageTooltip".Translate(), -9999f, false);
				CaravanUIUtility.DrawDaysWorthOfFoodInfo(new Rect(rect6.x, rect6.y + 22f, rect6.width, rect6.height), this.DaysWorthOfFood, false);
			}
			GUI.EndGroup();
			float num2 = 0f;
			if (this.cachedCurrencyTradeable != null)
			{
				float num3 = inRect.width - 16f;
				Rect rect7 = new Rect(0f, this.TopAreaHeight, num3, 30f);
				TradeUI.DrawTradeableRow(rect7, this.cachedCurrencyTradeable, 1);
				GUI.color = Color.gray;
				Widgets.DrawLineHorizontal(0f, this.TopAreaHeight + 30f - 1f, num3);
				GUI.color = Color.white;
				num2 = 30f;
			}
			Rect mainRect = new Rect(0f, this.TopAreaHeight + num2, inRect.width, inRect.height - this.TopAreaHeight - 38f - num2 - 20f);
			this.FillMainRect(mainRect);
			Rect rect8 = new Rect(inRect.width / 2f - this.AcceptButtonSize.x / 2f, inRect.height - 55f, this.AcceptButtonSize.x, this.AcceptButtonSize.y);
			if (Widgets.ButtonText(rect8, "AcceptButton".Translate(), true, false, true))
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
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmTraderShortFunds".Translate(), action, false, null));
				}
				Event.current.Use();
			}
			Rect rect9 = new Rect(rect8.x - 10f - this.OtherBottomButtonSize.x, rect8.y, this.OtherBottomButtonSize.x, this.OtherBottomButtonSize.y);
			if (Widgets.ButtonText(rect9, "ResetButton".Translate(), true, false, true))
			{
				SoundDefOf.TickLow.PlayOneShotOnCamera();
				TradeSession.deal.Reset();
				this.CacheTradeables();
				this.CountToTransferChanged();
				Event.current.Use();
			}
			Rect rect10 = new Rect(rect8.xMax + 10f, rect8.y, this.OtherBottomButtonSize.x, this.OtherBottomButtonSize.y);
			if (Widgets.ButtonText(rect10, "CancelButton".Translate(), true, false, true))
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
					int countToTransfer = this.cachedTradeables[i].CountToTransfer;
					TradeUI.DrawTradeableRow(rect, this.cachedTradeables[i], num4);
					if (countToTransfer != this.cachedTradeables[i].CountToTransfer)
					{
						this.CountToTransferChanged();
					}
				}
				num += 30f;
				num4++;
			}
			Widgets.EndScrollView();
		}

		public void FlashSilver()
		{
			Dialog_Trade.lastCurrencyFlashTime = Time.time;
		}

		public override bool CausesMessageBackground()
		{
			return true;
		}

		private void SetupPlayerCaravanVariables()
		{
			Caravan caravan = TradeSession.playerNegotiator.GetCaravan();
			if (caravan != null)
			{
				this.playerIsCaravan = true;
				this.playerCaravanAllPawnsAndItems = new List<Thing>();
				List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
				for (int i = 0; i < pawnsListForReading.Count; i++)
				{
					this.playerCaravanAllPawnsAndItems.Add(pawnsListForReading[i]);
				}
				this.playerCaravanAllPawnsAndItems.AddRange(CaravanInventoryUtility.AllInventoryItems(caravan));
			}
			else
			{
				this.playerIsCaravan = false;
			}
		}

		private void CountToTransferChanged()
		{
			this.massUsageDirty = true;
			this.massCapacityDirty = true;
			this.daysWorthOfFoodDirty = true;
		}
	}
}
