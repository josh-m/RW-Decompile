using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Listing_Standard : Listing
	{
		private const float DefSelectionLineHeight = 21f;

		public Listing_Standard(Rect rect, GameFont font) : base(rect)
		{
			Text.Font = font;
		}

		public Listing_Standard(Rect rect) : base(rect)
		{
			Text.Font = GameFont.Small;
		}

		public void Label(string label)
		{
			float height = Text.CalcHeight(label, base.ColumnWidth);
			Rect rect = base.GetRect(height);
			Widgets.Label(rect, label);
			base.Gap(this.verticalSpacing);
		}

		public void LabelDouble(string leftLabel, string rightLabel)
		{
			float num = base.ColumnWidth / 2f;
			float width = base.ColumnWidth - num;
			float a = Text.CalcHeight(leftLabel, num);
			float b = Text.CalcHeight(rightLabel, width);
			float height = Mathf.Max(a, b);
			Rect rect = base.GetRect(height);
			Widgets.Label(rect.LeftHalf(), leftLabel);
			Widgets.Label(rect.RightHalf(), rightLabel);
			base.Gap(this.verticalSpacing);
		}

		public bool RadioButton(string label, bool active, float tabIn = 0f)
		{
			float lineHeight = Text.LineHeight;
			base.NewColumnIfNeeded(lineHeight);
			bool result = Widgets.RadioButtonLabeled(new Rect(this.curX + tabIn, this.curY, base.ColumnWidth - tabIn, lineHeight), label, active);
			this.curY += lineHeight + this.verticalSpacing;
			return result;
		}

		public void CheckboxLabeled(string label, ref bool checkOn, string tooltip = null)
		{
			float lineHeight = Text.LineHeight;
			Rect rect = base.GetRect(lineHeight);
			if (!tooltip.NullOrEmpty())
			{
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				TooltipHandler.TipRegion(rect, tooltip);
			}
			Widgets.CheckboxLabeled(rect, label, ref checkOn, false);
			base.Gap(this.verticalSpacing);
		}

		public bool CheckboxLabeledSelectable(string label, ref bool selected, ref bool checkOn)
		{
			float lineHeight = Text.LineHeight;
			Rect rect = base.GetRect(lineHeight);
			bool result = Widgets.CheckboxLabeledSelectable(rect, label, ref selected, ref checkOn);
			base.Gap(this.verticalSpacing);
			return result;
		}

		public bool ButtonText(string label, string highlightTag = null)
		{
			Rect rect = base.GetRect(30f);
			bool result = Widgets.ButtonText(rect, label, true, false, true);
			if (highlightTag != null)
			{
				UIHighlighter.HighlightOpportunity(rect, highlightTag);
			}
			base.Gap(this.verticalSpacing);
			return result;
		}

		public bool ButtonTextLabeled(string label, string buttonLabel)
		{
			Rect rect = base.GetRect(30f);
			Widgets.Label(rect.LeftHalf(), label);
			bool result = Widgets.ButtonText(rect.RightHalf(), buttonLabel, true, false, true);
			base.Gap(this.verticalSpacing);
			return result;
		}

		public bool ButtonImage(Texture2D tex, float width, float height)
		{
			base.NewColumnIfNeeded(height);
			bool result = Widgets.ButtonImage(new Rect(this.curX, this.curY, width, height), tex);
			base.Gap(height + this.verticalSpacing);
			return result;
		}

		public string TextEntry(string text, int lineCount = 1)
		{
			Rect rect = base.GetRect(Text.LineHeight * (float)lineCount);
			string result;
			if (lineCount == 1)
			{
				result = Widgets.TextField(rect, text);
			}
			else
			{
				result = Widgets.TextArea(rect, text);
			}
			base.Gap(this.verticalSpacing);
			return result;
		}

		public string TextEntryLabeled(string label, string text, int lineCount = 1)
		{
			Rect rect = base.GetRect(Text.LineHeight * (float)lineCount);
			string result = Widgets.TextEntryLabeled(rect, label, text);
			base.Gap(this.verticalSpacing);
			return result;
		}

		public void TextFieldNumeric<T>(ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
		{
			Rect rect = base.GetRect(Text.LineHeight);
			Widgets.TextFieldNumeric<T>(rect, ref val, ref buffer, min, max);
			base.Gap(this.verticalSpacing);
		}

		public void TextFieldNumericLabeled<T>(string label, ref int val, ref string buffer, float min = 0f, float max = 1E+09f)
		{
			Rect rect = base.GetRect(Text.LineHeight);
			Widgets.TextFieldNumericLabeled<int>(rect, label, ref val, ref buffer, min, max);
			base.Gap(this.verticalSpacing);
		}

		public void IntRange(ref IntRange range, int min, int max)
		{
			Rect rect = base.GetRect(Text.LineHeight);
			Widgets.IntRange(rect, (int)base.CurHeight, ref range, min, max, null, 0);
			base.Gap(this.verticalSpacing);
		}

		public float Slider(float val, float min, float max)
		{
			Rect rect = base.GetRect(30f);
			float result = Widgets.HorizontalSlider(rect, val, min, max, false, null, null, null, -1f);
			base.Gap(this.verticalSpacing);
			return result;
		}

		public void IntAdjuster(ref int val, int countChange, int min = 0)
		{
			Rect rect = base.GetRect(24f);
			rect.width = 42f;
			if (Widgets.ButtonText(rect, "-" + countChange, true, false, true))
			{
				SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
				val -= countChange;
				if (val < min)
				{
					val = min;
				}
			}
			rect.x += rect.width + 2f;
			if (Widgets.ButtonText(rect, "+" + countChange, true, false, true))
			{
				SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
				val += countChange;
				if (val < min)
				{
					val = min;
				}
			}
			base.Gap(this.verticalSpacing);
		}

		public void IntSetter(ref int val, int target, string label, float width = 42f)
		{
			Rect rect = base.GetRect(24f);
			if (Widgets.ButtonText(rect, label, true, false, true))
			{
				SoundDefOf.TickLow.PlayOneShotOnCamera();
				val = target;
			}
			base.Gap(this.verticalSpacing);
		}

		public bool SelectableDef(string name, bool selected, Action deleteCallback)
		{
			Text.Font = GameFont.Tiny;
			float width = this.listingRect.width - 21f;
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect = new Rect(this.curX, this.curY, width, 21f);
			if (selected)
			{
				Widgets.DrawHighlight(rect);
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawBox(rect, 1);
			}
			Text.WordWrap = false;
			Widgets.Label(rect, name);
			Text.WordWrap = true;
			if (deleteCallback != null)
			{
				Rect butRect = new Rect(rect.xMax, rect.y, 21f, 21f);
				if (Widgets.ButtonImage(butRect, TexButton.DeleteX))
				{
					deleteCallback();
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
			this.curY += 21f;
			return Widgets.ButtonInvisible(rect, false);
		}

		public void LabelCheckboxDebug(string label, ref bool checkOn)
		{
			Text.Font = GameFont.Tiny;
			base.NewColumnIfNeeded(22f);
			Widgets.CheckboxLabeled(new Rect(this.curX, this.curY, base.ColumnWidth, 22f), label, ref checkOn, false);
			base.Gap(22f + this.verticalSpacing);
		}

		public bool ButtonDebug(string label)
		{
			Text.Font = GameFont.Tiny;
			base.NewColumnIfNeeded(22f);
			bool wordWrap = Text.WordWrap;
			Text.WordWrap = false;
			bool result = Widgets.ButtonText(new Rect(this.curX, this.curY, base.ColumnWidth, 22f), label, true, false, true);
			Text.WordWrap = wordWrap;
			base.Gap(22f + this.verticalSpacing);
			return result;
		}
	}
}
