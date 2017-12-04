using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_RefugeePod : ItemCollectionGenerator
	{
		private const float RelationWithColonistWeight = 20f;

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, Faction.OfSpacer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 20f, false, true, true, false, false, false, false, null, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			outThings.Add(pawn);
			HealthUtility.DamageUntilDowned(pawn);
		}
	}
}
