using System;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Armor : StockGenerator_MiscItems
	{
		public const float MinArmor = 0.15f;

		private static readonly SimpleCurve SelectionWeightMarketValueCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(500f, 1f),
				true
			},
			{
				new CurvePoint(1500f, 0.2f),
				true
			},
			{
				new CurvePoint(5000f, 0.1f),
				true
			}
		};

		public override bool HandlesThingDef(ThingDef td)
		{
			return td == ThingDefOf.Apparel_ShieldBelt || td == ThingDefOf.Apparel_SmokepopBelt || (base.HandlesThingDef(td) && td.IsApparel && (td.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, null) > 0.15f || td.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, null) > 0.15f));
		}

		protected override float SelectionWeight(ThingDef thingDef)
		{
			return StockGenerator_Armor.SelectionWeightMarketValueCurve.Evaluate(thingDef.BaseMarketValue);
		}
	}
}
