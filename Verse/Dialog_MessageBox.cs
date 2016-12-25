using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_MessageBox : Window
	{
		private const float TitleHeight = 42f;

		private const float ButtonHeight = 35f;

		public string text;

		public string title;

		public string buttonAText;

		public Action buttonAAction;

		public bool buttonADestructive;

		public string buttonBText;

		public Action buttonBAction;

		public float interactionDelay;

		private Vector2 scrollPosition = Vector2.zero;

		private float creationRealTime = -1f;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(640f, 460f);
			}
		}

		private float TimeUntilInteractive
		{
			get
			{
				return this.interactionDelay - (Time.realtimeSinceStartup - this.creationRealTime);
			}
		}

		private bool InteractionDelayExpired
		{
			get
			{
				return this.TimeUntilInteractive <= 0f;
			}
		}

		public Dialog_MessageBox(string text, string buttonAText = null, Action buttonAAction = null, string buttonBText = null, Action buttonBAction = null, string title = null, bool buttonADestructive = false)
		{
			this.text = text;
			this.buttonAText = buttonAText;
			this.buttonAAction = buttonAAction;
			this.buttonADestructive = buttonADestructive;
			this.buttonBText = buttonBText;
			this.buttonBAction = buttonBAction;
			this.title = title;
			if (buttonAText.NullOrEmpty())
			{
				this.buttonAText = "OK".Translate();
			}
			if (buttonAAction == null)
			{
				this.closeOnEscapeKey = true;
			}
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.closeOnEscapeKey = false;
			this.creationRealTime = RealTime.LastRealTime;
		}

		public static Dialog_MessageBox CreateConfirmation(string text, Action confirmedAct, bool destructive = false, string title = null)
		{
			return new Dialog_MessageBox(text, "Confirm".Translate(), confirmedAct, "GoBack".Translate(), null, title, destructive);
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
			Rect outRect = new Rect(inRect.x, num, inRect.width, inRect.height - 35f - 5f - num);
			float width = outRect.width - 16f;
			Rect viewRect = new Rect(0f, 0f, width, Text.CalcHeight(this.text, width));
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
			Widgets.Label(new Rect(0f, 0f, viewRect.width, viewRect.height), this.text);
			Widgets.EndScrollView();
			if (this.buttonADestructive)
			{
				GUI.color = new Color(1f, 0.3f, 0.35f);
			}
			string label = (!this.InteractionDelayExpired) ? (this.buttonAText + "(" + Mathf.Ceil(this.TimeUntilInteractive).ToString("F0") + ")") : this.buttonAText;
			float width2 = inRect.width / 2f - 20f;
			if (Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, width2, 35f), label, true, false, true) && this.InteractionDelayExpired)
			{
				if (this.buttonAAction != null)
				{
					this.buttonAAction();
				}
				this.Close(true);
			}
			GUI.color = Color.white;
			if (this.buttonBText != null && Widgets.ButtonText(new Rect(0f, inRect.height - 35f, width2, 35f), this.buttonBText, true, false, true))
			{
				if (this.buttonBAction != null)
				{
					this.buttonBAction();
				}
				this.Close(true);
			}
		}
	}
}
