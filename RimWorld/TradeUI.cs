using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class TradeUI
	{
		private const float CountColumnWidth = 75f;

		private const float PriceColumnWidth = 100f;

		private const float AdjustColumnWidth = 240f;

		private const float AmountAreaWidth = 90f;

		private const float AmountAreaHeight = 25f;

		public const float TotalNumbersColumnsWidths = 590f;

		private const float AdjustArrowWidth = 30f;

		public const float ResourceIconSize = 27f;

		private static readonly Texture2D TradeArrow = ContentFinder<Texture2D>.Get("UI/Widgets/TradeArrow", true);

		private static readonly Texture2D TradeAlternativeBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.04f));

		private static readonly Texture2D SilverFlashTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0f, 0f, 0.4f));

		public static readonly Color NoTradeColor = new Color(0.5f, 0.5f, 0.5f);

		public static void DrawTradeableRow(Rect rect, Tradeable trad, int index)
		{
			if (index % 2 == 1)
			{
				GUI.DrawTexture(rect, TradeUI.TradeAlternativeBGTex);
			}
			Text.Font = GameFont.Small;
			GUI.BeginGroup(rect);
			float num = rect.width;
			int num2 = trad.CountHeldBy(Transactor.Trader);
			if (num2 != 0)
			{
				Rect rect2 = new Rect(num - 75f, 0f, 75f, rect.height);
				if (Mouse.IsOver(rect2))
				{
					Widgets.DrawHighlight(rect2);
				}
				Text.Anchor = TextAnchor.MiddleRight;
				Rect rect3 = rect2;
				rect3.xMin += 5f;
				rect3.xMax -= 5f;
				Widgets.Label(rect3, num2.ToStringCached());
				TooltipHandler.TipRegion(rect2, "TraderCount".Translate());
				Rect rect4 = new Rect(rect2.x - 100f, 0f, 100f, rect.height);
				Text.Anchor = TextAnchor.MiddleRight;
				TradeUI.DrawPrice(rect4, trad, TradeAction.PlayerBuys);
			}
			num -= 175f;
			Rect rect5 = new Rect(num - 240f, 0f, 240f, rect.height);
			TradeUI.DrawCountAdjustInterface(rect5, trad, index);
			num -= 240f;
			int num3 = trad.CountHeldBy(Transactor.Colony);
			if (num3 != 0)
			{
				Rect rect6 = new Rect(num - 100f, 0f, 100f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				TradeUI.DrawPrice(rect6, trad, TradeAction.PlayerSells);
				Rect rect7 = new Rect(rect6.x - 75f, 0f, 75f, rect.height);
				if (Mouse.IsOver(rect7))
				{
					Widgets.DrawHighlight(rect7);
				}
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect8 = rect7;
				rect8.xMin += 5f;
				rect8.xMax -= 5f;
				Widgets.Label(rect8, num3.ToStringCached());
				TooltipHandler.TipRegion(rect7, "ColonyCount".Translate());
			}
			num -= 175f;
			Rect rect9 = new Rect(0f, 0f, num, rect.height);
			if (Mouse.IsOver(rect9))
			{
				Widgets.DrawHighlight(rect9);
			}
			Rect rect10 = new Rect(0f, 0f, 27f, 27f);
			Widgets.ThingIcon(rect10, trad.AnyThing);
			Widgets.InfoCardButton(40f, 0f, trad.AnyThing);
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect11 = new Rect(80f, 0f, rect9.width - 80f, rect.height);
			Text.WordWrap = false;
			if (!trad.TraderWillTrade)
			{
				GUI.color = TradeUI.NoTradeColor;
			}
			Widgets.Label(rect11, trad.Label);
			GUI.color = Color.white;
			Text.WordWrap = true;
			Tradeable localTrad = trad;
			TooltipHandler.TipRegion(rect9, new TipSignal(delegate
			{
				if (!localTrad.HasAnyThing)
				{
					return string.Empty;
				}
				return localTrad.Label + ": " + localTrad.TipDescription;
			}, localTrad.GetHashCode()));
			GenUI.ResetLabelAlign();
			GUI.EndGroup();
		}

		public static void DrawIcon(float x, float y, ThingDef thingDef)
		{
			Rect rect = new Rect(x, y, 27f, 27f);
			Color color = GUI.color;
			GUI.color = thingDef.graphicData.color;
			GUI.DrawTexture(rect, thingDef.uiIcon);
			GUI.color = color;
			TooltipHandler.TipRegion(rect, new TipSignal(() => thingDef.LabelCap + ": " + thingDef.description, thingDef.GetHashCode()));
		}

		private static void DrawPrice(Rect rect, Tradeable trad, TradeAction action)
		{
			if (trad.IsCurrency || !trad.TraderWillTrade)
			{
				return;
			}
			rect = rect.Rounded();
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			float num = trad.PriceFor(action);
			PriceType pType = PriceTypeUtlity.ClosestPriceType(num / trad.BaseMarketValue);
			switch (pType)
			{
			case PriceType.VeryCheap:
				GUI.color = new Color(0f, 1f, 0f);
				break;
			case PriceType.Cheap:
				GUI.color = new Color(0.5f, 1f, 0.5f);
				break;
			case PriceType.Normal:
				GUI.color = Color.white;
				break;
			case PriceType.Expensive:
				GUI.color = new Color(1f, 0.5f, 0.5f);
				break;
			case PriceType.Exorbitant:
				GUI.color = new Color(1f, 0f, 0f);
				break;
			}
			string label = "$" + num.ToString("F2");
			Func<string> textGetter = delegate
			{
				if (!trad.HasAnyThing)
				{
					return string.Empty;
				}
				return ((action != TradeAction.PlayerBuys) ? "SellPriceDesc".Translate() : "BuyPriceDesc".Translate()) + "\n\n" + "PriceTypeDesc".Translate(new object[]
				{
					("PriceType" + pType).Translate()
				});
			};
			TooltipHandler.TipRegion(rect, new TipSignal(textGetter, trad.GetHashCode() * 297));
			Rect rect2 = new Rect(rect);
			rect2.xMax -= 5f;
			rect2.xMin += 5f;
			if (Text.Anchor == TextAnchor.MiddleLeft)
			{
				rect2.xMax += 300f;
			}
			if (Text.Anchor == TextAnchor.MiddleRight)
			{
				rect2.xMin -= 300f;
			}
			Widgets.Label(rect2, label);
			GUI.color = Color.white;
		}

		private static void DrawCountAdjustInterface(Rect rect, Tradeable trad, int index)
		{
			if (!trad.TraderWillTrade)
			{
				TradeUI.DrawWillNotTradeIndication(rect, trad);
				return;
			}
			rect = rect.Rounded();
			Rect rect2 = new Rect(rect.center.x - 45f, rect.center.y - 12.5f, 90f, 25f).Rounded();
			if (Time.time - Dialog_Trade.lastCurrencyFlashTime < 1f && trad.IsCurrency)
			{
				GUI.DrawTexture(rect2, TradeUI.SilverFlashTex);
			}
			if (trad.IsCurrency)
			{
				GUI.color = ((trad.countToDrop != 0) ? Color.white : TradeUI.NoTradeColor);
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect2, trad.countToDrop.ToStringCached());
			}
			else
			{
				Rect rect3 = rect2.ContractedBy(2f);
				rect3.xMax -= 15f;
				rect3.xMin += 16f;
				Widgets.TextFieldNumeric<int>(rect3, ref trad.countToDrop, ref trad.editBuffer, (float)(-(float)trad.CountHeldBy(Transactor.Colony)), (float)trad.CountHeldBy(Transactor.Trader));
			}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			if (!trad.IsCurrency)
			{
				if (trad.CanSetToDropOneMore().Accepted)
				{
					Rect rect4 = new Rect(rect2.x - 30f, rect.y, 30f, rect.height);
					if (Widgets.ButtonText(rect4, "<", true, false, true))
					{
						AcceptanceReport acceptanceReport = trad.TrySetToDropOneMore();
						if (!acceptanceReport.Accepted)
						{
							Messages.Message(acceptanceReport.Reason, MessageSound.RejectInput);
						}
						else
						{
							SoundDefOf.TickHigh.PlayOneShotOnCamera();
						}
						TradeUI.ClearEditBuffer(trad);
					}
					rect4.x -= rect4.width;
					if (Widgets.ButtonText(rect4, "<<", true, false, true))
					{
						trad.SetToDropMax();
						SoundDefOf.TickHigh.PlayOneShotOnCamera();
						TradeUI.ClearEditBuffer(trad);
					}
				}
				if (trad.CanSetToLaunchOneMore().Accepted)
				{
					Rect rect5 = new Rect(rect2.xMax, rect.y, 30f, rect.height);
					if (Widgets.ButtonText(rect5, ">", true, false, true))
					{
						AcceptanceReport acceptanceReport2 = trad.TrySetToLaunchOneMore();
						if (!acceptanceReport2.Accepted)
						{
							Messages.Message(acceptanceReport2.Reason, MessageSound.RejectInput);
						}
						else
						{
							SoundDefOf.TickLow.PlayOneShotOnCamera();
						}
						TradeUI.ClearEditBuffer(trad);
					}
					rect5.x += rect5.width;
					if (Widgets.ButtonText(rect5, ">>", true, false, true))
					{
						trad.SetToLaunchMax();
						SoundDefOf.TickLow.PlayOneShotOnCamera();
						TradeUI.ClearEditBuffer(trad);
					}
				}
			}
			if (trad.countToDrop != 0)
			{
				Rect position = new Rect(rect2.x + rect2.width / 2f - (float)(TradeUI.TradeArrow.width / 2), rect2.y + rect2.height / 2f - (float)(TradeUI.TradeArrow.height / 2), (float)TradeUI.TradeArrow.width, (float)TradeUI.TradeArrow.height);
				if (trad.countToDrop > 0)
				{
					position.x += position.width;
					position.width *= -1f;
				}
				GUI.DrawTexture(position, TradeUI.TradeArrow);
			}
		}

		private static void DrawWillNotTradeIndication(Rect rect, Tradeable trad)
		{
			rect = rect.Rounded();
			GUI.color = TradeUI.NoTradeColor;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, "TraderWillNotTrade".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
		}

		private static void ClearEditBuffer(Tradeable trad)
		{
			trad.editBuffer = trad.countToDrop.ToStringCached();
		}
	}
}
