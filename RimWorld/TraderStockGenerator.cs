using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class TraderStockGenerator
	{
		[DebuggerHidden]
		public static IEnumerable<Thing> GenerateTraderThings(TraderKindDef traderDef, Map forMap)
		{
			foreach (StockGenerator stock in traderDef.stockGenerators)
			{
				foreach (Thing t in stock.GenerateThings(forMap))
				{
					if (t.def.tradeability != Tradeability.Stockable)
					{
						Log.Error(string.Concat(new object[]
						{
							traderDef,
							" generated carrying ",
							t,
							" which has is not Stockable. Ignoring..."
						}));
					}
					else
					{
						CompQuality cq = t.TryGetComp<CompQuality>();
						if (cq != null)
						{
							cq.SetQuality(QualityUtility.RandomTraderItemQuality(), ArtGenerationContext.Outsider);
						}
						if (t.def.colorGeneratorInTraderStock != null)
						{
							t.SetColor(t.def.colorGeneratorInTraderStock.NewRandomizedColor(), true);
						}
						if (t.def.Minifiable)
						{
							int stackCount = t.stackCount;
							t.stackCount = 1;
							MinifiedThing minified = t.MakeMinified();
							minified.stackCount = stackCount;
							yield return minified;
						}
						yield return t;
					}
				}
			}
		}

		internal static void LogGenerationData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (TraderKindDef current in DefDatabase<TraderKindDef>.AllDefs)
			{
				stringBuilder.AppendLine("Generated stock for " + current.defName + ":");
				foreach (Thing current2 in TraderStockGenerator.GenerateTraderThings(current, null))
				{
					MinifiedThing minifiedThing = current2 as MinifiedThing;
					Thing thing;
					if (minifiedThing != null)
					{
						thing = minifiedThing.InnerThing;
					}
					else
					{
						thing = current2;
					}
					string text = thing.LabelCap;
					QualityCategory qualityCategory;
					if (thing.TryGetQuality(out qualityCategory))
					{
						text = text + " (" + qualityCategory.ToString() + ")";
					}
					stringBuilder.AppendLine(text);
				}
				stringBuilder.AppendLine();
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
