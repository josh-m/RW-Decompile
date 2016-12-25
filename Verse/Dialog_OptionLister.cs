using System;
using UnityEngine;

namespace Verse
{
	public abstract class Dialog_OptionLister : Window
	{
		protected const float ButSpacing = 0f;

		protected Vector2 scrollPosition;

		protected Listing_Standard listing;

		protected string filter = string.Empty;

		protected float totalOptionsHeight;

		protected static readonly Vector2 ButSize = new Vector2(230f, 27f);

		protected readonly float ColumnSpacing = 20f;

		protected readonly float SectSpacing = 8f;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(1024f, 768f);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public Dialog_OptionLister()
		{
			this.closeOnEscapeKey = true;
			this.doCloseX = true;
			this.onlyOneOfTypeAllowed = true;
			this.absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.filter = Widgets.TextField(new Rect(0f, 0f, 200f, 30f), this.filter);
			if (Event.current.type == EventType.Layout)
			{
				this.totalOptionsHeight = 0f;
			}
			Rect outRect = new Rect(inRect);
			outRect.yMin += 35f;
			float num = (this.totalOptionsHeight + 72f) / 4f;
			if (num < outRect.height)
			{
				num = outRect.height;
			}
			Rect rect = new Rect(0f, 0f, outRect.width - 16f, num);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect);
			this.listing = new Listing_Standard(rect);
			this.listing.ColumnWidth = (rect.width - 51f) / 4f;
			this.DoListingItems();
			this.listing.End();
			Widgets.EndScrollView();
		}

		public override void PostClose()
		{
			base.PostClose();
			GUI.FocusControl(null);
		}

		protected abstract void DoListingItems();

		protected bool FilterAllows(string label)
		{
			return this.filter.NullOrEmpty() || label.NullOrEmpty() || label.IndexOf(this.filter, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}
}
