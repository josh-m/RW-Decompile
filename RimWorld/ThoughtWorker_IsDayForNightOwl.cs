using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsDayForNightOwl : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.Awake() && GenDate.HourInt >= 11 && GenDate.HourInt <= 17;
		}
	}
}
