using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_CopyPasteTimetable : PawnColumnWorker_CopyPaste
	{
		private static List<TimeAssignmentDef> clipboard;

		protected override bool AnythingInClipboard
		{
			get
			{
				return PawnColumnWorker_CopyPasteTimetable.clipboard != null;
			}
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (pawn.timetable == null)
			{
				return;
			}
			base.DoCell(rect, pawn, table);
		}

		protected override void CopyFrom(Pawn p)
		{
			PawnColumnWorker_CopyPasteTimetable.clipboard = p.timetable.times.ToList<TimeAssignmentDef>();
		}

		protected override void PasteTo(Pawn p)
		{
			for (int i = 0; i < 24; i++)
			{
				p.timetable.times[i] = PawnColumnWorker_CopyPasteTimetable.clipboard[i];
			}
		}
	}
}
