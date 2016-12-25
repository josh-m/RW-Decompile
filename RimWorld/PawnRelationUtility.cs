using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PawnRelationUtility
	{
		[DebuggerHidden]
		public static IEnumerable<PawnRelationDef> GetRelations(this Pawn me, Pawn other)
		{
			if (me != other)
			{
				if (me.RaceProps.IsFlesh && other.RaceProps.IsFlesh)
				{
					if (me.relations.RelatedToAnyoneOrAnyoneRelatedToMe && other.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
					{
						ProfilerThreadCheck.BeginSample("GetRelations()");
						try
						{
							bool isKin = false;
							bool anyNonKinFamilyByBloodRelation = false;
							List<PawnRelationDef> defs = DefDatabase<PawnRelationDef>.AllDefsListForReading;
							int i = 0;
							int count = defs.Count;
							while (i < count)
							{
								PawnRelationDef def = defs[i];
								if (def.Worker.InRelation(me, other))
								{
									if (def == PawnRelationDefOf.Kin)
									{
										isKin = true;
									}
									else
									{
										if (def.familyByBloodRelation)
										{
											anyNonKinFamilyByBloodRelation = true;
										}
										yield return def;
									}
								}
								i++;
							}
							if (isKin && !anyNonKinFamilyByBloodRelation)
							{
								yield return PawnRelationDefOf.Kin;
							}
						}
						finally
						{
							base.<>__Finally0();
						}
					}
				}
			}
		}

		public static PawnRelationDef GetMostImportantRelation(this Pawn me, Pawn other)
		{
			PawnRelationDef pawnRelationDef = null;
			foreach (PawnRelationDef current in me.GetRelations(other))
			{
				if (pawnRelationDef == null || current.importance > pawnRelationDef.importance)
				{
					pawnRelationDef = current;
				}
			}
			return pawnRelationDef;
		}

		public static void Notify_PawnsSeenByPlayer(IEnumerable<Pawn> seenPawns, string letterHeader, bool informEvenIfSeenBefore = false)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(letterHeader);
			IEnumerable<Pawn> enumerable = from x in seenPawns
			where x.RaceProps.IsFlesh
			select x;
			IEnumerable<Pawn> enumerable2 = from x in Find.MapPawns.FreeColonistsAndPrisoners
			where x.relations.everSeenByPlayer
			select x;
			if (!informEvenIfSeenBefore)
			{
				enumerable = from x in enumerable
				where !x.relations.everSeenByPlayer
				select x;
			}
			bool flag = false;
			Pawn pawn = null;
			foreach (Pawn current in enumerable)
			{
				bool flag2 = false;
				foreach (Pawn current2 in enumerable2)
				{
					if (current != current2)
					{
						PawnRelationDef mostImportantRelation = current.GetMostImportantRelation(current2);
						if (mostImportantRelation != null)
						{
							if (!flag2)
							{
								flag2 = true;
								stringBuilder.AppendLine();
								stringBuilder.AppendLine(current.KindLabel.CapitalizeFirst() + " " + current.LabelShort + ":");
							}
							if (current.Spawned)
							{
								pawn = current;
							}
							flag = true;
							stringBuilder.AppendLine(string.Concat(new string[]
							{
								"  ",
								mostImportantRelation.GetGenderSpecificLabelCap(current2),
								" - ",
								current2.KindLabel,
								" ",
								current2.LabelShort
							}));
							current.relations.everSeenByPlayer = true;
						}
					}
				}
			}
			if (flag)
			{
				if (pawn != null)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelNoticedRelatedPawns".Translate(), stringBuilder.ToString().TrimEndNewlines(), LetterType.Good, pawn, null);
				}
				else
				{
					Find.LetterStack.ReceiveLetter("LetterLabelNoticedRelatedPawns".Translate(), stringBuilder.ToString().TrimEndNewlines(), LetterType.Good, null);
				}
			}
		}

		public static bool TryAppendRelationsWithColonistsInfo(ref string text, Pawn pawn)
		{
			string text2 = null;
			return PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref text2, pawn);
		}

		public static bool TryAppendRelationsWithColonistsInfo(ref string text, ref string title, Pawn pawn)
		{
			Pawn mostImportantColonyRelative = PawnRelationUtility.GetMostImportantColonyRelative(pawn);
			if (mostImportantColonyRelative == null)
			{
				return false;
			}
			if (title != null)
			{
				title = title + " " + "RelationshipAppendedLetterSuffix".Translate();
			}
			string genderSpecificLabel = mostImportantColonyRelative.GetMostImportantRelation(pawn).GetGenderSpecificLabel(pawn);
			string text2 = "\n\n";
			if (mostImportantColonyRelative.IsColonist)
			{
				text2 += "RelationshipAppendedLetterTextColonist".Translate(new object[]
				{
					mostImportantColonyRelative.LabelShort,
					genderSpecificLabel
				});
			}
			else
			{
				text2 += "RelationshipAppendedLetterTextPrisoner".Translate(new object[]
				{
					mostImportantColonyRelative.LabelShort,
					genderSpecificLabel
				});
			}
			text += text2.AdjustedFor(pawn);
			return true;
		}

		public static Pawn GetMostImportantColonyRelative(Pawn pawn)
		{
			IEnumerable<Pawn> enumerable = from x in Find.MapPawns.FreeColonistsAndPrisoners
			where x.relations.everSeenByPlayer
			select x;
			float num = 0f;
			Pawn pawn2 = null;
			foreach (Pawn current in enumerable)
			{
				PawnRelationDef mostImportantRelation = pawn.GetMostImportantRelation(current);
				if (mostImportantRelation != null)
				{
					if (pawn2 == null || mostImportantRelation.importance > num)
					{
						num = mostImportantRelation.importance;
						pawn2 = current;
					}
				}
			}
			return pawn2;
		}

		public static float MaxPossibleBioAgeAt(float myBiologicalAge, float myChronologicalAge, float atChronologicalAge)
		{
			float num = Mathf.Min(myBiologicalAge, myChronologicalAge - atChronologicalAge);
			if (num < 0f)
			{
				return -1f;
			}
			return num;
		}

		public static float MinPossibleBioAgeAt(float myBiologicalAge, float atChronologicalAge)
		{
			return Mathf.Max(myBiologicalAge - atChronologicalAge, 0f);
		}
	}
}
