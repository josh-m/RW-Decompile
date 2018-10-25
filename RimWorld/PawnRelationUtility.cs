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
						try
						{
							bool anyNonKinFamilyByBloodRelation = false;
							List<PawnRelationDef> defs = DefDatabase<PawnRelationDef>.AllDefsListForReading;
							int i = 0;
							int count = defs.Count;
							while (i < count)
							{
								PawnRelationDef def = defs[i];
								if (def != PawnRelationDefOf.Kin)
								{
									if (def.Worker.InRelation(me, other))
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
							if (!anyNonKinFamilyByBloodRelation && PawnRelationDefOf.Kin.Worker.InRelation(me, other))
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

		public static void Notify_PawnsSeenByPlayer(IEnumerable<Pawn> seenPawns, out string pawnRelationsInfo, bool informEvenIfSeenBefore = false, bool writeSeenPawnsNames = true)
		{
			StringBuilder stringBuilder = new StringBuilder();
			IEnumerable<Pawn> enumerable = from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners
			where x.relations.everSeenByPlayer
			select x;
			bool flag = false;
			foreach (Pawn current in seenPawns)
			{
				if (current.RaceProps.IsFlesh)
				{
					if (informEvenIfSeenBefore || !current.relations.everSeenByPlayer)
					{
						current.relations.everSeenByPlayer = true;
						bool flag2 = false;
						foreach (Pawn current2 in enumerable)
						{
							if (current != current2)
							{
								PawnRelationDef mostImportantRelation = current2.GetMostImportantRelation(current);
								if (mostImportantRelation != null)
								{
									if (!flag2)
									{
										flag2 = true;
										if (flag)
										{
											stringBuilder.AppendLine();
										}
										if (writeSeenPawnsNames)
										{
											stringBuilder.AppendLine(current.KindLabel.CapitalizeFirst() + " " + current.LabelShort + ":");
										}
									}
									flag = true;
									stringBuilder.AppendLine("  " + "Relationship".Translate(mostImportantRelation.GetGenderSpecificLabelCap(current), current2.KindLabel + " " + current2.LabelShort, current2));
								}
							}
						}
					}
				}
			}
			if (flag)
			{
				pawnRelationsInfo = stringBuilder.ToString().TrimEndNewlines();
			}
			else
			{
				pawnRelationsInfo = null;
			}
		}

		public static void Notify_PawnsSeenByPlayer_Letter(IEnumerable<Pawn> seenPawns, ref string letterLabel, ref string letterText, string relationsInfoHeader, bool informEvenIfSeenBefore = false, bool writeSeenPawnsNames = true)
		{
			string text;
			PawnRelationUtility.Notify_PawnsSeenByPlayer(seenPawns, out text, informEvenIfSeenBefore, writeSeenPawnsNames);
			if (!text.NullOrEmpty())
			{
				if (letterLabel.NullOrEmpty())
				{
					letterLabel = "LetterLabelNoticedRelatedPawns".Translate();
				}
				else
				{
					letterLabel = letterLabel + " " + "RelationshipAppendedLetterSuffix".Translate();
				}
				if (!letterText.NullOrEmpty())
				{
					letterText += "\n\n";
				}
				letterText = letterText + relationsInfoHeader + "\n\n" + text;
			}
		}

		public static void Notify_PawnsSeenByPlayer_Letter_Send(IEnumerable<Pawn> seenPawns, string relationsInfoHeader, LetterDef letterDef, bool informEvenIfSeenBefore = false, bool writeSeenPawnsNames = true)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(seenPawns, ref empty, ref empty2, relationsInfoHeader, informEvenIfSeenBefore, writeSeenPawnsNames);
			if (!empty2.NullOrEmpty())
			{
				Pawn pawn = null;
				foreach (Pawn current in seenPawns)
				{
					if (PawnRelationUtility.GetMostImportantColonyRelative(current) != null)
					{
						pawn = current;
						break;
					}
				}
				if (pawn == null)
				{
					pawn = seenPawns.FirstOrDefault<Pawn>();
				}
				Find.LetterStack.ReceiveLetter(empty, empty2, letterDef, pawn, null, null);
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
			if (mostImportantColonyRelative.IsColonist)
			{
				text = text + "\n\n" + "RelationshipAppendedLetterTextColonist".Translate(mostImportantColonyRelative.LabelShort, genderSpecificLabel, mostImportantColonyRelative.Named("RELATIVE"), pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN");
			}
			else
			{
				text = text + "\n\n" + "RelationshipAppendedLetterTextPrisoner".Translate(mostImportantColonyRelative.LabelShort, genderSpecificLabel, mostImportantColonyRelative.Named("RELATIVE"), pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN");
			}
			return true;
		}

		public static Pawn GetMostImportantColonyRelative(Pawn pawn)
		{
			if (pawn.relations == null || !pawn.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
			{
				return null;
			}
			IEnumerable<Pawn> enumerable = from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners
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
