using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class RelationsUtility
	{
		private static List<Thought> tmpThoughts = new List<Thought>();

		public static bool PawnsKnowEachOther(Pawn p1, Pawn p2)
		{
			return (p1.Faction != null && p1.Faction == p2.Faction) || (p1.RaceProps.IsFlesh && p1.relations.DirectRelations.Find((DirectPawnRelation x) => x.otherPawn == p2) != null) || (p2.RaceProps.IsFlesh && p2.relations.DirectRelations.Find((DirectPawnRelation x) => x.otherPawn == p1) != null) || RelationsUtility.HasAnySocialMemoryWith(p1, p2) || RelationsUtility.HasAnySocialMemoryWith(p2, p1);
		}

		public static bool IsDisfigured(Pawn pawn)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Part != null && hediffs[i].Part.def.beautyRelated)
				{
					if (hediffs[i] is Hediff_MissingPart || hediffs[i] is Hediff_Injury)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool TryDevelopBondRelation(Pawn humanlike, Pawn animal, float baseChance)
		{
			if (!animal.RaceProps.Animal)
			{
				return false;
			}
			if (animal.RaceProps.trainableIntelligence < TrainableIntelligence.Intermediate)
			{
				return false;
			}
			if (humanlike.relations.DirectRelationExists(PawnRelationDefOf.Bond, animal))
			{
				return false;
			}
			if (animal.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, (Pawn x) => x.Spawned) != null)
			{
				return false;
			}
			int num = 0;
			List<DirectPawnRelation> directRelations = animal.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (directRelations[i].def == PawnRelationDefOf.Bond && !directRelations[i].otherPawn.Dead)
				{
					num++;
				}
			}
			int num2 = 0;
			List<DirectPawnRelation> directRelations2 = humanlike.relations.DirectRelations;
			for (int j = 0; j < directRelations2.Count; j++)
			{
				if (directRelations2[j].def == PawnRelationDefOf.Bond && !directRelations2[j].otherPawn.Dead)
				{
					num2++;
				}
			}
			if (num > 0)
			{
				baseChance *= Mathf.Pow(0.2f, (float)num);
			}
			if (num2 > 0)
			{
				baseChance *= Mathf.Pow(0.55f, (float)num2);
			}
			if (Rand.Value < baseChance)
			{
				if (!humanlike.story.traits.HasTrait(TraitDefOf.Psychopath))
				{
					humanlike.relations.AddDirectRelation(PawnRelationDefOf.Bond, animal);
				}
				if (humanlike.Faction == Faction.OfPlayer || animal.Faction == Faction.OfPlayer)
				{
					TaleRecorder.RecordTale(TaleDefOf.BondedWithAnimal, new object[]
					{
						humanlike,
						animal
					});
				}
				bool flag = false;
				string text = null;
				if (animal.Name == null || animal.Name.Numerical)
				{
					flag = true;
					text = ((animal.Name != null) ? animal.Name.ToStringFull : animal.LabelIndefinite());
					animal.Name = PawnBioAndNameGenerator.GeneratePawnName(animal, NameStyle.Full, null);
				}
				if (PawnUtility.ShouldSendNotificationAbout(humanlike) || PawnUtility.ShouldSendNotificationAbout(animal))
				{
					string text2;
					if (flag)
					{
						text2 = "MessageNewBondRelationNewName".Translate(new object[]
						{
							humanlike.LabelShort,
							text,
							animal.Name.ToStringFull
						}).AdjustedFor(animal).CapitalizeFirst();
					}
					else
					{
						text2 = "MessageNewBondRelation".Translate(new object[]
						{
							humanlike.LabelShort,
							animal.LabelShort
						}).CapitalizeFirst();
					}
					Messages.Message(text2, humanlike, MessageSound.Benefit);
				}
				return true;
			}
			return false;
		}

		public static string LabelWithBondInfo(Pawn humanlike, Pawn animal)
		{
			string text = humanlike.LabelShort;
			if (humanlike.relations.DirectRelationExists(PawnRelationDefOf.Bond, animal))
			{
				text = text + " " + "BondBrackets".Translate();
			}
			return text;
		}

		private static bool HasAnySocialMemoryWith(Pawn p, Pawn otherPawn)
		{
			if (!p.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike)
			{
				return false;
			}
			if (p.Dead)
			{
				return false;
			}
			p.needs.mood.thoughts.GetMainThoughts(RelationsUtility.tmpThoughts);
			for (int i = 0; i < RelationsUtility.tmpThoughts.Count; i++)
			{
				Thought_MemorySocial thought_MemorySocial = RelationsUtility.tmpThoughts[i] as Thought_MemorySocial;
				if (thought_MemorySocial != null && thought_MemorySocial.otherPawn == otherPawn)
				{
					return true;
				}
			}
			RelationsUtility.tmpThoughts.Clear();
			return false;
		}
	}
}
