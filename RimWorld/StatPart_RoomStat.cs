using System;
using Verse;

namespace RimWorld
{
	public class StatPart_RoomStat : StatPart
	{
		private RoomStatDef roomStat;

		private string customLabel;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Room room = req.Thing.GetRoom();
				if (room != null)
				{
					val *= room.GetStat(this.roomStat);
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Room room = req.Thing.GetRoom();
				if (room != null)
				{
					string labelCap;
					if (!this.customLabel.NullOrEmpty())
					{
						labelCap = this.customLabel;
					}
					else
					{
						labelCap = this.roomStat.LabelCap;
					}
					return labelCap + ": x" + room.GetStat(this.roomStat).ToStringPercent();
				}
			}
			return null;
		}
	}
}
