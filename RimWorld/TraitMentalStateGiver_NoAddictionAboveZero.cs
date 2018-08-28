using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TraitMentalStateGiver_NoAddictionAboveZero : TraitMentalStateGiver
	{
		public override bool CheckGive(Pawn pawn, int checkInterval)
		{
			bool flag = false;
			List<Need> allNeeds = pawn.needs.AllNeeds;
			for (int i = 0; i < allNeeds.Count; i++)
			{
				Need_Chemical need_Chemical = allNeeds[i] as Need_Chemical;
				if (need_Chemical != null && need_Chemical.CurLevel > 0.001f)
				{
					flag = true;
					break;
				}
			}
			return !flag && base.CheckGive(pawn, checkInterval);
		}
	}
}
