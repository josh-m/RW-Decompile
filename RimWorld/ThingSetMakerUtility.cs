using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ThingSetMakerUtility
	{
		public static List<ThingDef> allGeneratableItems = new List<ThingDef>();

		public static void Reset()
		{
			ThingSetMakerUtility.allGeneratableItems.Clear();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (ThingSetMakerUtility.CanGenerate(current))
				{
					ThingSetMakerUtility.allGeneratableItems.Add(current);
				}
			}
			ThingSetMaker_Meteorite.Reset();
		}

		public static bool CanGenerate(ThingDef thingDef)
		{
			return (thingDef.category == ThingCategory.Item || thingDef.Minifiable) && (thingDef.category != ThingCategory.Item || thingDef.EverHaulable) && !thingDef.isUnfinishedThing && !thingDef.IsCorpse && thingDef.PlayerAcquirable && thingDef.graphicData != null && !typeof(MinifiedThing).IsAssignableFrom(thingDef.thingClass);
		}

		public static IEnumerable<ThingDef> GetAllowedThingDefs(ThingSetMakerParams parms)
		{
			ThingSetMakerUtility.<GetAllowedThingDefs>c__AnonStorey0 <GetAllowedThingDefs>c__AnonStorey = new ThingSetMakerUtility.<GetAllowedThingDefs>c__AnonStorey0();
			<GetAllowedThingDefs>c__AnonStorey.parms = parms;
			ThingSetMakerUtility.<GetAllowedThingDefs>c__AnonStorey0 arg_33_0 = <GetAllowedThingDefs>c__AnonStorey;
			TechLevel? techLevel = <GetAllowedThingDefs>c__AnonStorey.parms.techLevel;
			arg_33_0.techLevel = ((!techLevel.HasValue) ? TechLevel.Undefined : techLevel.Value);
			IEnumerable<ThingDef> source = <GetAllowedThingDefs>c__AnonStorey.parms.filter.AllowedThingDefs;
			if (<GetAllowedThingDefs>c__AnonStorey.techLevel != TechLevel.Undefined)
			{
				source = from x in source
				where x.techLevel <= <GetAllowedThingDefs>c__AnonStorey.techLevel
				select x;
			}
			return source.Where(delegate(ThingDef x)
			{
				int arg_7F_0;
				if (ThingSetMakerUtility.CanGenerate(x))
				{
					float? maxThingMarketValue = <GetAllowedThingDefs>c__AnonStorey.parms.maxThingMarketValue;
					if (maxThingMarketValue.HasValue)
					{
						float? maxThingMarketValue2 = <GetAllowedThingDefs>c__AnonStorey.parms.maxThingMarketValue;
						if (!(x.BaseMarketValue <= maxThingMarketValue2))
						{
							goto IL_7E;
						}
					}
					arg_7F_0 = ((<GetAllowedThingDefs>c__AnonStorey.parms.validator == null || <GetAllowedThingDefs>c__AnonStorey.parms.validator(x)) ? 1 : 0);
					return arg_7F_0 != 0;
				}
				IL_7E:
				arg_7F_0 = 0;
				return arg_7F_0 != 0;
			});
		}

		public static void AssignQuality(Thing thing, QualityGenerator? qualityGenerator)
		{
			CompQuality compQuality = thing.TryGetComp<CompQuality>();
			if (compQuality != null)
			{
				QualityGenerator qualityGenerator2 = (!qualityGenerator.HasValue) ? QualityGenerator.BaseGen : qualityGenerator.Value;
				QualityCategory q = QualityUtility.GenerateQuality(qualityGenerator2);
				compQuality.SetQuality(q, ArtGenerationContext.Outsider);
			}
		}

		public static float AdjustedBigCategoriesSelectionWeight(ThingDef d, int numMeats, int numLeathers)
		{
			float num = 1f;
			if (d.IsMeat)
			{
				num *= Mathf.Min(5f / (float)numMeats, 1f);
			}
			if (d.IsLeather)
			{
				num *= Mathf.Min(5f / (float)numLeathers, 1f);
			}
			return num;
		}

		public static bool PossibleToWeighNoMoreThan(ThingDef t, float maxMass, IEnumerable<ThingDef> allowedStuff)
		{
			if (maxMass == 3.40282347E+38f || t.category == ThingCategory.Pawn)
			{
				return true;
			}
			if (maxMass < 0f)
			{
				return false;
			}
			if (t.MadeFromStuff)
			{
				foreach (ThingDef current in allowedStuff)
				{
					if (t.GetStatValueAbstract(StatDefOf.Mass, current) <= maxMass)
					{
						return true;
					}
				}
				return false;
			}
			return t.GetStatValueAbstract(StatDefOf.Mass, null) <= maxMass;
		}

		public static bool TryGetRandomThingWhichCanWeighNoMoreThan(IEnumerable<ThingDef> candidates, TechLevel stuffTechLevel, float maxMass, out ThingStuffPair thingStuffPair)
		{
			ThingDef thingDef;
			if (!(from x in candidates
			where ThingSetMakerUtility.PossibleToWeighNoMoreThan(x, maxMass, GenStuff.AllowedStuffsFor(x, stuffTechLevel))
			select x).TryRandomElement(out thingDef))
			{
				thingStuffPair = default(ThingStuffPair);
				return false;
			}
			ThingDef stuff;
			if (thingDef.MadeFromStuff)
			{
				if (!(from x in GenStuff.AllowedStuffsFor(thingDef, stuffTechLevel)
				where thingDef.GetStatValueAbstract(StatDefOf.Mass, x) <= maxMass
				select x).TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out stuff))
				{
					thingStuffPair = default(ThingStuffPair);
					return false;
				}
			}
			else
			{
				stuff = null;
			}
			thingStuffPair = new ThingStuffPair(thingDef, stuff, 1f);
			return true;
		}

		public static bool PossibleToWeighNoMoreThan(IEnumerable<ThingDef> candidates, TechLevel stuffTechLevel, float maxMass, int count)
		{
			if (maxMass == 3.40282347E+38f || count <= 0)
			{
				return true;
			}
			if (maxMass < 0f)
			{
				return false;
			}
			float num = 3.40282347E+38f;
			foreach (ThingDef current in candidates)
			{
				num = Mathf.Min(num, ThingSetMakerUtility.GetMinMass(current, stuffTechLevel));
			}
			return num <= maxMass * (float)count;
		}

		public static float GetMinMass(ThingDef thingDef, TechLevel stuffTechLevel)
		{
			float num = 3.40282347E+38f;
			if (thingDef.MadeFromStuff)
			{
				foreach (ThingDef current in GenStuff.AllowedStuffsFor(thingDef, stuffTechLevel))
				{
					if (current.stuffProps.commonality > 0f)
					{
						num = Mathf.Min(num, thingDef.GetStatValueAbstract(StatDefOf.Mass, current));
					}
				}
			}
			else
			{
				num = Mathf.Min(num, thingDef.GetStatValueAbstract(StatDefOf.Mass, null));
			}
			return num;
		}

		public static float GetMinMarketValue(ThingDef thingDef, TechLevel stuffTechLevel)
		{
			float num = 3.40282347E+38f;
			if (thingDef.MadeFromStuff)
			{
				foreach (ThingDef current in GenStuff.AllowedStuffsFor(thingDef, stuffTechLevel))
				{
					if (current.stuffProps.commonality > 0f)
					{
						num = Mathf.Min(num, StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(thingDef, current, QualityCategory.Awful), true));
					}
				}
			}
			else
			{
				num = Mathf.Min(num, StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(thingDef, null, QualityCategory.Awful), true));
			}
			return num;
		}
	}
}
