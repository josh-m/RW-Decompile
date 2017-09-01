using System;

namespace Verse
{
	public abstract class Listing_Lines : Listing
	{
		public float lineHeight = 20f;

		protected void EndLine()
		{
			this.curY += this.lineHeight + this.verticalSpacing;
		}
	}
}
