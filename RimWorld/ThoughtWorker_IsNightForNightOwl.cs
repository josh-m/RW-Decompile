using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsNightForNightOwl : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.Awake() && (GenDate.HourInt >= 23 || GenDate.HourInt <= 5);
		}
	}
}
