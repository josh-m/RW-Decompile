using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ParentRelationUtility
	{
		public static Pawn GetFather(this Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return null;
			}
			return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, (Pawn x) => x.gender != Gender.Female);
		}

		public static Pawn GetMother(this Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return null;
			}
			return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, (Pawn x) => x.gender == Gender.Female);
		}

		public static void SetFather(this Pawn pawn, Pawn newFather)
		{
			if (newFather != null && newFather.gender == Gender.Female)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to set ",
					newFather,
					" with gender ",
					newFather.gender,
					" as ",
					pawn,
					"'s father."
				}));
				return;
			}
			Pawn father = pawn.GetFather();
			if (father != newFather)
			{
				if (father != null)
				{
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Parent, father);
				}
				if (newFather != null)
				{
					pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newFather);
				}
			}
		}

		public static void SetMother(this Pawn pawn, Pawn newMother)
		{
			if (newMother != null && newMother.gender != Gender.Female)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to set ",
					newMother,
					" with gender ",
					newMother.gender,
					" as ",
					pawn,
					"'s mother."
				}));
				return;
			}
			Pawn mother = pawn.GetMother();
			if (mother != newMother)
			{
				if (mother != null)
				{
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Parent, mother);
				}
				if (newMother != null)
				{
					pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newMother);
				}
			}
		}

		public static float GetRandomSecondParentSkinColor(float otherParentSkin, float childSkin, float? secondChildSkin = null)
		{
			float mirror;
			if (secondChildSkin.HasValue)
			{
				mirror = (childSkin + secondChildSkin.Value) / 2f;
			}
			else
			{
				mirror = childSkin;
			}
			float reflectedSkin = ChildRelationUtility.GetReflectedSkin(otherParentSkin, mirror);
			float num = childSkin;
			float num2 = childSkin;
			if (secondChildSkin.HasValue)
			{
				num = Mathf.Min(num, secondChildSkin.Value);
				num2 = Mathf.Max(num2, secondChildSkin.Value);
			}
			float clampMin = 0f;
			float clampMax = 1f;
			if (reflectedSkin >= num2)
			{
				clampMin = num2;
			}
			else
			{
				clampMax = num;
			}
			return PawnSkinColors.GetRandomMelaninSimilarTo(reflectedSkin, clampMin, clampMax);
		}
	}
}
