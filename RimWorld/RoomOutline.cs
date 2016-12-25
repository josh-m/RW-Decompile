using System;
using Verse;

namespace RimWorld
{
	public class RoomOutline
	{
		public CellRect rect;

		public int CellsCountIgnoringWalls
		{
			get
			{
				if (this.rect.Width <= 2 || this.rect.Height <= 2)
				{
					return 0;
				}
				return (this.rect.Width - 2) * (this.rect.Height - 2);
			}
		}

		public RoomOutline(CellRect rect)
		{
			this.rect = rect;
		}
	}
}
