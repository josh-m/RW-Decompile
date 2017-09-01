using System;
using Verse;

namespace RimWorld
{
	public class HoldOffsetSet
	{
		public HoldOffset northDefault;

		public HoldOffset east;

		public HoldOffset south;

		public HoldOffset west;

		public HoldOffset Pick(Rot4 rotation)
		{
			if (rotation == Rot4.North)
			{
				return this.northDefault;
			}
			if (rotation == Rot4.East)
			{
				return this.east;
			}
			if (rotation == Rot4.South)
			{
				return this.south;
			}
			if (rotation == Rot4.West)
			{
				return this.west;
			}
			return null;
		}
	}
}
