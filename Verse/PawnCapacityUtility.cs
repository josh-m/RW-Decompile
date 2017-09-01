using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class PawnCapacityUtility
	{
		public abstract class CapacityImpactor
		{
			public abstract string Readable(Pawn pawn);
		}

		public class CapacityImpactorBodyPartHealth : PawnCapacityUtility.CapacityImpactor
		{
			public BodyPartRecord bodyPart;

			public override string Readable(Pawn pawn)
			{
				return string.Format("{0}: {1} / {2}", this.bodyPart.def.LabelCap, pawn.health.hediffSet.GetPartHealth(this.bodyPart), this.bodyPart.def.GetMaxHealth(pawn));
			}
		}

		public class CapacityImpactorCapacity : PawnCapacityUtility.CapacityImpactor
		{
			public PawnCapacityDef capacity;

			public override string Readable(Pawn pawn)
			{
				return string.Format("{0}: {1}%", this.capacity.LabelCap, (pawn.health.capacities.GetLevel(this.capacity) * 100f).ToString("F0"));
			}
		}

		public class CapacityImpactorHediff : PawnCapacityUtility.CapacityImpactor
		{
			public Hediff hediff;

			public override string Readable(Pawn pawn)
			{
				return string.Format("{0}", this.hediff.LabelCap);
			}
		}

		public class CapacityImpactorPain : PawnCapacityUtility.CapacityImpactor
		{
			public override string Readable(Pawn pawn)
			{
				return string.Format("{0}: {1}%", "Pain".Translate(), (pawn.health.hediffSet.PainTotal * 100f).ToString("F0"));
			}
		}

		public static bool BodyCanEverDoCapacity(BodyDef bodyDef, PawnCapacityDef capacity)
		{
			return capacity.Worker.CanHaveCapacity(bodyDef);
		}

		public static float CalculateCapacityLevel(HediffSet diffSet, PawnCapacityDef capacity, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			if (capacity.zeroIfCannotBeAwake && !diffSet.pawn.health.capacities.CanBeAwake)
			{
				if (impactors != null)
				{
					impactors.Add(new PawnCapacityUtility.CapacityImpactorCapacity
					{
						capacity = PawnCapacityDefOf.Consciousness
					});
				}
				return 0f;
			}
			float num = capacity.Worker.CalculateCapacityLevel(diffSet, impactors);
			if (num > 0f && capacity.minValue <= 0f)
			{
				float num2 = 99999f;
				float num3 = 1f;
				for (int i = 0; i < diffSet.hediffs.Count; i++)
				{
					Hediff hediff = diffSet.hediffs[i];
					List<PawnCapacityModifier> capMods = hediff.CapMods;
					if (capMods != null)
					{
						for (int j = 0; j < capMods.Count; j++)
						{
							PawnCapacityModifier pawnCapacityModifier = capMods[j];
							if (pawnCapacityModifier.capacity == capacity)
							{
								num += pawnCapacityModifier.offset;
								num3 *= pawnCapacityModifier.postFactor;
								if (pawnCapacityModifier.setMax < num2)
								{
									num2 = pawnCapacityModifier.setMax;
								}
								if (impactors != null)
								{
									impactors.Add(new PawnCapacityUtility.CapacityImpactorHediff
									{
										hediff = hediff
									});
								}
							}
						}
					}
				}
				num *= num3;
				num = Mathf.Min(num, num2);
			}
			num = Mathf.Max(num, capacity.minValue);
			return GenMath.RoundedHundredth(num);
		}

		public static float CalculatePartEfficiency(HediffSet diffSet, BodyPartRecord part, bool ignoreAddedParts = false, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			BodyPartRecord rec;
			for (rec = part.parent; rec != null; rec = rec.parent)
			{
				if (diffSet.HasDirectlyAddedPartFor(rec))
				{
					Hediff_AddedPart hediff_AddedPart = (from x in diffSet.GetHediffs<Hediff_AddedPart>()
					where x.Part == rec
					select x).First<Hediff_AddedPart>();
					if (impactors != null)
					{
						impactors.Add(new PawnCapacityUtility.CapacityImpactorHediff
						{
							hediff = hediff_AddedPart
						});
					}
					return hediff_AddedPart.def.addedPartProps.partEfficiency;
				}
			}
			if (part.parent != null && diffSet.PartIsMissing(part.parent))
			{
				return 0f;
			}
			float num = 1f;
			if (!ignoreAddedParts)
			{
				for (int i = 0; i < diffSet.hediffs.Count; i++)
				{
					Hediff_AddedPart hediff_AddedPart2 = diffSet.hediffs[i] as Hediff_AddedPart;
					if (hediff_AddedPart2 != null && hediff_AddedPart2.Part == part)
					{
						num *= hediff_AddedPart2.def.addedPartProps.partEfficiency;
						if (hediff_AddedPart2.def.addedPartProps.partEfficiency != 1f && impactors != null)
						{
							impactors.Add(new PawnCapacityUtility.CapacityImpactorHediff
							{
								hediff = hediff_AddedPart2
							});
						}
					}
				}
			}
			float b = -1f;
			float num2 = 0f;
			bool flag = false;
			for (int j = 0; j < diffSet.hediffs.Count; j++)
			{
				if (diffSet.hediffs[j].Part == part && diffSet.hediffs[j].CurStage != null)
				{
					HediffStage curStage = diffSet.hediffs[j].CurStage;
					num2 += curStage.partEfficiencyOffset;
					flag |= curStage.partIgnoreMissingHP;
					if (curStage.partEfficiencyOffset != 0f && curStage.everVisible && impactors != null)
					{
						impactors.Add(new PawnCapacityUtility.CapacityImpactorHediff
						{
							hediff = diffSet.hediffs[j]
						});
					}
				}
			}
			if (!flag)
			{
				float num3 = diffSet.GetPartHealth(part) / part.def.GetMaxHealth(diffSet.pawn);
				if (num3 != 1f && impactors != null)
				{
					impactors.Add(new PawnCapacityUtility.CapacityImpactorBodyPartHealth
					{
						bodyPart = part
					});
				}
				num *= num3;
			}
			num += num2;
			if (num > 0.0001f)
			{
				num = Mathf.Max(num, b);
			}
			return Mathf.Max(num, 0f);
		}

		public static float CalculateImmediatePartEfficiencyAndRecord(HediffSet diffSet, BodyPartRecord part, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			if (diffSet.AncestorHasDirectlyAddedParts(part))
			{
				return 1f;
			}
			return PawnCapacityUtility.CalculatePartEfficiency(diffSet, part, false, impactors);
		}

		public static float CalculateNaturalPartsAverageEfficiency(HediffSet diffSet, BodyPartGroupDef bodyPartGroup)
		{
			float num = 0f;
			int num2 = 0;
			IEnumerable<BodyPartRecord> enumerable = from x in diffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
			where x.groups.Contains(bodyPartGroup)
			select x;
			foreach (BodyPartRecord current in enumerable)
			{
				if (!diffSet.PartOrAnyAncestorHasDirectlyAddedParts(current))
				{
					num += PawnCapacityUtility.CalculatePartEfficiency(diffSet, current, false, null);
				}
				num2++;
			}
			if (num2 == 0 || num < 0f)
			{
				return 0f;
			}
			return num / (float)num2;
		}

		public static float CalculateTagEfficiency(HediffSet diffSet, string tag, float maximum = 3.40282347E+38f, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			BodyDef body = diffSet.pawn.RaceProps.body;
			float num = 0f;
			int num2 = 0;
			List<PawnCapacityUtility.CapacityImpactor> list = null;
			foreach (BodyPartRecord current in body.GetPartsWithTag(tag))
			{
				List<PawnCapacityUtility.CapacityImpactor> impactors2 = list;
				float num3 = PawnCapacityUtility.CalculatePartEfficiency(diffSet, current, false, impactors2);
				if (impactors != null && num3 != 1f && list == null)
				{
					list = new List<PawnCapacityUtility.CapacityImpactor>();
					impactors2 = list;
					PawnCapacityUtility.CalculatePartEfficiency(diffSet, current, false, impactors2);
				}
				num += num3;
				num2++;
			}
			if (num2 == 0)
			{
				return 1f;
			}
			float num4 = num / (float)num2;
			float num5 = Mathf.Min(num4, maximum);
			if (impactors != null && list != null && (maximum != 1f || num4 <= 1f || num5 == 1f))
			{
				impactors.AddRange(list);
			}
			return num5;
		}

		public static float CalculateLimbEfficiency(HediffSet diffSet, string limbCoreTag, string limbSegmentTag, string limbDigitTag, float appendageWeight, out float functionalPercentage, List<PawnCapacityUtility.CapacityImpactor> impactors)
		{
			BodyDef body = diffSet.pawn.RaceProps.body;
			float num = 0f;
			int num2 = 0;
			int num3 = 0;
			foreach (BodyPartRecord current in body.GetPartsWithTag(limbCoreTag))
			{
				float num4 = PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord(diffSet, current, impactors);
				foreach (BodyPartRecord current2 in current.GetConnectedParts(limbSegmentTag))
				{
					num4 *= PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord(diffSet, current2, impactors);
				}
				if (current.HasChildParts(limbDigitTag))
				{
					num4 = Mathf.Lerp(num4, num4 * current.GetChildParts(limbDigitTag).Average((BodyPartRecord digitPart) => PawnCapacityUtility.CalculateImmediatePartEfficiencyAndRecord(diffSet, digitPart, impactors)), appendageWeight);
				}
				num += num4;
				num2++;
				if (num4 > 0f)
				{
					num3++;
				}
			}
			if (num2 == 0)
			{
				functionalPercentage = 0f;
				return 0f;
			}
			functionalPercentage = (float)num3 / (float)num2;
			return num / (float)num2;
		}
	}
}
