using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsNightForNightOwl : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.Awake() && (GenLocalDate.HourInt(p) >= 23 || GenLocalDate.HourInt(p) <= 5);
		}
	}
}
