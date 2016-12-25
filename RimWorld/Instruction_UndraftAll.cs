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
				return 1f - (float)this.DraftedPawns().Count<Pawn>() / (float)base.Map.mapPawns.FreeColonistsSpawnedCount;
			}
		}

		private IEnumerable<Pawn> DraftedPawns()
		{
			return from p in base.Map.mapPawns.FreeColonistsSpawned
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
