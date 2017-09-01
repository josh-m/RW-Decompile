using System;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Art : StockGenerator_MiscItems
	{
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
				new CurvePoint(1000f, 0.2f),
				true
			}
		};

		public override bool HandlesThingDef(ThingDef td)
		{
			return base.HandlesThingDef(td) && td.Minifiable && td.category == ThingCategory.Building && td.thingClass == typeof(Building_Art);
		}

		protected override float SelectionWeight(ThingDef thingDef)
		{
			return StockGenerator_Art.SelectionWeightMarketValueCurve.Evaluate(thingDef.BaseMarketValue);
		}
	}
}
