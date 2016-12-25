using System;
using Verse;

namespace RimWorld
{
	public class Instruction_BuildNearRoom : Instruction_BuildAtRoom
	{
		protected override CellRect BuildableRect
		{
			get
			{
				return Find.TutorialState.roomRect.ExpandedBy(10);
			}
		}

		protected override bool AllowBuildAt(IntVec3 c)
		{
			return base.AllowBuildAt(c) && !Find.TutorialState.roomRect.Contains(c);
		}
	}
}
