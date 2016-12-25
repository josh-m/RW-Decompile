using System;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Spouse : PawnRelationWorker
	{
		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, false) * base.BaseGenerationChanceFactor(generated, other, request);
		}

		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			generated.relations.AddDirectRelation(PawnRelationDefOf.Spouse, other);
			LovePartnerRelationUtility.TryToShareChildrenForGeneratedLovePartner(generated, other, request, 1f);
			PawnRelationWorker_Spouse.ResolveMyName(ref request, generated);
			PawnRelationWorker_Spouse.ResolveMySkinColor(ref request, generated, other);
		}

		private static void ResolveMyName(ref PawnGenerationRequest request, Pawn generated)
		{
			if (request.FixedLastName != null)
			{
				return;
			}
			if (Rand.Value < 0.8f)
			{
				request.SetFixedLastName(((NameTriple)generated.GetSpouse().Name).Last);
			}
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
