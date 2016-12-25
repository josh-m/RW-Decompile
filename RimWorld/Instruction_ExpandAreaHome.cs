using System;
using Verse;

namespace RimWorld
{
	public class Instruction_ExpandAreaHome : Instruction_ExpandArea
	{
		protected override Area MyArea
		{
			get
			{
				return base.Map.areaManager.Home;
			}
		}
	}
}
