using System;
using Verse;

namespace RimWorld
{
	public class Instruction_ExpandAreaBuildRoof : Instruction_ExpandArea
	{
		protected override Area MyArea
		{
			get
			{
				return base.Map.areaManager.BuildRoof;
			}
		}
	}
}
