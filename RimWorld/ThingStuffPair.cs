using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public struct ThingStuffPair
	{
		public ThingDef thing;

		public ThingDef stuff;

		public float commonalityMultiplier;

		private float cachedPrice;

		private float cachedInsulationCold;

		public float Price
		{
			get
			{
				return this.cachedPrice;
			}
		}

		public float InsulationCold
		{
			get
			{
				return this.cachedInsulationCold;
			}
		}

		public float Commonality
		{
			get
			{
				float num = this.commonalityMultiplier;
				num *= this.thing.generateCommonality;
				if (this.thing.IsApparel)
				{
					num *= this.thing.apparel.commonality;
					if (this.stuff != null)
					{
						num *= this.stuff.stuffProps.commonality;
					}
				}
				if (PawnWeaponGenerator.IsDerpWeapon(this.thing, this.stuff))
				{
					num *= 0.01f;
				}
				return num;
			}
		}

		public ThingStuffPair(ThingDef thing, ThingDef stuff, float commonalityMultiplier)
		{
			this.thing = thing;
			this.stuff = stuff;
			this.commonalityMultiplier = commonalityMultiplier;
			this.cachedPrice = thing.GetStatValueAbstract(StatDefOf.MarketValue, stuff);
			this.cachedInsulationCold = thing.GetStatValueAbstract(StatDefOf.Insulation_Cold, null);
		}

		public static List<ThingStuffPair> AllWith(Predicate<ThingDef> thingValidator)
		{
			List<ThingStuffPair> list = new List<ThingStuffPair>();
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ThingDef thingDef = allDefsListForReading[i];
				if (thingValidator(thingDef))
				{
					if (!thingDef.MadeFromStuff)
					{
						list.Add(new ThingStuffPair(thingDef, null, 1f));
					}
					else
					{
						IEnumerable<ThingDef> enumerable = from st in DefDatabase<ThingDef>.AllDefs
						where st.IsStuff && st.stuffProps.CanMake(thingDef)
						select st;
						int num = enumerable.Count<ThingDef>();
						float num2 = 1f / (float)num;
						foreach (ThingDef current in enumerable)
						{
							list.Add(new ThingStuffPair(thingDef, current, num2));
						}
					}
				}
			}
			return (from p in list
			orderby p.Price descending
			select p).ToList<ThingStuffPair>();
		}

		public override string ToString()
		{
			string text;
			if (this.stuff == null)
			{
				text = this.thing.label;
			}
			else
			{
				text = this.stuff.LabelAsStuff + " " + this.thing.label;
			}
			return string.Concat(new string[]
			{
				"(",
				text,
				" $",
				this.Price.ToString("F0"),
				" c=",
				this.Commonality.ToString("F4"),
				")"
			});
		}
	}
}
