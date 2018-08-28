using System;
using Verse;

namespace RimWorld
{
	public static class DownedRefugeeQuestUtility
	{
		private const float RelationWithColonistWeight = 20f;

		private const float ChanceToRedressWorldPawn = 0.2f;

		public static Pawn GenerateRefugee(int tile)
		{
			PawnKindDef spaceRefugee = PawnKindDefOf.SpaceRefugee;
			Faction randomFactionForRefugee = DownedRefugeeQuestUtility.GetRandomFactionForRefugee();
			PawnGenerationRequest request = new PawnGenerationRequest(spaceRefugee, randomFactionForRefugee, PawnGenerationContext.NonPlayer, tile, false, false, false, false, true, false, 20f, true, true, true, false, false, false, false, null, null, new float?(0.2f), null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			HealthUtility.DamageUntilDowned(pawn, false);
			HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, false);
			return pawn;
		}

		public static Faction GetRandomFactionForRefugee()
		{
			Faction result;
			if (Rand.Chance(0.6f) && Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out result, true, false, TechLevel.Undefined))
			{
				return result;
			}
			return null;
		}
	}
}
