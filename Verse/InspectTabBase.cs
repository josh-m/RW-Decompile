using System;
using UnityEngine;

namespace Verse
{
	public abstract class InspectTabBase
	{
		public string labelKey;

		protected Vector2 size;

		public string tutorTag;

		private string cachedTutorHighlightTagClosed;

		protected abstract float PaneTopY
		{
			get;
		}

		protected abstract bool StillValid
		{
			get;
		}

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

		protected Rect TabRect
		{
			get
			{
				this.UpdateSize();
				float y = this.PaneTopY - 30f - this.size.y;
				return new Rect(0f, y, this.size.x, this.size.y);
			}
		}

		public void DoTabGUI()
		{
			Rect rect = this.TabRect;
			Find.WindowStack.ImmediateWindow(235086, rect, WindowLayer.GameUI, delegate
			{
				if (!this.StillValid || !this.IsVisible)
				{
					return;
				}
				if (Widgets.CloseButtonFor(rect.AtZero()))
				{
					this.CloseTab();
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
						ex
					}), 49827);
				}
			}, true, false, 1f);
			this.ExtraOnGUI();
		}

		protected abstract void CloseTab();

		protected abstract void FillTab();

		protected virtual void ExtraOnGUI()
		{
		}

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
