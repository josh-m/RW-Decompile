using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_UndraftAll : Lesson_Instruction
	{
		protected override float ProgressPercent
		{
			get
			{
				return (float)this.DraftedPawns().Count<Pawn>() / (float)Find.MapPawns.FreeColonistsSpawnedCount;
			}
		}

		private IEnumerable<Pawn> DraftedPawns()
		{
			return from p in Find.MapPawns.FreeColonistsSpawned
			where p.Drafted
			select p;
		}

		public override void LessonUpdate()
		{
			foreach (Pawn current in this.DraftedPawns())
			{
				GenDraw.DrawArrowPointingAt(current.DrawPos, false);
			}
			if (this.ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
