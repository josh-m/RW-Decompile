using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class TransferableUIUtility
	{
		private const float AmountAreaWidth = 90f;

		private const float AmountAreaHeight = 25f;

		private const float AdjustArrowWidth = 30f;

		public const float ResourceIconSize = 27f;

		public const float SortersHeight = 27f;

		private static List<TransferableCountToTransferStoppingPoint> stoppingPoints = new List<TransferableCountToTransferStoppingPoint>();

		public static readonly Color ZeroCountColor = new Color(0.5f, 0.5f, 0.5f);

		public static readonly Texture2D FlashTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0f, 0f, 0.4f));

		private static readonly Texture2D TradeArrow = ContentFinder<Texture2D>.Get("UI/Widgets/TradeArrow", true);

		public static void DoCountAdjustInterface(Rect rect, ITransferable trad, int index, int min, int max, bool flash = false, List<TransferableCountToTransferStoppingPoint> extraStoppingPoints = null)
		{
			TransferableUIUtility.stoppingPoints.Clear();
			if (extraStoppingPoints != null)
			{
				TransferableUIUtility.stoppingPoints.AddRange(extraStoppingPoints);
			}
			for (int i = TransferableUIUtility.stoppingPoints.Count - 1; i >= 0; i--)
			{
				if (TransferableUIUtility.stoppingPoints[i].threshold != 0 && (TransferableUIUtility.stoppingPoints[i].threshold <= min || TransferableUIUtility.stoppingPoints[i].threshold >= max))
				{
					TransferableUIUtility.stoppingPoints.RemoveAt(i);
				}
			}
			bool flag = false;
			for (int j = 0; j < TransferableUIUtility.stoppingPoints.Count; j++)
			{
				if (TransferableUIUtility.stoppingPoints[j].threshold == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				TransferableUIUtility.stoppingPoints.Add(new TransferableCountToTransferStoppingPoint(0, "0", "0"));
			}
			TransferableUIUtility.DoCountAdjustInterfaceInternal(rect, trad, index, min, max, flash);
		}

		private static void DoCountAdjustInterfaceInternal(Rect rect, ITransferable trad, int index, int min, int max, bool flash)
		{
			rect = rect.Rounded();
			Rect rect2 = new Rect(rect.center.x - 45f, rect.center.y - 12.5f, 90f, 25f).Rounded();
			if (flash)
			{
				GUI.DrawTexture(rect2, TransferableUIUtility.FlashTex);
			}
			TransferableOneWay transferableOneWay = trad as TransferableOneWay;
			bool flag = transferableOneWay != null && transferableOneWay.HasAnyThing && transferableOneWay.AnyThing is Pawn && transferableOneWay.MaxCount == 1;
			if (!trad.Interactive)
			{
				GUI.color = ((trad.CountToTransfer != 0) ? Color.white : TransferableUIUtility.ZeroCountColor);
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect2, trad.CountToTransfer.ToStringCached());
			}
			else if (flag)
			{
				bool flag2 = trad.CountToTransfer != 0;
				bool flag3 = flag2;
				Widgets.Checkbox(rect2.position, ref flag3, 24f, false);
				if (flag3 != flag2)
				{
					if (flag3)
					{
						trad.SetToTransferMaxToDest();
						TransferableUIUtility.ClearEditBuffer(trad);
					}
					else
					{
						trad.SetToTransferMaxToSource();
						TransferableUIUtility.ClearEditBuffer(trad);
					}
				}
			}
			else
			{
				Rect rect3 = rect2.ContractedBy(2f);
				rect3.xMax -= 15f;
				rect3.xMin += 16f;
				int countToTransfer = trad.CountToTransfer;
				string editBuffer = trad.EditBuffer;
				Widgets.TextFieldNumeric<int>(rect3, ref countToTransfer, ref editBuffer, (float)min, (float)max);
				trad.CountToTransfer = countToTransfer;
				trad.EditBuffer = editBuffer;
			}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			if (trad.Interactive && !flag)
			{
				TransferablePositiveCountDirection positiveCountDirection = trad.PositiveCountDirection;
				if (trad.CanSetToTransferOneMoreToSource().Accepted)
				{
					Rect rect4 = new Rect(rect2.x - 30f, rect.y, 30f, rect.height);
					if (Widgets.ButtonText(rect4, "<", true, false, true))
					{
						AcceptanceReport acceptanceReport = trad.TrySetToTransferOneMoreToSource();
						if (!acceptanceReport.Accepted)
						{
							Messages.Message(acceptanceReport.Reason, MessageSound.RejectInput);
						}
						else
						{
							SoundDefOf.TickHigh.PlayOneShotOnCamera();
						}
						TransferableUIUtility.ClearEditBuffer(trad);
					}
					string label = "<<";
					int? num = null;
					int num2 = 0;
					for (int i = 0; i < TransferableUIUtility.stoppingPoints.Count; i++)
					{
						TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint = TransferableUIUtility.stoppingPoints[i];
						if (positiveCountDirection == TransferablePositiveCountDirection.Source)
						{
							if (trad.CountToTransfer < transferableCountToTransferStoppingPoint.threshold && (transferableCountToTransferStoppingPoint.threshold < num2 || !num.HasValue))
							{
								label = transferableCountToTransferStoppingPoint.leftLabel;
								num = new int?(transferableCountToTransferStoppingPoint.threshold);
							}
						}
						else if (trad.CountToTransfer > transferableCountToTransferStoppingPoint.threshold && (transferableCountToTransferStoppingPoint.threshold > num2 || !num.HasValue))
						{
							label = transferableCountToTransferStoppingPoint.leftLabel;
							num = new int?(transferableCountToTransferStoppingPoint.threshold);
						}
					}
					rect4.x -= rect4.width;
					if (Widgets.ButtonText(rect4, label, true, false, true))
					{
						if (num.HasValue)
						{
							trad.CountToTransfer = num.Value;
						}
						else
						{
							trad.SetToTransferMaxToSource();
						}
						SoundDefOf.TickHigh.PlayOneShotOnCamera();
						TransferableUIUtility.ClearEditBuffer(trad);
					}
				}
				if (trad.CanSetToTransferOneMoreToDest().Accepted)
				{
					Rect rect5 = new Rect(rect2.xMax, rect.y, 30f, rect.height);
					if (Widgets.ButtonText(rect5, ">", true, false, true))
					{
						AcceptanceReport acceptanceReport2 = trad.TrySetToTransferOneMoreToDest();
						if (!acceptanceReport2.Accepted)
						{
							Messages.Message(acceptanceReport2.Reason, MessageSound.RejectInput);
						}
						else
						{
							SoundDefOf.TickLow.PlayOneShotOnCamera();
						}
						TransferableUIUtility.ClearEditBuffer(trad);
					}
					string label2 = ">>";
					int? num3 = null;
					int num4 = 0;
					for (int j = 0; j < TransferableUIUtility.stoppingPoints.Count; j++)
					{
						TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint2 = TransferableUIUtility.stoppingPoints[j];
						if (positiveCountDirection == TransferablePositiveCountDirection.Destination)
						{
							if (trad.CountToTransfer < transferableCountToTransferStoppingPoint2.threshold && (transferableCountToTransferStoppingPoint2.threshold < num4 || !num3.HasValue))
							{
								label2 = transferableCountToTransferStoppingPoint2.rightLabel;
								num3 = new int?(transferableCountToTransferStoppingPoint2.threshold);
							}
						}
						else if (trad.CountToTransfer > transferableCountToTransferStoppingPoint2.threshold && (transferableCountToTransferStoppingPoint2.threshold > num4 || !num3.HasValue))
						{
							label2 = transferableCountToTransferStoppingPoint2.rightLabel;
							num3 = new int?(transferableCountToTransferStoppingPoint2.threshold);
						}
					}
					rect5.x += rect5.width;
					if (Widgets.ButtonText(rect5, label2, true, false, true))
					{
						if (num3.HasValue)
						{
							trad.CountToTransfer = num3.Value;
						}
						else
						{
							trad.SetToTransferMaxToDest();
						}
						SoundDefOf.TickLow.PlayOneShotOnCamera();
						TransferableUIUtility.ClearEditBuffer(trad);
					}
				}
			}
			if (trad.CountToTransfer != 0)
			{
				Rect position = new Rect(rect2.x + rect2.width / 2f - (float)(TransferableUIUtility.TradeArrow.width / 2), rect2.y + rect2.height / 2f - (float)(TransferableUIUtility.TradeArrow.height / 2), (float)TransferableUIUtility.TradeArrow.width, (float)TransferableUIUtility.TradeArrow.height);
				TransferablePositiveCountDirection positiveCountDirection2 = trad.PositiveCountDirection;
				if ((positiveCountDirection2 == TransferablePositiveCountDirection.Source && trad.CountToTransfer > 0) || (positiveCountDirection2 == TransferablePositiveCountDirection.Destination && trad.CountToTransfer < 0))
				{
					position.x += position.width;
					position.width *= -1f;
				}
				GUI.DrawTexture(position, TransferableUIUtility.TradeArrow);
			}
		}

		public static void DrawMassInfo(Rect rect, float usedMass, float availableMass, string tip, float lastMassFlashTime = -9999f, bool alignRight = false)
		{
			if (usedMass > availableMass)
			{
				GUI.color = Color.red;
			}
			else
			{
				GUI.color = Color.gray;
			}
			string text = "MassUsageInfo".Translate(new object[]
			{
				usedMass.ToString("0.##"),
				availableMass.ToString("0.##")
			});
			Vector2 vector = Text.CalcSize(text);
			Rect rect2;
			if (alignRight)
			{
				rect2 = new Rect(rect.xMax - vector.x, rect.y, vector.x, vector.y);
			}
			else
			{
				rect2 = new Rect(rect.x, rect.y, vector.x, vector.y);
			}
			bool flag = Time.time - lastMassFlashTime < 1f;
			if (flag)
			{
				GUI.DrawTexture(rect2, TransferableUIUtility.FlashTex);
			}
			Widgets.Label(rect2, text);
			TooltipHandler.TipRegion(rect2, tip);
			GUI.color = Color.white;
		}

		public static void DrawTransferableInfo(ITransferable trad, Rect idRect, Color labelColor)
		{
			if (!trad.HasAnyThing)
			{
				return;
			}
			if (Mouse.IsOver(idRect))
			{
				Widgets.DrawHighlight(idRect);
			}
			Rect rect = new Rect(0f, 0f, 27f, 27f);
			Widgets.ThingIcon(rect, trad.AnyThing, 1f);
			Widgets.InfoCardButton(40f, 0f, trad.AnyThing);
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect2 = new Rect(80f, 0f, idRect.width - 80f, idRect.height);
			Text.WordWrap = false;
			GUI.color = labelColor;
			Widgets.Label(rect2, trad.Label);
			GUI.color = Color.white;
			Text.WordWrap = true;
			ITransferable localTrad = trad;
			TooltipHandler.TipRegion(idRect, new TipSignal(delegate
			{
				if (!localTrad.HasAnyThing)
				{
					return string.Empty;
				}
				return localTrad.Label + ": " + localTrad.TipDescription;
			}, localTrad.GetHashCode()));
		}

		public static void ClearEditBuffer(ITransferable trad)
		{
			trad.EditBuffer = trad.CountToTransfer.ToStringCached();
		}

		public static float DefaultListOrderPriority(ITransferable transferable)
		{
			if (!transferable.HasAnyThing)
			{
				return 0f;
			}
			ThingDef thingDef = transferable.ThingDef;
			Tradeable tradeable = transferable as Tradeable;
			int num;
			if (tradeable != null && tradeable.IsCurrency)
			{
				num = 100;
			}
			else if (thingDef == ThingDefOf.Gold)
			{
				num = 99;
			}
			else if (thingDef.Minifiable)
			{
				num = 90;
			}
			else if (thingDef.IsApparel)
			{
				num = 80;
			}
			else if (thingDef.IsRangedWeapon)
			{
				num = 70;
			}
			else if (thingDef.IsMeleeWeapon)
			{
				num = 60;
			}
			else if (thingDef.isBodyPartOrImplant)
			{
				num = 50;
			}
			else if (thingDef.CountAsResource)
			{
				num = -10;
			}
			else
			{
				num = 20;
			}
			return (float)num;
		}

		public static void DoTransferableSorters(TransferableSorterDef sorter1, TransferableSorterDef sorter2, Action<TransferableSorterDef> sorter1Setter, Action<TransferableSorterDef> sorter2Setter)
		{
			Rect position = new Rect(0f, 0f, 350f, 27f);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Tiny;
			Rect rect = new Rect(0f, 0f, 60f, 27f);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, "SortBy".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect2 = new Rect(rect.xMax + 10f, 0f, 130f, 27f);
			if (Widgets.ButtonText(rect2, sorter1.LabelCap, true, false, true))
			{
				TransferableUIUtility.OpenSorterChangeFloatMenu(sorter1Setter);
			}
			Rect rect3 = new Rect(rect2.xMax + 10f, 0f, 130f, 27f);
			if (Widgets.ButtonText(rect3, sorter2.LabelCap, true, false, true))
			{
				TransferableUIUtility.OpenSorterChangeFloatMenu(sorter2Setter);
			}
			GUI.EndGroup();
		}

		private static void OpenSorterChangeFloatMenu(Action<TransferableSorterDef> sorterSetter)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<TransferableSorterDef> allDefsListForReading = DefDatabase<TransferableSorterDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				TransferableSorterDef def = allDefsListForReading[i];
				list.Add(new FloatMenuOption(def.LabelCap, delegate
				{
					sorterSetter(def);
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
