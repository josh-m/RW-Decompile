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

		private Pair<float, float> cachedDaysWorthOfFood;

		protected readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);

		protected readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(1024f, (float)UI.screenHeight);
			}
		}

		private int Tile
		{
			get
			{
				return TradeSession.playerNegotiator.Tile;
			}
		}

		private bool EnvironmentAllowsEatingVirtualPlantsNow
		{
			get
			{
				return VirtualPlantsUtility.EnvironmentAllowsEatingVirtualPlantsNowAt(this.Tile);
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
					this.cachedMassUsage = CollectionsMassCalculator.MassUsageLeftAfterTradeableTransfer(this.playerCaravanAllPawnsAndItems, this.cachedTradeables, IgnorePawnsInventoryMode.Ignore, false, false);
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

		private Pair<float, float> DaysWorthOfFood
		{
			get
			{
				if (this.daysWorthOfFoodDirty)
				{
					this.daysWorthOfFoodDirty = false;
					float first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(this.playerCaravanAllPawnsAndItems, this.cachedTradeables, this.EnvironmentAllowsEatingVirtualPlantsNow, IgnorePawnsInventoryMode.Ignore);
					this.cachedDaysWorthOfFood = new Pair<float, float>(first, DaysUntilRotCalculator.ApproxDaysUntilRotLeftAfterTradeableTransfer(this.playerCaravanAllPawnsAndItems, this.cachedTradeables, this.Tile, IgnorePawnsInventoryMode.Ignore));
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
			if (TradeSession.playerNegotiator.health.capacities.GetLevel(PawnCapacityDefOf.Talking) < 0.99f)
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
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect4 = new Rect(0f, 27f, rect.width / 2f, rect.height / 2f);
			Widgets.Label(rect4, "Negotiator".Translate() + ": " + TradeSession.playerNegotiator.LabelShort);
			Text.Anchor = TextAnchor.UpperRight;
			Rect rect5 = new Rect(rect.width / 2f, 27f, rect.width / 2f, rect.height / 2f);
			Widgets.Label(rect5, TradeSession.trader.TraderKind.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = new Color(1f, 1f, 1f, 0.6f);
			Text.Font = GameFont.Tiny;
			Rect rect6 = new Rect(rect.width / 2f - 100f - 30f, 0f, 200f, rect.height);
			Text.Anchor = TextAnchor.LowerCenter;
			Widgets.Label(rect6, "PositiveBuysNegativeSells".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			if (this.playerIsCaravan)
			{
				Text.Font = GameFont.Small;
				float massUsage = this.MassUsage;
				float massCapacity = this.MassCapacity;
				Rect rect7 = rect.AtZero();
				rect7.y = 45f;
				TransferableUIUtility.DrawMassInfo(rect7, massUsage, massCapacity, "TradeMassUsageTooltip".Translate(), -9999f, false);
				CaravanUIUtility.DrawDaysWorthOfFoodInfo(new Rect(rect7.x, rect7.y + 19f, rect7.width, rect7.height), this.DaysWorthOfFood.First, this.DaysWorthOfFood.Second, this.EnvironmentAllowsEatingVirtualPlantsNow, false, 200f);
			}
			GUI.EndGroup();
			float num2 = 0f;
			if (this.cachedCurrencyTradeable != null)
			{
				float num3 = inRect.width - 16f;
				Rect rect8 = new Rect(0f, this.TopAreaHeight, num3, 30f);
				TradeUI.DrawTradeableRow(rect8, this.cachedCurrencyTradeable, 1);
				GUI.color = Color.gray;
				Widgets.DrawLineHorizontal(0f, this.TopAreaHeight + 30f - 1f, num3);
				GUI.color = Color.white;
				num2 = 30f;
			}
			Rect mainRect = new Rect(0f, this.TopAreaHeight + num2, inRect.width, inRect.height - this.TopAreaHeight - 38f - num2 - 20f);
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
							SoundDefOf.ExecuteTrade.PlayOneShotOnCamera(null);
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
					SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmTraderShortFunds".Translate(), action, false, null));
				}
				Event.current.Use();
			}
			Rect rect10 = new Rect(rect9.x - 10f - this.OtherBottomButtonSize.x, rect9.y, this.OtherBottomButtonSize.x, this.OtherBottomButtonSize.y);
			if (Widgets.ButtonText(rect10, "ResetButton".Translate(), true, false, true))
			{
				SoundDefOf.TickLow.PlayOneShotOnCamera(null);
				TradeSession.deal.Reset();
				this.CacheTradeables();
				this.CountToTransferChanged();
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
			Widgets.BeginScrollView(mainRect, ref this.scrollPosition, viewRect, true);
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
