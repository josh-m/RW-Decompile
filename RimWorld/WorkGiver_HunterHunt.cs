using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_HunterHunt : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation des in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Hunt))
			{
				yield return des.target.Thing;
			}
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			return !WorkGiver_HunterHunt.HasHuntingWeapon(pawn) || WorkGiver_HunterHunt.HasShieldAndRangedWeapon(pawn);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			return pawn2 != null && pawn2.RaceProps.Animal && pawn.CanReserve(t, 1, -1, null, forced) && pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Hunt) != null;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(JobDefOf.Hunt, t);
		}

		public static bool HasHuntingWeapon(Pawn p)
		{
			return p.equipment.Primary != null && p.equipment.Primary.def.IsRangedWeapon;
		}

		public static bool HasShieldAndRangedWeapon(Pawn p)
		{
			if (p.equipment.Primary != null && !p.equipment.Primary.def.Verbs[0].MeleeRange)
			{
				List<Apparel> wornApparel = p.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (wornApparel[i] is ShieldBelt)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
