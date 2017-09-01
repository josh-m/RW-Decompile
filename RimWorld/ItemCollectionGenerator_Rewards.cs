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

		protected override ItemCollectionGeneratorParams RandomTestParams
		{
			get
			{
				ItemCollectionGeneratorParams randomTestParams = base.RandomTestParams;
				randomTestParams.count = Rand.RangeInclusive(5, 8);
				randomTestParams.totalMarketValue = Rand.Range(10000f, 20000f);
				randomTestParams.techLevel = TechLevel.Transcendent;
				return randomTestParams;
			}
		}

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			int count = parms.count;
			float totalMarketValue = parms.totalMarketValue;
			TechLevel techLevel = parms.techLevel;
			Predicate<ThingDef> validator = parms.validator;
			for (int i = 0; i < count; i++)
			{
				outThings.Add(this.GenerateReward(totalMarketValue / (float)count, techLevel, validator));
			}
		}

		private Thing GenerateReward(float value, TechLevel techLevel, Predicate<ThingDef> validator = null)
		{
			if (Rand.Value < 0.5f)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver, null);
				thing.stackCount = Mathf.Max(GenMath.RoundRandom(value), 1);
				return thing;
			}
			ItemCollectionGenerator_Rewards.Option option2 = (from option in ItemCollectionGeneratorUtility.allGeneratableItems.Select(delegate(ThingDef td)
			{
				if (td.techLevel > techLevel)
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
				if (td.MadeFromStuff)
				{
					if (!(from x in GenStuff.AllowedStuffsFor(td)
					where x.techLevel <= techLevel
					select x).TryRandomElementByWeight((ThingDef st) => st.stuffProps.commonality, out stuff))
					{
						return null;
					}
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
			Thing thing2 = ThingMaker.MakeThing(option2.thingDef, option2.stuff);
			if (option2.thingDef.HasComp(typeof(CompQuality)))
			{
				thing2.TryGetComp<CompQuality>().SetQuality(option2.quality, ArtGenerationContext.Outsider);
			}
			return thing2;
		}
	}
}
