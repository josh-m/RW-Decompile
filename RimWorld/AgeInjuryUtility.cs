using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	internal static class AgeInjuryUtility
	{
		private const int MaxOldInjuryAge = 100;

		public static IEnumerable<HediffGiver_Birthday> RandomHediffsToGainOnBirthday(Pawn pawn, int age)
		{
			return AgeInjuryUtility.RandomHediffsToGainOnBirthday(pawn.def, age);
		}

		[DebuggerHidden]
		private static IEnumerable<HediffGiver_Birthday> RandomHediffsToGainOnBirthday(ThingDef raceDef, int age)
		{
			List<HediffGiverSetDef> sets = raceDef.race.hediffGiverSets;
			if (sets != null)
			{
				for (int i = 0; i < sets.Count; i++)
				{
					List<HediffGiver> givers = sets[i].hediffGivers;
					for (int j = 0; j < givers.Count; j++)
					{
						HediffGiver_Birthday agb = givers[j] as HediffGiver_Birthday;
						if (agb != null)
						{
							float ageFractionOfLifeExpectancy = (float)age / raceDef.race.lifeExpectancy;
							if (Rand.Value < agb.ageFractionChanceCurve.Evaluate(ageFractionOfLifeExpectancy))
							{
								yield return agb;
							}
						}
					}
				}
			}
		}

		public static void GenerateRandomOldAgeInjuries(Pawn pawn, bool tryNotToKillPawn)
		{
			int num = 0;
			for (int i = 10; i < Mathf.Min(pawn.ageTracker.AgeBiologicalYears, 120); i += 10)
			{
				if (Rand.Value < 0.15f)
				{
					num++;
				}
			}
			for (int j = 0; j < num; j++)
			{
				DamageDef dam = AgeInjuryUtility.RandomOldInjuryDamageType();
				int num2 = Rand.RangeInclusive(2, 6);
				IEnumerable<BodyPartRecord> source = from x in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
				where x.depth == BodyPartDepth.Outside && !Mathf.Approximately(x.def.oldInjuryBaseChance, 0f) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(x)
				select x;
				if (source.Any<BodyPartRecord>())
				{
					BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.absoluteFleshCoverage);
					HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dam, pawn, bodyPartRecord);
					if (bodyPartRecord.def.oldInjuryBaseChance > 0f && hediffDefFromDamage.CompPropsFor(typeof(HediffComp_GetsOld)) != null)
					{
						Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, pawn, null);
						hediff_Injury.Severity = (float)num2;
						hediff_Injury.TryGetComp<HediffComp_GetsOld>().IsOld = true;
						pawn.health.AddHediff(hediff_Injury, bodyPartRecord, null);
					}
				}
			}
			for (int k = 1; k < pawn.ageTracker.AgeBiologicalYears; k++)
			{
				foreach (HediffGiver_Birthday current in AgeInjuryUtility.RandomHediffsToGainOnBirthday(pawn, k))
				{
					current.TryApplyAndSimulateSeverityChange(pawn, (float)k, tryNotToKillPawn);
					if (pawn.Dead)
					{
						break;
					}
				}
				if (pawn.Dead)
				{
					break;
				}
			}
		}

		private static DamageDef RandomOldInjuryDamageType()
		{
			switch (Rand.RangeInclusive(0, 3))
			{
			case 0:
				return DamageDefOf.Bullet;
			case 1:
				return DamageDefOf.Scratch;
			case 2:
				return DamageDefOf.Bite;
			case 3:
				return DamageDefOf.Stab;
			default:
				throw new Exception();
			}
		}

		public static void LogOldInjuryCalculations()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("=======Theoretical injuries=========");
			for (int i = 0; i < 10; i++)
			{
				stringBuilder.AppendLine("#" + i + ":");
				List<HediffDef> list = new List<HediffDef>();
				for (int j = 0; j < 100; j++)
				{
					foreach (HediffGiver_Birthday current in AgeInjuryUtility.RandomHediffsToGainOnBirthday(ThingDefOf.Human, j))
					{
						if (!list.Contains(current.hediff))
						{
							list.Add(current.hediff);
							stringBuilder.AppendLine(string.Concat(new object[]
							{
								"  age ",
								j,
								" - ",
								current.hediff
							}));
						}
					}
				}
			}
			Log.Message(stringBuilder.ToString());
			stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("=======Actual injuries=========");
			for (int k = 0; k < 200; k++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
				if (pawn.ageTracker.AgeBiologicalYears >= 40)
				{
					stringBuilder.AppendLine(pawn.Name + " age " + pawn.ageTracker.AgeBiologicalYears);
					foreach (Hediff current2 in pawn.health.hediffSet.hediffs)
					{
						stringBuilder.AppendLine(" - " + current2);
					}
				}
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
