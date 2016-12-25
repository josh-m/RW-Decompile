using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_Message : Window
	{
		private const float TitleHeight = 42f;

		private string title;

		private string text;

		private static Vector2 scrollPosition = Vector2.zero;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(640f, 460f);
			}
		}

		public Dialog_Message(string text, string title = null)
		{
			this.text = text;
			this.title = title;
			this.forcePause = true;
			this.closeOnEscapeKey = true;
			this.absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = inRect.y;
			if (!this.title.NullOrEmpty())
			{
				Text.Font = GameFont.Medium;
				Widgets.Label(new Rect(0f, num, inRect.width, 42f), this.title);
				num += 42f;
			}
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(inRect.x, num, inRect.width, inRect.height - 40f - num);
			float width = outRect.width - 16f;
			Rect viewRect = new Rect(0f, 0f, width, Text.CalcHeight(this.text, width));
			Widgets.BeginScrollView(outRect, ref Dialog_Message.scrollPosition, viewRect);
			Widgets.Label(new Rect(0f, 0f, viewRect.width, viewRect.height), this.text);
			Widgets.EndScrollView();
			if (Widgets.ButtonText(new Rect(inRect.width / 2f - 80f, inRect.height - 35f, 160f, 35f), "OK".Translate(), true, false, true))
			{
				this.Close(true);
			}
		}
	}
}
