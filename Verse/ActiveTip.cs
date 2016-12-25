using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class ActiveTip
	{
		private const int TipMargin = 4;

		public TipSignal signal;

		public double firstTriggerTime;

		public int lastTriggerFrame;

		public static readonly Texture2D TooltipBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/TooltipBG", true);

		private string FinalText
		{
			get
			{
				string text;
				if (this.signal.textGetter != null)
				{
					try
					{
						text = this.signal.textGetter();
					}
					catch (Exception ex)
					{
						Log.Error(ex.ToString());
						text = "Error getting tip text.";
					}
				}
				else
				{
					text = this.signal.text;
				}
				return text.TrimEnd(new char[0]);
			}
		}

		public Rect TipRect
		{
			get
			{
				float num = (float)((!LanguageDatabase.activeLanguage.info.canBeTiny) ? 320 : 260);
				string finalText = this.FinalText;
				Vector2 vector = Text.CalcSize(finalText);
				if (vector.x > num)
				{
					vector.x = num;
					vector.y = Text.CalcHeight(finalText, vector.x);
				}
				Rect rect = new Rect(0f, 0f, vector.x, vector.y);
				rect = rect.ContractedBy(-4f);
				return rect;
			}
		}

		public ActiveTip(TipSignal signal)
		{
			this.signal = signal;
		}

		public ActiveTip(ActiveTip cloneSource)
		{
			this.signal = cloneSource.signal;
			this.firstTriggerTime = cloneSource.firstTriggerTime;
			this.lastTriggerFrame = cloneSource.lastTriggerFrame;
		}

		public float DrawTooltip(Vector2 pos)
		{
			Text.Font = GameFont.Small;
			string finalText = this.FinalText;
			Rect bgRect = this.TipRect;
			bgRect.position = pos;
			Find.WindowStack.ImmediateWindow(153 * this.signal.uniqueId + 62346, bgRect, WindowLayer.Super, delegate
			{
				Rect rect = bgRect.AtZero();
				Widgets.DrawAtlas(rect, ActiveTip.TooltipBGAtlas);
				Text.Font = GameFont.Small;
				Widgets.Label(rect.ContractedBy(4f), finalText);
			}, false, false, 1f);
			return bgRect.height;
		}
	}
}
