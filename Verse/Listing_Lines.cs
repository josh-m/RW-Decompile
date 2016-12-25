using System;
using UnityEngine;

namespace Verse
{
	public abstract class Listing_Lines : Listing
	{
		public float lineHeight = 20f;

		public Listing_Lines(Rect rect) : base(rect)
		{
		}

		protected void EndLine()
		{
			this.curY += this.lineHeight + this.verticalSpacing;
		}
	}
}
