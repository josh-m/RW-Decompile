using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_GeneralChoice : Window
	{
		private DialogChoiceConfig config;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(600f, 400f);
			}
		}

		public Dialog_GeneralChoice(DialogChoiceConfig config)
		{
			this.config = config;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.closeOnEscapeKey = false;
			if (config.buttonAAction == null)
			{
				config.buttonAText = "OK".Translate();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Widgets.Label(new Rect(0f, 0f, inRect.width, inRect.height), this.config.text);
			if (this.config.buttonAText != string.Empty && Widgets.ButtonText(new Rect(0f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), this.config.buttonAText, true, false, true))
			{
				if (this.config.buttonAAction != null)
				{
					this.config.buttonAAction();
				}
				this.Close(true);
			}
			if (this.config.buttonBText != string.Empty && Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), this.config.buttonBText, true, false, true))
			{
				if (this.config.buttonBAction != null)
				{
					this.config.buttonBAction();
				}
				this.Close(true);
			}
		}
	}
}
