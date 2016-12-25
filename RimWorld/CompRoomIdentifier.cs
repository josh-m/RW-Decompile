using System;
using Verse;

namespace RimWorld
{
	public class CompRoomIdentifier : ThingComp
	{
		private CompProperties_RoomIdentifier Props
		{
			get
			{
				return (CompProperties_RoomIdentifier)this.props;
			}
		}

		public override string CompInspectStringExtra()
		{
			Room room = this.parent.GetRoom();
			string str;
			if (room == null || room.Role == RoomRoleDefOf.None)
			{
				str = "Outdoors".Translate();
			}
			else
			{
				str = string.Concat(new string[]
				{
					room.Role.LabelCap,
					" (",
					room.GetStatScoreStage(this.Props.roomStat).label,
					", ",
					this.Props.roomStat.ScoreToString(room.GetStat(this.Props.roomStat)),
					")"
				});
			}
			return "Room".Translate() + ": " + str;
		}
	}
}
