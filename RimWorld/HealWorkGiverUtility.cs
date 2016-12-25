using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class HealWorkGiverUtility
	{
		public static Thing FindBestMedicine(Pawn healer, Pawn patient)
		{
			if (patient.playerSettings == null || patient.playerSettings.medCare <= MedicalCareCategory.NoMeds)
			{
				return null;
			}
			Predicate<Thing> predicate = (Thing m) => !m.IsForbidden(healer) && patient.playerSettings.medCare.AllowsMedicine(m.def) && healer.CanReserve(m, 1);
			Func<Thing, float> priorityGetter = (Thing t) => t.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThing_Global_Reachable(patient.Position, Find.ListerThings.ThingsInGroup(ThingRequestGroup.Medicine), PathEndMode.ClosestTouch, TraverseParms.For(healer, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, priorityGetter);
		}
	}
}
