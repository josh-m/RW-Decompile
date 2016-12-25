using System;

namespace Verse.AI.Group
{
	public class LordToil_End : LordToil
	{
		public override bool ShouldFail
		{
			get
			{
				return true;
			}
		}

		public override void UpdateAllDuties()
		{
		}
	}
}
