using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Page : Window
	{
		public const float TitleAreaHeight = 45f;

		public const float BottomButHeight = 38f;

		public Page prev;

		public Page next;

		public Action nextAct;

		public static readonly Vector2 StandardSize = new Vector2(1020f, 764f);

		protected static readonly Vector2 BottomButSize = new Vector2(150f, 38f);

		public override Vector2 InitialSize
		{
			get
			{
				return Page.StandardSize;
			}
		}

		public virtual string PageTitle
		{
			get
			{
				return null;
			}
		}

		public Page()
		{
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
		}

		protected void DrawPageTitle(Rect rect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, 0f, rect.width, 45f), this.PageTitle);
			Text.Font = GameFont.Small;
		}

		protected Rect GetMainRect(Rect rect, float extraTopSpace = 0f, bool ignoreTitle = false)
		{
			float num = 0f;
			if (!ignoreTitle)
			{
				num = 45f + extraTopSpace;
			}
			return new Rect(0f, num, rect.width, rect.height - 38f - num - 17f);
		}

		protected void DoBottomButtons(Rect rect, string nextLabel = null, string midLabel = null, Action midAct = null, bool showNext = true)
		{
			float y = rect.height - 38f;
			Text.Font = GameFont.Small;
			string label = "Back".Translate();
			Rect rect2 = new Rect(rect.x, y, Page.BottomButSize.x, Page.BottomButSize.y);
			if (Widgets.ButtonText(rect2, label, true, false, true) && this.CanDoBack())
			{
				this.DoBack();
			}
			if (showNext)
			{
				if (nextLabel.NullOrEmpty())
				{
					nextLabel = "Next".Translate();
				}
				Rect rect3 = new Rect(rect.x + rect.width - Page.BottomButSize.x, y, Page.BottomButSize.x, Page.BottomButSize.y);
				if (Widgets.ButtonText(rect3, nextLabel, true, false, true) && this.CanDoNext())
				{
					this.DoNext();
				}
				UIHighlighter.HighlightOpportunity(rect3, "NextPage");
			}
			if (midAct != null)
			{
				Rect rect4 = new Rect(rect.x + rect.width / 2f - Page.BottomButSize.x / 2f, y, Page.BottomButSize.x, Page.BottomButSize.y);
				if (Widgets.ButtonText(rect4, midLabel, true, false, true))
				{
					midAct();
				}
			}
		}

		protected virtual bool CanDoBack()
		{
			return !TutorSystem.TutorialMode || TutorSystem.AllowAction("GotoPrevPage");
		}

		protected virtual bool CanDoNext()
		{
			return !TutorSystem.TutorialMode || TutorSystem.AllowAction("GotoNextPage");
		}

		protected virtual void DoNext()
		{
			if (this.next != null)
			{
				Find.WindowStack.Add(this.next);
			}
			if (this.nextAct != null)
			{
				this.nextAct();
			}
			TutorSystem.Notify_Event("PageClosed");
			TutorSystem.Notify_Event("GoToNextPage");
			this.Close(true);
		}

		protected virtual void DoBack()
		{
			if (this.prev != null)
			{
				Find.WindowStack.Add(this.prev);
			}
			TutorSystem.Notify_Event("PageClosed");
			TutorSystem.Notify_Event("GoToPrevPage");
			this.Close(true);
		}
	}
}
