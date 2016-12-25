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
				if (value > this.listingRect.width)
				{
					Log.Error(string.Concat(new object[]
					{
						"Listing set ColumnWith to ",
						value,
						" which is more than the whole listing rect width of ",
						this.listingRect.width,
						". Clamping."
					}));
					value = this.listingRect.width;
				}
				this.columnWidthInt = value;
			}
		}

		public Listing(Rect rect)
		{
			this.listingRect = rect;
			this.columnWidthInt = this.listingRect.width;
			GUI.BeginGroup(rect);
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

		public virtual void End()
		{
			GUI.EndGroup();
		}
	}
}
