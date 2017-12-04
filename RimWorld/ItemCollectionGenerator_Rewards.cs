using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_Rewards : ItemCollectionGenerator
	{
		private class Option
		{
			public ThingDef thingDef;

			public QualityCategory quality;

			public ThingDef stuff;
		}

		private const float SilverChance = 0.5f;

		private const float SpecialRewardChance = 0.35f;

		private const float ExpensiveMineableResourceChance = 0.13f;

		private const float ExpensiveMineableResourceValueFactor = 0.9f;

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			int? count = parms.count;
			int num = (!count.HasValue) ? Rand.RangeInclusive(1, 3) : count.Value;
			float? totalMarketValue = parms.totalMarketValue;
			float num2 = (!totalMarketValue.HasValue) ? Rand.Range(1500f, 4000f) : totalMarketValue.Value;
			TechLevel? techLevel = parms.techLevel;
			TechLevel techLevel2 = (!techLevel.HasValue) ? TechLevel.Spacer : techLevel.Value;
			Predicate<ThingDef> validator = parms.validator;
			for (int i = 0; i < num; i++)
			{
				outThings.Add(this.GenerateReward(num2 / (float)num, techLevel2, validator));
			}
		}

		private Thing GenerateReward(float value, TechLevel techLevel, Predicate<ThingDef> validator = null)
		{
			if (Rand.Chance(0.5f) && (validator == null || validator(ThingDefOf.Silver)))
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver, null);
				thing.stackCount = ThingUtility.RoundedResourceStackCount(Mathf.Max(GenMath.RoundRandom(value), 1));
				return thing;
			}
			ThingDef thingDef;
			if (Rand.Chance(0.35f) && (from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.itemGeneratorTags != null && x.itemGeneratorTags.Contains(ItemCollectionGeneratorUtility.SpecialRewardTag) && (validator == null || validator(x)) && Mathf.Abs(1f - x.BaseMarketValue / value) <= 0.35f
			select x).TryRandomElement(out thingDef))
			{
				Thing thing2 = ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef));
				CompQuality compQuality = thing2.TryGetComp<CompQuality>();
				if (compQuality != null)
				{
					compQuality.SetQuality(QualityUtility.RandomBaseGenItemQuality(), ArtGenerationContext.Outsider);
				}
				return thing2;
			}
			if (Rand.Chance(0.13f))
			{
				float minExpensiveMineableResourceMarketValue = ThingDefOf.Uranium.BaseMarketValue;
				ThingDef thingDef2;
				if ((from x in ItemCollectionGenerator_Meteorite.mineables
				where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue >= minExpensiveMineableResourceMarketValue && (validator == null || validator(x.building.mineableThing))
				select x).TryRandomElement(out thingDef2))
				{
					float num = value * 0.9f;
					ThingDef mineableThing = thingDef2.building.mineableThing;
					Thing thing3 = ThingMaker.MakeThing(mineableThing, null);
					thing3.stackCount = Mathf.Max(GenMath.RoundRandom(num / mineableThing.BaseMarketValue), 1);
					return thing3;
				}
			}
			ItemCollectionGenerator_Rewards.Option option2 = (from option in ItemCollectionGeneratorUtility.allGeneratableItems.Select(delegate(ThingDef td)
			{
				if (td.techLevel > techLevel)
				{
					return null;
				}
				if (td.itemGeneratorTags != null && td.itemGeneratorTags.Contains(ItemCollectionGeneratorUtility.SpecialRewardTag))
				{
					return null;
				}
				if (!td.IsWithinCategory(ThingCategoryDefOf.Apparel) && !td.IsWithinCategory(ThingCategoryDefOf.Weapons) && !td.IsWithinCategory(ThingCategoryDefOf.Art) && (td.building == null || !td.Minifiable) && (td.tradeTags == null || !td.tradeTags.Contains("Exotic")))
				{
					return null;
				}
				if (validator != null && !validator(td))
				{
					return null;
				}
				ThingDef stuff = null;
				if (td.MadeFromStuff && !GenStuff.TryRandomStuffByCommonalityFor(td, out stuff, techLevel))
				{
					return null;
				}
				return new ItemCollectionGenerator_Rewards.Option
				{
					thingDef = td,
					quality = ((!td.HasComp(typeof(CompQuality))) ? QualityCategory.Normal : QualityUtility.RandomQuality()),
					stuff = stuff
				};
			})
			where option != null
			select option).MinBy(delegate(ItemCollectionGenerator_Rewards.Option option)
			{
				float value2 = StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(option.thingDef, option.stuff, option.quality), true);
				return Mathf.Abs(value - value2);
			});
			Thing thing4 = ThingMaker.MakeThing(option2.thingDef, option2.stuff);
			CompQuality compQuality2 = thing4.TryGetComp<CompQuality>();
			if (compQuality2 != null)
			{
				compQuality2.SetQuality(option2.quality, ArtGenerationContext.Outsider);
			}
			return thing4;
		}
	}
}
