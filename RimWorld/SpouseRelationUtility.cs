using System;
using Verse;

namespace RimWorld
{
	public static class SpouseRelationUtility
	{
		public const float ChanceForSpousesToHaveTheSameName = 0.8f;

		public static Pawn GetSpouse(this Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return null;
			}
			return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
		}

		public static Pawn GetSpouseOppositeGender(this Pawn pawn)
		{
			Pawn spouse = pawn.GetSpouse();
			if (spouse == null)
			{
				return null;
			}
			if ((pawn.gender == Gender.Male && spouse.gender == Gender.Female) || (pawn.gender == Gender.Female && spouse.gender == Gender.Male))
			{
				return spouse;
			}
			return null;
		}
	}
}
