using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_BuildSandbags : Lesson_Instruction
	{
		private List<IntVec3> sandbagCells;

		protected override float ProgressPercent
		{
			get
			{
				int num = 0;
				int num2 = 0;
				foreach (IntVec3 current in this.sandbagCells)
				{
					if (TutorUtility.BuildingOrBlueprintOrFrameCenterExists(current, base.Map, ThingDefOf.Sandbags))
					{
						num2++;
					}
					num++;
				}
				return (float)num2 / (float)num;
			}
		}

		public override void OnActivated()
		{
			base.OnActivated();
			Find.TutorialState.sandbagsRect = TutorUtility.FindUsableRect(7, 7, base.Map, 0f, false);
			this.sandbagCells = new List<IntVec3>();
			foreach (IntVec3 current in Find.TutorialState.sandbagsRect.EdgeCells)
			{
				if (current.x != Find.TutorialState.sandbagsRect.CenterCell.x && current.z != Find.TutorialState.sandbagsRect.CenterCell.z)
				{
					this.sandbagCells.Add(current);
				}
			}
			foreach (IntVec3 current2 in Find.TutorialState.sandbagsRect.ContractedBy(1))
			{
				if (!Find.TutorialState.sandbagsRect.ContractedBy(2).Contains(current2))
				{
					List<Thing> thingList = current2.GetThingList(base.Map);
					for (int i = thingList.Count - 1; i >= 0; i--)
					{
						Thing thing = thingList[i];
						if (thing.def.passability != Traversability.Standable && (thing.def.category == ThingCategory.Plant || thing.def.category == ThingCategory.Item))
						{
							thing.Destroy(DestroyMode.Vanish);
						}
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.LookList<IntVec3>(ref this.sandbagCells, "sandbagCells", LookMode.Undefined, new object[0]);
		}

		public override void LessonOnGUI()
		{
			TutorUtility.DrawLabelOnGUI(Gen.AveragePosition(this.sandbagCells), this.def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			List<IntVec3> cells = (from c in this.sandbagCells
			where !TutorUtility.BuildingOrBlueprintOrFrameCenterExists(c, base.Map, ThingDefOf.Sandbags)
			select c).ToList<IntVec3>();
			GenDraw.DrawFieldEdges(cells);
			GenDraw.DrawArrowPointingAt(Gen.AveragePosition(this.sandbagCells), false);
			if (this.ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-Sandbags")
			{
				return TutorUtility.EventCellsAreWithin(ep, this.sandbagCells);
			}
			return base.AllowAction(ep);
		}
	}
}
