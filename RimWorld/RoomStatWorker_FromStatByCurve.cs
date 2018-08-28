using System;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_FromStatByCurve : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			return this.def.curve.Evaluate(room.GetStat(this.def.inputStat));
		}
	}
}
