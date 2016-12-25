using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_EquipWeapons : Lesson_Instruction
	{
		protected override float ProgressPercent
		{
			get
			{
				return (float)(from c in base.Map.mapPawns.FreeColonists
				where c.equipment.Primary != null
				select c).Count<Pawn>() / (float)base.Map.mapPawns.FreeColonistsCount;
			}
		}

		private IEnumerable<Thing> Weapons
		{
			get
			{
				return from it in Find.TutorialState.startingItems
				where Instruction_EquipWeapons.IsWeapon(it) && it.Spawned
				select it;
			}
		}

		public static bool IsWeapon(Thing t)
		{
			return t.def.IsWeapon && t.def.BaseMarketValue > 30f;
		}

		public override void LessonOnGUI()
		{
			foreach (Thing current in this.Weapons)
			{
				TutorUtility.DrawLabelOnThingOnGUI(current, this.def.onMapInstruction);
			}
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			foreach (Thing current in this.Weapons)
			{
				GenDraw.DrawArrowPointingAt(current.DrawPos, true);
			}
			if (this.ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
