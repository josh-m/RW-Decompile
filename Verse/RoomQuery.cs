using System;

namespace Verse
{
	public static class RoomQuery
	{
		public static Room RoomAt(IntVec3 c, Map map)
		{
			if (!c.InBounds(map))
			{
				return null;
			}
			Region validRegionAt = map.regionGrid.GetValidRegionAt(c);
			if (validRegionAt != null)
			{
				return validRegionAt.Room;
			}
			return null;
		}

		public static Room RoomAtFast(IntVec3 c, Map map)
		{
			Region validRegionAt = map.regionGrid.GetValidRegionAt(c);
			if (validRegionAt != null)
			{
				return validRegionAt.Room;
			}
			return null;
		}

		public static Room RoomAt(Thing thing)
		{
			if (!thing.Spawned)
			{
				return null;
			}
			return RoomQuery.RoomAt(thing.Position, thing.Map);
		}
	}
}
