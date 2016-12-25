using RimWorld;
using System;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public abstract class ITab
	{
		public string labelKey;

		protected Vector2 size;

		public string tutorTag;

		private string cachedTutorHighlightTagClosed;

		public virtual bool IsVisible
		{
			get
			{
				return true;
			}
		}

		public string TutorHighlightTagClosed
		{
			get
			{
				if (this.tutorTag == null)
				{
					return null;
				}
				if (this.cachedTutorHighlightTagClosed == null)
				{
					this.cachedTutorHighlightTagClosed = "ITab-" + this.tutorTag + "-Closed";
				}
				return this.cachedTutorHighlightTagClosed;
			}
		}

		protected object SelObject
		{
			get
			{
				return Find.Selector.SingleSelectedObject;
			}
		}

		protected Thing SelThing
		{
			get
			{
				return Find.Selector.SingleSelectedThing;
			}
		}

		protected Pawn SelPawn
		{
			get
			{
				return this.SelThing as Pawn;
			}
		}

		public void DoTabGUI()
		{
			MainTabWindow_Inspect inspectWorker = (MainTabWindow_Inspect)MainTabDefOf.Inspect.Window;
			this.UpdateSize();
			float y = inspectWorker.PaneTopY - 30f - this.size.y;
			Rect outRect = new Rect(0f, y, this.size.x, this.size.y);
			Find.WindowStack.ImmediateWindow(235086, outRect, WindowLayer.GameUI, delegate
			{
				if (Find.MainTabsRoot.OpenTab != MainTabDefOf.Inspect || !((MainTabWindow_Inspect)Find.MainTabsRoot.OpenTab.Window).CurTabs.Contains(this) || !this.IsVisible)
				{
					return;
				}
				if (Widgets.CloseButtonFor(outRect.AtZero()))
				{
					inspectWorker.CloseOpenTab();
				}
				try
				{
					this.FillTab();
				}
				catch (Exception ex)
				{
					Log.ErrorOnce(string.Concat(new object[]
					{
						"Exception filling tab ",
						this.GetType(),
						": ",
						ex.ToString()
					}), 49827);
				}
			}, true, false, 1f);
		}

		protected abstract void FillTab();

		protected virtual void UpdateSize()
		{
		}

		public virtual void OnOpen()
		{
		}

		public virtual void TabTick()
		{
		}

		public virtual void TabUpdate()
		{
		}
	}
}
