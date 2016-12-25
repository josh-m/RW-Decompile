using System;
using Verse;

namespace RimWorld
{
	public class GenStep_FindPlayerStartSpot : GenStep
	{
		public override void Generate()
		{
			DeepProfiler.Start("RebuildAllRegions");
			RegionAndRoomUpdater.RebuildAllRegionsAndRooms();
			DeepProfiler.End();
			int debug_numStand = 0;
			int debug_numRoom = 0;
			int debug_numTouch = 0;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!c.Standable())
				{
					debug_numStand++;
					return false;
				}
				Room room = c.GetRoom();
				if (room == null)
				{
					debug_numRoom++;
					return false;
				}
				if (!room.TouchesMapEdge)
				{
					debug_numTouch++;
					return false;
				}
				return true;
			};
			IntVec3 playerStartSpot = IntVec3.Invalid;
			bool flag = false;
			int i;
			for (i = 7; i > 2; i--)
			{
				int num = Find.Map.Size.x / i;
				int minEdgeDistance = (Find.Map.Size.x - num) / 2;
				if (CellFinderLoose.TryFindRandomNotEdgeCellWith(minEdgeDistance, validator, out playerStartSpot))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Log.Error("Found no good player start spot. Choosing randomly.");
				Log.Message(string.Concat(new object[]
				{
					" i=",
					i,
					", numStand=",
					debug_numStand,
					", numRoom=",
					debug_numRoom,
					", numTouch=",
					debug_numTouch
				}));
				playerStartSpot = CellFinderLoose.RandomCellWith(validator, 1000);
			}
			MapGenerator.PlayerStartSpot = playerStartSpot;
		}
	}
}
