using System;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Lover : PawnRelationWorker
	{
		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, false) * base.BaseGenerationChanceFactor(generated, other, request);
		}

		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			generated.relations.AddDirectRelation(PawnRelationDefOf.Lover, other);
			LovePartnerRelationUtility.TryToShareChildrenForGeneratedLovePartner(generated, other, request, 0.35f);
			PawnRelationWorker_Lover.ResolveMySkinColor(ref request, generated, other);
		}

		private static void ResolveMySkinColor(ref PawnGenerationRequest request, Pawn generated, Pawn other)
		{
			if (request.FixedSkinWhiteness.HasValue)
			{
				return;
			}
			request.SetFixedSkinWhiteness(PawnSkinColors.GetRandomSkinColorSimilarTo(other.story.skinWhiteness, 0f, 1f));
		}
	}
}
