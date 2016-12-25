using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Armor : StockGenerator
	{
		public const float MinArmor = 0.15f;

		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings()
		{
			int count = this.countRange.RandomInRange;
			for (int i = 0; i < count; i++)
			{
				ThingDef thingDef;
				if (!(from td in DefDatabase<ThingDef>.AllDefs
				where this.<>f__this.IsValidThing(td)
				select td).TryRandomElement(out thingDef))
				{
					break;
				}
				Thing thing = StockGeneratorUtility.TryMakeForStockSingle(thingDef, 1);
				yield return thing;
			}
		}

		private bool IsValidThing(ThingDef td)
		{
			return td == ThingDefOf.Apparel_PersonalShield || (td.tradeability == Tradeability.Stockable && td.techLevel <= this.maxTechLevelGenerate && td.IsApparel && (td.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, null) > 0.15f || td.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, null) > 0.15f));
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return this.IsValidThing(thingDef);
		}
	}
}
