using System;
using Verse;

namespace RimWorld
{
	public class StockGenerator_WeaponsRanged : StockGenerator_MiscItems
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
			return base.HandlesThingDef(td) && td.IsRangedWeapon;
		}

		protected override float SelectionWeight(ThingDef thingDef)
		{
			return StockGenerator_WeaponsRanged.SelectionWeightMarketValueCurve.Evaluate(thingDef.BaseMarketValue);
		}
	}
}
