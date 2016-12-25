using RimWorld;
using System;

namespace Verse.AI.Group
{
	public class LordToil_ExitMapBest : LordToil_ExitMap
	{
		protected override DutyDef DutyDef
		{
			get
			{
				return DutyDefOf.ExitMapBest;
			}
		}

		public LordToil_ExitMapBest(LocomotionUrgency locomotion = LocomotionUrgency.None, bool canDig = false) : base(locomotion, canDig)
		{
		}
	}
}
