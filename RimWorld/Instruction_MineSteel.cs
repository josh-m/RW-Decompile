using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Instruction_MineSteel : Lesson_Instruction
	{
		private List<IntVec3> mineCells;

		protected override float ProgressPercent
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.mineCells.Count; i++)
				{
					IntVec3 c = this.mineCells[i];
					if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.Mine) != null || c.GetEdifice(base.Map) == null || c.GetEdifice(base.Map).def != ThingDefOf.MineableSteel)
					{
						num++;
					}
				}
				return (float)num / (float)this.mineCells.Count;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.LookList<IntVec3>(ref this.mineCells, "mineCells", LookMode.Undefined, new object[0]);
		}

		public override void OnActivated()
		{
			base.OnActivated();
			CellRect cellRect = TutorUtility.FindUsableRect(10, 10, base.Map, 0f, true);
			new GenStep_ScatterLumpsMineable
			{
				forcedDefToScatter = ThingDefOf.MineableSteel
			}.DebugForceScatterAt(cellRect.CenterCell);
			this.mineCells = new List<IntVec3>();
			foreach (IntVec3 current in cellRect)
			{
				Building edifice = current.GetEdifice(base.Map);
				if (edifice != null && edifice.def == ThingDefOf.MineableSteel)
				{
					this.mineCells.Add(current);
				}
			}
		}

		public override void LessonOnGUI()
		{
			if (!this.mineCells.NullOrEmpty<IntVec3>())
			{
				TutorUtility.DrawLabelOnGUI(Gen.AveragePosition(this.mineCells), this.def.onMapInstruction);
			}
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			GenDraw.DrawArrowPointingAt(Gen.AveragePosition(this.mineCells), false);
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-Mine")
			{
				return TutorUtility.EventCellsAreWithin(ep, this.mineCells);
			}
			return base.AllowAction(ep);
		}

		public override void Notify_Event(EventPack ep)
		{
			if (ep.Tag == "Designate-Mine" && this.ProgressPercent > 0.999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
