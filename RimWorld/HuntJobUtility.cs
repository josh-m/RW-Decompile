using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class HuntJobUtility
	{
		public static Corpse TryFindCorpse(Pawn killedPawn)
		{
			List<Thing> thingList = killedPawn.Position.GetThingList();
			for (int i = 0; i < thingList.Count; i++)
			{
				Corpse corpse = thingList[i] as Corpse;
				if (corpse != null && corpse.innerPawn == killedPawn)
				{
					return corpse;
				}
			}
			return null;
		}

		public static bool WasKilledByHunter(Pawn pawn, DamageInfo? dinfo)
		{
			if (!dinfo.HasValue)
			{
				return false;
			}
			Pawn pawn2 = dinfo.Value.Instigator as Pawn;
			if (pawn2 == null || pawn2.CurJob == null)
			{
				return false;
			}
			JobDriver_Hunt jobDriver_Hunt = pawn2.jobs.curDriver as JobDriver_Hunt;
			return jobDriver_Hunt != null && jobDriver_Hunt.Victim == pawn;
		}
	}
}
