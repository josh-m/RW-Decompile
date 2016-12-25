using System;

namespace Verse
{
	public static class RoomQuery
	{
		public static Room RoomAt(IntVec3 c)
		{
			if (!c.InBounds())
			{
				return null;
			}
			Region validRegionAt = Find.RegionGrid.GetValidRegionAt(c);
			if (validRegionAt != null)
			{
				return validRegionAt.Room;
			}
			return null;
		}

		public static Room RoomAtFast(IntVec3 c)
		{
			Region validRegionAt = Find.RegionGrid.GetValidRegionAt(c);
			if (validRegionAt != null)
			{
				return validRegionAt.Room;
			}
			return null;
		}
	}
}
