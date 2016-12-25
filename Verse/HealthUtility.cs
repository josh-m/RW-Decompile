using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class HealthUtility
	{
		public static readonly Color GoodConditionColor = new Color(0.6f, 0.8f, 0.65f);

		public static readonly Color DarkRedColor = new Color(0.73f, 0.02f, 0.02f);

		public static readonly Color ImpairedColor = new Color(0.9f, 0.7f, 0f);

		public static readonly Color SlightlyImpairedColor = new Color(0.9f, 0.9f, 0f);

		public static string GetGeneralConditionLabel(Pawn pawn)
		{
			if (pawn.health.Dead)
			{
				return "Dead".Translate();
			}
			if (!pawn.health.capacities.CanBeAwake)
			{
				return "Unconscious".Translate();
			}
			if (pawn.health.InPainShock)
			{
				return "PainShock".Translate();
			}
			if (pawn.Downed)
			{
				return "Incapacitated".Translate();
			}
			bool flag = false;
			for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
			{
				Hediff_Injury hediff_Injury = pawn.health.hediffSet.hediffs[i] as Hediff_Injury;
				if (hediff_Injury != null)
				{
					if (!hediff_Injury.IsOld())
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				return "Injured".Translate();
			}
			if (pawn.health.hediffSet.Pain > 0.3f)
			{
				return "InPain".Translate();
			}
			return "Healthy".Translate();
		}

		public static Pair<string, Color> GetPartConditionLabel(Pawn pawn, BodyPartRecord part)
		{
			float partHealth = pawn.health.hediffSet.GetPartHealth(part);
			float maxHealth = part.def.GetMaxHealth(pawn);
			float num = partHealth / maxHealth;
			string first = string.Empty;
			Color second = Color.white;
			if (partHealth <= 0f)
			{
				Hediff_MissingPart hediff_MissingPart = null;
				List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				for (int i = 0; i < missingPartsCommonAncestors.Count; i++)
				{
					if (missingPartsCommonAncestors[i].Part == part)
					{
						hediff_MissingPart = missingPartsCommonAncestors[i];
						break;
					}
				}
				if (hediff_MissingPart == null)
				{
					bool fresh = false;
					if (hediff_MissingPart != null && hediff_MissingPart.IsFresh)
					{
						fresh = true;
					}
					bool solid = part.def.IsSolid(part, pawn.health.hediffSet.hediffs);
					first = HealthUtility.GetGeneralDestroyedPartLabel(part, fresh, solid);
					second = Color.gray;
				}
				else
				{
					first = hediff_MissingPart.LabelCap;
					second = hediff_MissingPart.LabelColor;
				}
			}
			else if (num < 0.4f)
			{
				first = "SeriouslyImpaired".Translate();
				second = HealthUtility.DarkRedColor;
			}
			else if (num < 0.7f)
			{
				first = "Impaired".Translate();
				second = HealthUtility.ImpairedColor;
			}
			else if (num < 0.999f)
			{
				first = "SlightlyImpaired".Translate();
				second = HealthUtility.SlightlyImpairedColor;
			}
			else
			{
				first = "GoodCondition".Translate();
				second = HealthUtility.GoodConditionColor;
			}
			return new Pair<string, Color>(first, second);
		}

		public static string GetGeneralDestroyedPartLabel(BodyPartRecord part, bool fresh, bool solid)
		{
			if (part.parent == null)
			{
				return "SeriouslyImpaired".Translate();
			}
			if (part.depth != BodyPartDepth.Inside && !fresh)
			{
				return "MissingBodyPart".Translate();
			}
			if (solid)
			{
				return "ShatteredBodyPart".Translate();
			}
			return "DestroyedBodyPart".Translate();
		}

		private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
		{
			return from x in bodyModel.GetNotMissingParts(null, null)
			where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
			select x;
		}

		public static void GiveInjuriesOperationFailureCatastrophic(Pawn p)
		{
			HealthUtility.GiveSurgeryInjuries(p, 65);
		}

		public static void GiveInjuriesOperationFailureMinor(Pawn p)
		{
			HealthUtility.GiveSurgeryInjuries(p, 20);
		}

		private static void GiveSurgeryInjuries(Pawn p, int totalDamage)
		{
			HediffSet hediffSet = p.health.hediffSet;
			IEnumerable<BodyPartRecord> notMissingParts = hediffSet.GetNotMissingParts(null, null);
			while (totalDamage > 0 && notMissingParts.Any<BodyPartRecord>())
			{
				BodyPartRecord part = notMissingParts.RandomElementByWeight((BodyPartRecord x) => x.absoluteFleshCoverage);
				float partHealth = hediffSet.GetPartHealth(part);
				int num = Mathf.RoundToInt(partHealth * Rand.Range(0.65f, 1f));
				DamageInfo dinfo = new DamageInfo(DamageDefOf.SurgicalCut, num, null, new BodyPartDamageInfo?(new BodyPartDamageInfo(part, false, null)), null);
				p.TakeDamage(dinfo);
				totalDamage -= num;
			}
		}

		public static void GiveInjuriesToForceDowned(Pawn p)
		{
			if (p.health.Downed)
			{
				return;
			}
			HediffSet hediffSet = p.health.hediffSet;
			p.health.forceIncap = true;
			int num = 0;
			while (num < 300 && !p.Downed && HealthUtility.HittablePartsViolence(hediffSet).Any<BodyPartRecord>())
			{
				num++;
				BodyPartRecord bodyPartRecord = HealthUtility.HittablePartsViolence(hediffSet).RandomElementByWeight((BodyPartRecord x) => x.absoluteFleshCoverage);
				int num2 = Mathf.RoundToInt(hediffSet.GetPartHealth(bodyPartRecord)) - 3;
				if (num2 >= 8)
				{
					DamageDef def;
					if (bodyPartRecord.depth == BodyPartDepth.Outside)
					{
						def = HealthUtility.RandomViolenceDamageType();
					}
					else
					{
						def = DamageDefOf.Blunt;
					}
					p.TakeDamage(new DamageInfo(def, Rand.RangeInclusive(Mathf.RoundToInt((float)num2 * 0.65f), num2), null, new BodyPartDamageInfo?(new BodyPartDamageInfo(bodyPartRecord, false, null)), null)
					{
						AllowDamagePropagation = false
					});
				}
			}
			if (p.Dead)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(p + " died during GiveInjuriesToForceDowned");
				for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
				{
					stringBuilder.AppendLine("   -" + p.health.hediffSet.hediffs[i].ToString());
				}
				Log.Error(stringBuilder.ToString());
			}
			p.health.forceIncap = false;
		}

		public static void GiveInjuriesToKill(Pawn p)
		{
			HediffSet hediffSet = p.health.hediffSet;
			int num = 0;
			while (!p.Dead && num < 100 && HealthUtility.HittablePartsViolence(hediffSet).Any<BodyPartRecord>())
			{
				num++;
				BodyPartRecord bodyPartRecord = HealthUtility.HittablePartsViolence(hediffSet).RandomElementByWeight((BodyPartRecord x) => x.absoluteFleshCoverage);
				int amount = Rand.RangeInclusive(8, 25);
				DamageDef def;
				if (bodyPartRecord.depth == BodyPartDepth.Outside)
				{
					def = HealthUtility.RandomViolenceDamageType();
				}
				else
				{
					def = DamageDefOf.Blunt;
				}
				DamageInfo dinfo = new DamageInfo(def, amount, null, new BodyPartDamageInfo?(new BodyPartDamageInfo(bodyPartRecord, false, null)), null);
				p.TakeDamage(dinfo);
			}
			if (!p.Dead)
			{
				Log.Error(p + " not killed during GiveInjuriesToKill");
			}
		}

		public static DamageDef RandomViolenceDamageType()
		{
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				return DamageDefOf.Bullet;
			case 1:
				return DamageDefOf.Blunt;
			case 2:
				return DamageDefOf.Stab;
			case 3:
				return DamageDefOf.Scratch;
			case 4:
				return DamageDefOf.Cut;
			default:
				return null;
			}
		}

		public static HediffDef GetHediffDefFromDamage(DamageDef dam, Pawn pawn, BodyPartRecord part)
		{
			HediffDef result = dam.hediff;
			if (part.def.IsSkinCovered(part, pawn.health.hediffSet) && dam.hediffSkin != null)
			{
				result = dam.hediffSkin;
			}
			if (part.def.IsSolid(part, pawn.health.hediffSet.hediffs) && dam.hediffSolid != null)
			{
				result = dam.hediffSolid;
			}
			return result;
		}

		public static void TryAnesthesize(Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return;
			}
			pawn.health.forceIncap = true;
			pawn.health.AddHediff(HediffDefOf.Anesthetic, null, null);
			pawn.health.forceIncap = false;
		}

		public static void AdjustSeverity(Pawn pawn, HediffDef hdDef, float sevOffset)
		{
			if (sevOffset == 0f)
			{
				return;
			}
			Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hdDef);
			if (hediff != null)
			{
				hediff.Severity += sevOffset;
				pawn.health.Notify_HediffChanged(hediff);
			}
			else if (sevOffset > 0f)
			{
				hediff = HediffMaker.MakeHediff(hdDef, pawn, null);
				hediff.Severity = sevOffset;
				pawn.health.AddHediff(hediff, null, null);
			}
		}

		public static BodyPartRemovalIntent PartRemovalIntent(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.health.hediffSet.hediffs.Any((Hediff d) => d.Visible && d.Part == part && d.def.isBad))
			{
				return BodyPartRemovalIntent.Amputate;
			}
			return BodyPartRemovalIntent.Harvest;
		}

		public static bool PawnShouldGetImmediateTending(Pawn pawn)
		{
			if (!pawn.health.ShouldBeTendedNow)
			{
				return false;
			}
			float bleedingRate = pawn.health.hediffSet.BleedingRate;
			if (bleedingRate < 0.001f)
			{
				return false;
			}
			float num = 0f;
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
			if (firstHediffOfDef != null)
			{
				num = firstHediffOfDef.Severity;
			}
			return (bleedingRate > 0.9f && num > 0.2f) || (bleedingRate > 0.7f && num > 0.4f) || (bleedingRate > 0.001f && num > 0.6f);
		}
	}
}
