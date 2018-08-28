using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Nutrition : ThingSetMaker
	{
		private int nextSeed;

		public ThingSetMaker_Nutrition()
		{
			this.nextSeed = Rand.Int;
		}

		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			if (!this.AllowedThingDefs(parms).Any<ThingDef>())
			{
				return false;
			}
			IntRange? countRange = parms.countRange;
			if (countRange.HasValue && parms.countRange.Value.max <= 0)
			{
				return false;
			}
			FloatRange? totalNutritionRange = parms.totalNutritionRange;
			if (!totalNutritionRange.HasValue || parms.totalNutritionRange.Value.max <= 0f)
			{
				return false;
			}
			float? maxTotalMass = parms.maxTotalMass;
			if (maxTotalMass.HasValue && parms.maxTotalMass != 3.40282347E+38f)
			{
				IEnumerable<ThingDef> arg_11D_0 = this.AllowedThingDefs(parms);
				TechLevel? techLevel = parms.techLevel;
				TechLevel arg_11D_1 = (!techLevel.HasValue) ? TechLevel.Undefined : techLevel.Value;
				float arg_11D_2 = parms.maxTotalMass.Value;
				IntRange? countRange2 = parms.countRange;
				if (!ThingSetMakerUtility.PossibleToWeighNoMoreThan(arg_11D_0, arg_11D_1, arg_11D_2, (!countRange2.HasValue) ? 1 : parms.countRange.Value.min))
				{
					return false;
				}
			}
			float num;
			return this.GeneratePossibleDefs(parms, out num, this.nextSeed).Any<ThingStuffPairWithQuality>();
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			float? maxTotalMass = parms.maxTotalMass;
			float maxMass = (!maxTotalMass.HasValue) ? 3.40282347E+38f : maxTotalMass.Value;
			float totalValue;
			List<ThingStuffPairWithQuality> list = this.GeneratePossibleDefs(parms, out totalValue, this.nextSeed);
			for (int i = 0; i < list.Count; i++)
			{
				outThings.Add(list[i].MakeThing());
			}
			ThingSetMakerByTotalStatUtility.IncreaseStackCountsToTotalValue(outThings, totalValue, (Thing x) => x.GetStatValue(StatDefOf.Nutrition, true), maxMass);
			this.nextSeed++;
		}

		protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
		{
			return ThingSetMakerUtility.GetAllowedThingDefs(parms);
		}

		private List<ThingStuffPairWithQuality> GeneratePossibleDefs(ThingSetMakerParams parms, out float totalNutrition, int seed)
		{
			Rand.PushState(seed);
			List<ThingStuffPairWithQuality> result = this.GeneratePossibleDefs(parms, out totalNutrition);
			Rand.PopState();
			return result;
		}

		private List<ThingStuffPairWithQuality> GeneratePossibleDefs(ThingSetMakerParams parms, out float totalNutrition)
		{
			IEnumerable<ThingDef> enumerable = this.AllowedThingDefs(parms);
			if (!enumerable.Any<ThingDef>())
			{
				totalNutrition = 0f;
				return new List<ThingStuffPairWithQuality>();
			}
			IntRange? countRange = parms.countRange;
			IntRange intRange = (!countRange.HasValue) ? new IntRange(1, 2147483647) : countRange.Value;
			FloatRange? totalNutritionRange = parms.totalNutritionRange;
			FloatRange floatRange = (!totalNutritionRange.HasValue) ? FloatRange.Zero : totalNutritionRange.Value;
			TechLevel? techLevel = parms.techLevel;
			TechLevel techLevel2 = (!techLevel.HasValue) ? TechLevel.Undefined : techLevel.Value;
			float? maxTotalMass = parms.maxTotalMass;
			float num = (!maxTotalMass.HasValue) ? 3.40282347E+38f : maxTotalMass.Value;
			QualityGenerator? qualityGenerator = parms.qualityGenerator;
			QualityGenerator qualityGenerator2 = (!qualityGenerator.HasValue) ? QualityGenerator.BaseGen : qualityGenerator.Value;
			totalNutrition = floatRange.RandomInRange;
			int numMeats = enumerable.Count((ThingDef x) => x.IsMeat);
			int numLeathers = enumerable.Count((ThingDef x) => x.IsLeather);
			Func<ThingDef, float> func = (ThingDef x) => ThingSetMakerUtility.AdjustedBigCategoriesSelectionWeight(x, numMeats, numLeathers);
			IntRange countRange2 = intRange;
			float totalValue = totalNutrition;
			IEnumerable<ThingDef> allowed = enumerable;
			TechLevel techLevel3 = techLevel2;
			QualityGenerator qualityGenerator3 = qualityGenerator2;
			Func<ThingStuffPairWithQuality, float> getMinValue = (ThingStuffPairWithQuality x) => x.GetStatValue(StatDefOf.Nutrition);
			Func<ThingStuffPairWithQuality, float> getMaxValue = (ThingStuffPairWithQuality x) => x.GetStatValue(StatDefOf.Nutrition) * (float)x.thing.stackLimit;
			Func<ThingDef, float> weightSelector = func;
			float maxMass = num;
			return ThingSetMakerByTotalStatUtility.GenerateDefsWithPossibleTotalValue(countRange2, totalValue, allowed, techLevel3, qualityGenerator3, getMinValue, getMaxValue, weightSelector, 100, maxMass);
		}

		[DebuggerHidden]
		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			TechLevel? techLevel2 = parms.techLevel;
			TechLevel techLevel = (!techLevel2.HasValue) ? TechLevel.Undefined : techLevel2.Value;
			foreach (ThingDef t in this.AllowedThingDefs(parms))
			{
				float? maxTotalMass = parms.maxTotalMass;
				if (maxTotalMass.HasValue && parms.maxTotalMass != 3.40282347E+38f)
				{
					float? maxTotalMass2 = parms.maxTotalMass;
					if (ThingSetMakerUtility.GetMinMass(t, techLevel) > maxTotalMass2)
					{
						continue;
					}
				}
				FloatRange? totalNutritionRange = parms.totalNutritionRange;
				if (!totalNutritionRange.HasValue || parms.totalNutritionRange.Value.max == 3.40282347E+38f || !t.IsNutritionGivingIngestible || t.ingestible.CachedNutrition <= parms.totalNutritionRange.Value.max)
				{
					yield return t;
				}
			}
		}
	}
}
