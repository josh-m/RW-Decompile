using System;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanBedUtility
	{
		public static bool InCaravanBed(this Pawn p)
		{
			return p.CurrentCaravanBed() != null;
		}

		public static Building_Bed CurrentCaravanBed(this Pawn p)
		{
			Caravan caravan = p.GetCaravan();
			if (caravan == null)
			{
				return null;
			}
			return caravan.beds.GetBedUsedBy(p);
		}

		public static bool WouldBenefitFromRestingInBed(Pawn p)
		{
			return !p.Dead && p.health.hediffSet.HasImmunizableNotImmuneHediff();
		}

		public static string AppendUsingBedsLabel(string str, int bedCount)
		{
			string str2 = (bedCount != 1) ? "UsingBedrolls".Translate(bedCount) : "UsingBedroll".Translate();
			return str + " (" + str2 + ")";
		}
	}
}
