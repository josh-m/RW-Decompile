using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_TimetableTracker : IExposable
	{
		private Pawn pawn;

		public List<TimeAssignmentDef> times;

		public TimeAssignmentDef CurrentAssignment
		{
			get
			{
				return this.times[GenLocalDate.HourOfDay(this.pawn)];
			}
		}

		public Pawn_TimetableTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.times = new List<TimeAssignmentDef>(24);
			for (int i = 0; i < 24; i++)
			{
				TimeAssignmentDef item;
				if (i <= 5 || i > 21)
				{
					item = TimeAssignmentDefOf.Sleep;
				}
				else
				{
					item = TimeAssignmentDefOf.Anything;
				}
				this.times.Add(item);
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<TimeAssignmentDef>(ref this.times, "times", LookMode.Undefined, new object[0]);
		}

		public TimeAssignmentDef GetAssignment(int hour)
		{
			return this.times[hour];
		}

		public void SetAssignment(int hour, TimeAssignmentDef ta)
		{
			this.times[hour] = ta;
		}
	}
}
