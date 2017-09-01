using System;
using UnityEngine;

namespace Verse
{
	public abstract class Listing
	{
		public const float ColumnSpacing = 17f;

		private const float DefaultGap = 12f;

		public float verticalSpacing = 2f;

		protected Rect listingRect;

		protected float curY;

		protected float curX;

		private float columnWidthInt;

		private bool hasCustomColumnWidth;

		public float CurHeight
		{
			get
			{
				return this.curY;
			}
		}

		public float ColumnWidth
		{
			get
			{
				return this.columnWidthInt;
			}
			set
			{
				this.columnWidthInt = value;
				this.hasCustomColumnWidth = true;
			}
		}

		public void NewColumn()
		{
			this.curY = 0f;
			this.curX += this.ColumnWidth + 17f;
		}

		protected void NewColumnIfNeeded(float neededHeight)
		{
			if (this.curY + neededHeight > this.listingRect.height)
			{
				this.NewColumn();
			}
		}

		public Rect GetRect(float height)
		{
			this.NewColumnIfNeeded(height);
			Rect result = new Rect(this.curX, this.curY, this.ColumnWidth, height);
			this.curY += height;
			return result;
		}

		public void Gap(float gapHeight = 12f)
		{
			this.curY += gapHeight;
		}

		public void GapLine(float gapHeight = 12f)
		{
			float y = this.curY + gapHeight / 2f;
			Color color = GUI.color;
			GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
			Widgets.DrawLineHorizontal(this.curX, y, this.ColumnWidth);
			GUI.color = color;
			this.curY += gapHeight;
		}

		public virtual void Begin(Rect rect)
		{
			this.listingRect = rect;
			if (this.hasCustomColumnWidth)
			{
				if (this.columnWidthInt > this.listingRect.width)
				{
					Log.Error(string.Concat(new object[]
					{
						"Listing set ColumnWith to ",
						this.columnWidthInt,
						" which is more than the whole listing rect width of ",
						this.listingRect.width,
						". Clamping."
					}));
					this.columnWidthInt = this.listingRect.width;
				}
			}
			else
			{
				this.columnWidthInt = this.listingRect.width;
			}
			this.curX = 0f;
			this.curY = 0f;
			GUI.BeginGroup(rect);
		}

		public virtual void End()
		{
			GUI.EndGroup();
		}
	}
}
