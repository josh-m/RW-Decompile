using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_ChopWood : Lesson_Instruction
	{
		protected override float ProgressPercent
		{
			get
			{
				return (float)(from d in base.Map.designationManager.DesignationsOfDef(DesignationDefOf.HarvestPlant)
				where d.target.Thing.def.plant.IsTree
				select d).Count<Designation>() / (float)this.def.targetCount;
			}
		}

		public override void LessonUpdate()
		{
			if (this.ProgressPercent > 0.999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
