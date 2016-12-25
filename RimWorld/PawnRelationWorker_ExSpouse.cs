using System;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_ExSpouse : PawnRelationWorker
	{
		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, true) * base.BaseGenerationChanceFactor(generated, other, request);
		}

		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			generated.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, other);
			LovePartnerRelationUtility.TryToShareChildrenForGeneratedLovePartner(generated, other, request, 1f);
			PawnRelationWorker_ExSpouse.ResolveMySkinColor(ref request, generated, other);
		}

		private static void ResolveMySkinColor(ref PawnGenerationRequest request, Pawn generated, Pawn other)
		{
			if (request.FixedMelanin.HasValue)
			{
				return;
			}
			request.SetFixedMelanin(PawnSkinColors.GetRandomMelaninSimilarTo(other.story.melanin, 0f, 1f));
		}
	}
}
