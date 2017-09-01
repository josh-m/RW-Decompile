using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsCarryingIncendiaryWeapon : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.equipment.Primary == null)
			{
				return false;
			}
			List<VerbProperties> verbs = p.equipment.Primary.def.Verbs;
			for (int i = 0; i < verbs.Count; i++)
			{
				if (verbs[i].ai_IsIncendiary)
				{
					return true;
				}
			}
			return false;
		}
	}
}
