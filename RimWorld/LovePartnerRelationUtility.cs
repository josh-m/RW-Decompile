using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class LovePartnerRelationUtility
	{
		private const float MinAgeToGenerateWithLovePartnerRelation = 14f;

		public static bool HasAnyLovePartner(Pawn pawn)
		{
			return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null) != null || pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null) != null || pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null) != null;
		}

		public static bool IsLovePartnerRelation(PawnRelationDef relation)
		{
			return relation == PawnRelationDefOf.Lover || relation == PawnRelationDefOf.Fiance || relation == PawnRelationDefOf.Spouse;
		}

		public static bool IsExLovePartnerRelation(PawnRelationDef relation)
		{
			return relation == PawnRelationDefOf.ExLover || relation == PawnRelationDefOf.ExSpouse;
		}

		public static bool HasAnyLovePartnerOfTheSameGender(Pawn pawn)
		{
			return pawn.relations.DirectRelations.Find((DirectPawnRelation x) => LovePartnerRelationUtility.IsLovePartnerRelation(x.def) && x.otherPawn.gender == pawn.gender) != null;
		}

		public static bool HasAnyExLovePartnerOfTheSameGender(Pawn pawn)
		{
			return pawn.relations.DirectRelations.Find((DirectPawnRelation x) => LovePartnerRelationUtility.IsExLovePartnerRelation(x.def) && x.otherPawn.gender == pawn.gender) != null;
		}

		public static bool HasAnyLovePartnerOfTheOppositeGender(Pawn pawn)
		{
			return pawn.relations.DirectRelations.Find((DirectPawnRelation x) => LovePartnerRelationUtility.IsLovePartnerRelation(x.def) && x.otherPawn.gender != pawn.gender) != null;
		}

		public static bool HasAnyExLovePartnerOfTheOppositeGender(Pawn pawn)
		{
			return pawn.relations.DirectRelations.Find((DirectPawnRelation x) => LovePartnerRelationUtility.IsExLovePartnerRelation(x.def) && x.otherPawn.gender != pawn.gender) != null;
		}

		public static Pawn ExistingLovePartner(Pawn pawn)
		{
			Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
			if (firstDirectRelationPawn != null)
			{
				return firstDirectRelationPawn;
			}
			firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
			if (firstDirectRelationPawn != null)
			{
				return firstDirectRelationPawn;
			}
			firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
			if (firstDirectRelationPawn != null)
			{
				return firstDirectRelationPawn;
			}
			return null;
		}

		public static bool LovePartnerRelationExists(Pawn first, Pawn second)
		{
			return first.relations.DirectRelationExists(PawnRelationDefOf.Lover, second) || first.relations.DirectRelationExists(PawnRelationDefOf.Fiance, second) || first.relations.DirectRelationExists(PawnRelationDefOf.Spouse, second);
		}

		public static bool ExLovePartnerRelationExists(Pawn first, Pawn second)
		{
			return first.relations.DirectRelationExists(PawnRelationDefOf.ExSpouse, second) || first.relations.DirectRelationExists(PawnRelationDefOf.ExLover, second);
		}

		public static void GiveRandomExLoverOrExSpouseRelation(Pawn first, Pawn second)
		{
			PawnRelationDef def;
			if (Rand.Value < 0.5f)
			{
				def = PawnRelationDefOf.ExLover;
			}
			else
			{
				def = PawnRelationDefOf.ExSpouse;
			}
			first.relations.AddDirectRelation(def, second);
		}

		public static Pawn GetPartnerInMyBed(Pawn pawn)
		{
			if (pawn.CurJob == null || !pawn.jobs.curDriver.layingDown)
			{
				return null;
			}
			Building_Bed layingDownBed = pawn.jobs.curDriver.layingDownBed;
			if (layingDownBed == null)
			{
				return null;
			}
			if (layingDownBed.SleepingSlotsCount <= 1)
			{
				return null;
			}
			if (!LovePartnerRelationUtility.HasAnyLovePartner(pawn))
			{
				return null;
			}
			foreach (Pawn current in layingDownBed.CurOccupants)
			{
				if (current != pawn)
				{
					if (LovePartnerRelationUtility.LovePartnerRelationExists(pawn, current))
					{
						return current;
					}
				}
			}
			return null;
		}

		public static Pawn ExistingMostLikedLovePartner(Pawn p, bool allowDead)
		{
			DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, allowDead);
			if (directPawnRelation != null)
			{
				return directPawnRelation.otherPawn;
			}
			return null;
		}

		public static DirectPawnRelation ExistingMostLikedLovePartnerRel(Pawn p, bool allowDead)
		{
			if (!p.RaceProps.IsFlesh)
			{
				return null;
			}
			DirectPawnRelation directPawnRelation = null;
			int num = -2147483648;
			List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (allowDead || !directRelations[i].otherPawn.Dead)
				{
					if (LovePartnerRelationUtility.IsLovePartnerRelation(directRelations[i].def))
					{
						int num2 = p.relations.OpinionOf(directRelations[i].otherPawn);
						if (directPawnRelation == null || num2 > num)
						{
							directPawnRelation = directRelations[i];
							num = num2;
						}
					}
				}
			}
			return directPawnRelation;
		}

		public static float GetLovinMtbHours(Pawn pawn, Pawn partner)
		{
			if (pawn.Dead || partner.Dead)
			{
				return -1f;
			}
			if (DebugSettings.alwaysDoLovin)
			{
				return 0.1f;
			}
			if (pawn.needs.food.Starving || partner.needs.food.Starving)
			{
				return -1f;
			}
			if (pawn.health.hediffSet.BleedRateTotal > 0f || partner.health.hediffSet.BleedRateTotal > 0f)
			{
				return -1f;
			}
			float num = LovePartnerRelationUtility.LovinMtbSinglePawnFactor(pawn);
			if (num <= 0f)
			{
				return -1f;
			}
			float num2 = LovePartnerRelationUtility.LovinMtbSinglePawnFactor(partner);
			if (num2 <= 0f)
			{
				return -1f;
			}
			float num3 = 12f;
			num3 *= num;
			num3 *= num2;
			num3 /= Mathf.Max(pawn.relations.SecondaryRomanceChanceFactor(partner), 0.1f);
			num3 /= Mathf.Max(partner.relations.SecondaryRomanceChanceFactor(pawn), 0.1f);
			num3 *= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, (float)pawn.relations.OpinionOf(partner));
			return num3 * GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, (float)partner.relations.OpinionOf(pawn));
		}

		private static float LovinMtbSinglePawnFactor(Pawn pawn)
		{
			float num = 1f;
			num /= 1f - pawn.health.hediffSet.PainTotal;
			float efficiency = pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Consciousness);
			if (efficiency < 0.5f)
			{
				num /= efficiency * 2f;
			}
			return num / GenMath.FlatHill(0f, 14f, 16f, 25f, 80f, 0.2f, pawn.ageTracker.AgeBiologicalYearsFloat);
		}

		public static void TryToShareBed(Pawn first, Pawn second)
		{
			if (LovePartnerRelationUtility.TryToShareBed_Int(first, second))
			{
				return;
			}
			LovePartnerRelationUtility.TryToShareBed_Int(second, first);
		}

		private static bool TryToShareBed_Int(Pawn bedOwner, Pawn otherPawn)
		{
			Building_Bed ownedBed = bedOwner.ownership.OwnedBed;
			if (ownedBed != null && ownedBed.AnyUnownedSleepingSlot)
			{
				otherPawn.ownership.ClaimBedIfNonMedical(ownedBed);
				return true;
			}
			return false;
		}

		public static float LovePartnerRelationGenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
		{
			if (generated.ageTracker.AgeBiologicalYearsFloat < 14f)
			{
				return 0f;
			}
			if (other.ageTracker.AgeBiologicalYearsFloat < 14f)
			{
				return 0f;
			}
			if (generated.gender == other.gender && (!other.story.traits.HasTrait(TraitDefOf.Gay) || !request.AllowGay))
			{
				return 0f;
			}
			if (generated.gender != other.gender && other.story.traits.HasTrait(TraitDefOf.Gay))
			{
				return 0f;
			}
			float num = 1f;
			if (ex)
			{
				int num2 = 0;
				List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
				for (int i = 0; i < directRelations.Count; i++)
				{
					if (LovePartnerRelationUtility.IsExLovePartnerRelation(directRelations[i].def))
					{
						num2++;
					}
				}
				num = Mathf.Pow(0.2f, (float)num2);
			}
			else if (LovePartnerRelationUtility.HasAnyLovePartner(other))
			{
				return 0f;
			}
			float num3 = (generated.gender != other.gender) ? 1f : 0.01f;
			float generationChanceAgeFactor = LovePartnerRelationUtility.GetGenerationChanceAgeFactor(generated);
			float generationChanceAgeFactor2 = LovePartnerRelationUtility.GetGenerationChanceAgeFactor(other);
			float generationChanceAgeGapFactor = LovePartnerRelationUtility.GetGenerationChanceAgeGapFactor(generated, other, ex);
			float num4 = 1f;
			if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
			{
				num4 = 0.01f;
			}
			float num5;
			if (request.FixedMelanin.HasValue)
			{
				num5 = ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin);
			}
			else
			{
				num5 = PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin);
			}
			return num * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * num3 * num5 * num4;
		}

		private static float GetGenerationChanceAgeFactor(Pawn p)
		{
			float value = GenMath.LerpDouble(14f, 27f, 0f, 1f, p.ageTracker.AgeBiologicalYearsFloat);
			return Mathf.Clamp(value, 0f, 1f);
		}

		private static float GetGenerationChanceAgeGapFactor(Pawn p1, Pawn p2, bool ex)
		{
			float num = Mathf.Abs(p1.ageTracker.AgeBiologicalYearsFloat - p2.ageTracker.AgeBiologicalYearsFloat);
			if (ex)
			{
				float num2 = LovePartnerRelationUtility.MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p1, p2);
				if (num2 >= 0f)
				{
					num = Mathf.Min(num, num2);
				}
				float num3 = LovePartnerRelationUtility.MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p2, p1);
				if (num3 >= 0f)
				{
					num = Mathf.Min(num, num3);
				}
			}
			if (num > 40f)
			{
				return 0f;
			}
			float value = GenMath.LerpDouble(0f, 20f, 1f, 0.001f, num);
			return Mathf.Clamp(value, 0.001f, 1f);
		}

		private static float MinPossibleAgeGapAtMinAgeToGenerateAsLovers(Pawn p1, Pawn p2)
		{
			float num = p1.ageTracker.AgeChronologicalYearsFloat - 14f;
			if (num < 0f)
			{
				Log.Warning("at < 0");
				return 0f;
			}
			float num2 = PawnRelationUtility.MaxPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, p2.ageTracker.AgeChronologicalYearsFloat, num);
			float num3 = PawnRelationUtility.MinPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, num);
			if (num2 < 0f)
			{
				return -1f;
			}
			if (num2 < 14f)
			{
				return -1f;
			}
			if (num3 <= 14f)
			{
				return 0f;
			}
			return num3 - 14f;
		}

		public static void TryToShareChildrenForGeneratedLovePartner(Pawn generated, Pawn other, PawnGenerationRequest request, float extraChanceFactor)
		{
			if (generated.gender == other.gender)
			{
				return;
			}
			List<Pawn> list = other.relations.Children.ToList<Pawn>();
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i];
				float num = 1f;
				if (generated.gender == Gender.Male)
				{
					num = ChildRelationUtility.ChanceOfBecomingChildOf(pawn, generated, other, null, new PawnGenerationRequest?(request), null);
				}
				else if (generated.gender == Gender.Female)
				{
					num = ChildRelationUtility.ChanceOfBecomingChildOf(pawn, other, generated, null, null, new PawnGenerationRequest?(request));
				}
				num *= extraChanceFactor;
				if (Rand.Value < num)
				{
					if (generated.gender == Gender.Male)
					{
						pawn.SetFather(generated);
					}
					else if (generated.gender == Gender.Female)
					{
						pawn.SetMother(generated);
					}
				}
			}
		}

		public static void ChangeSpouseRelationsToExSpouse(Pawn pawn)
		{
			while (true)
			{
				Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
				if (firstDirectRelationPawn == null)
				{
					break;
				}
				pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, firstDirectRelationPawn);
				pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, firstDirectRelationPawn);
			}
		}

		public static Pawn GetMostDislikedNonPartnerBedOwner(Pawn p)
		{
			Building_Bed ownedBed = p.ownership.OwnedBed;
			if (ownedBed == null)
			{
				return null;
			}
			Pawn pawn = null;
			int num = 0;
			for (int i = 0; i < ownedBed.owners.Count; i++)
			{
				if (ownedBed.owners[i] != p)
				{
					if (!LovePartnerRelationUtility.LovePartnerRelationExists(p, ownedBed.owners[i]))
					{
						int num2 = p.relations.OpinionOf(ownedBed.owners[i]);
						if (pawn == null || num2 < num)
						{
							pawn = ownedBed.owners[i];
							num = num2;
						}
					}
				}
			}
			return pawn;
		}

		public static float IncestOpinionOffsetFor(Pawn other, Pawn pawn)
		{
			float num = 0f;
			List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (LovePartnerRelationUtility.IsLovePartnerRelation(directRelations[i].def))
				{
					if (directRelations[i].otherPawn != pawn)
					{
						if (!directRelations[i].otherPawn.Dead)
						{
							foreach (PawnRelationDef current in other.GetRelations(directRelations[i].otherPawn))
							{
								float incestOpinionOffset = current.incestOpinionOffset;
								if (incestOpinionOffset < num)
								{
									num = incestOpinionOffset;
								}
							}
						}
					}
				}
			}
			return num;
		}
	}
}
