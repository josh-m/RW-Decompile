using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_PlaceGrowingZone : Lesson_Instruction
	{
		private CellRect growingZoneRect;

		private List<IntVec3> cachedCells;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<CellRect>(ref this.growingZoneRect, "growingZoneRect", default(CellRect), false);
		}

		public override void OnActivated()
		{
			base.OnActivated();
			this.growingZoneRect = TutorUtility.FindUsableRect(10, 8, base.Map, 0.5f, false);
			this.cachedCells = this.growingZoneRect.Cells.ToList<IntVec3>();
		}

		public override void LessonOnGUI()
		{
			TutorUtility.DrawCellRectOnGUI(this.growingZoneRect, this.def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			GenDraw.DrawFieldEdges(this.cachedCells);
			GenDraw.DrawArrowPointingAt(this.growingZoneRect.CenterVector3, false);
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-ZoneAdd_Growing")
			{
				return TutorUtility.EventCellsMatchExactly(ep, this.cachedCells);
			}
			return base.AllowAction(ep);
		}
	}
}
