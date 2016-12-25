using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_RoomIdentifier : CompProperties
	{
		public RoomStatDef roomStat;

		public CompProperties_RoomIdentifier()
		{
			this.compClass = typeof(CompRoomIdentifier);
		}
	}
}
