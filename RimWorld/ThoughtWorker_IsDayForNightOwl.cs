using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsDayForNightOwl : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.Awake() && GenLocalDate.HourInt(p) >= 11 && GenLocalDate.HourInt(p) <= 17;
		}
	}
}
