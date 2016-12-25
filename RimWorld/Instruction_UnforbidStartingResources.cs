using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_UnforbidStartingResources : Lesson_Instruction
	{
		protected override float ProgressPercent
		{
			get
			{
				return (float)(from it in Find.TutorialState.startingItems
				where !it.IsForbidden(Faction.OfPlayer) || it.Destroyed
				select it).Count<Thing>() / (float)Find.TutorialState.startingItems.Count;
			}
		}

		private IEnumerable<Thing> NeedUnforbidItems()
		{
			return from it in Find.TutorialState.startingItems
			where it.IsForbidden(Faction.OfPlayer) && !it.Destroyed
			select it;
		}

		public override void PostDeactivated()
		{
			base.PostDeactivated();
			Find.TutorialState.startingItems.RemoveAll((Thing it) => !Instruction_EquipWeapons.IsWeapon(it));
		}

		public override void LessonOnGUI()
		{
			foreach (Thing current in this.NeedUnforbidItems())
			{
				TutorUtility.DrawLabelOnThingOnGUI(current, this.def.onMapInstruction);
			}
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			if (this.ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
			foreach (Thing current in this.NeedUnforbidItems())
			{
				GenDraw.DrawArrowPointingAt(current.DrawPos, true);
			}
		}
	}
}
