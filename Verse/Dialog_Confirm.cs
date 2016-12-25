using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_Confirm : Window
	{
		private const float TitleHeight = 40f;

		private string text;

		private Action confirmedAction;

		private bool destructiveAction;

		private string title;

		public string confirmLabel;

		public string goBackLabel;

		public bool showGoBack;

		public float interactionDelay;

		private Vector2 scrollPos = default(Vector2);

		private float scrollViewHeight;

		private float createRealTime;

		public override Vector2 InitialSize
		{
			get
			{
				float num = 300f;
				if (this.title != null)
				{
					num += 40f;
				}
				return new Vector2(500f, num);
			}
		}

		private float TimeUntilInteractive
		{
			get
			{
				return this.interactionDelay - (Time.realtimeSinceStartup - this.createRealTime);
			}
		}

		private bool InteractionDelayExpired
		{
			get
			{
				return this.TimeUntilInteractive <= 0f;
			}
		}

		public Dialog_Confirm(string text, Action confirmedAction, bool destructive = false, string title = null, bool showGoBack = true)
		{
			this.text = text;
			this.confirmedAction = confirmedAction;
			this.destructiveAction = destructive;
			this.title = title;
			this.showGoBack = showGoBack;
			this.confirmLabel = "Confirm".Translate();
			this.goBackLabel = "GoBack".Translate();
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.closeOnEscapeKey = showGoBack;
			this.createRealTime = Time.realtimeSinceStartup;
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = inRect.y;
			if (!this.title.NullOrEmpty())
			{
				Text.Font = GameFont.Medium;
				Widgets.Label(new Rect(0f, num, inRect.width, 40f), this.title);
				num += 40f;
			}
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(0f, num, inRect.width, inRect.height - 45f - num);
			Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, this.scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref this.scrollPos, viewRect);
			Widgets.Label(new Rect(0f, 0f, viewRect.width, this.scrollViewHeight), this.text);
			if (Event.current.type == EventType.Layout)
			{
				this.scrollViewHeight = Text.CalcHeight(this.text, viewRect.width);
			}
			Widgets.EndScrollView();
			if (this.destructiveAction)
			{
				GUI.color = new Color(1f, 0.3f, 0.35f);
			}
			string label = (!this.InteractionDelayExpired) ? (this.confirmLabel + "(" + Mathf.Ceil(this.TimeUntilInteractive).ToString("F0") + ")") : this.confirmLabel;
			if (Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), label, true, false, true) && this.InteractionDelayExpired)
			{
				this.confirmedAction();
				this.Close(true);
			}
			GUI.color = Color.white;
			if (this.showGoBack && Widgets.ButtonText(new Rect(0f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), this.goBackLabel, true, false, true))
			{
				this.Close(true);
			}
		}
	}
}
