using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_TraderStock : ItemCollectionGenerator
	{
		protected override ItemCollectionGeneratorParams RandomTestParams
		{
			get
			{
				ItemCollectionGeneratorParams randomTestParams = base.RandomTestParams;
				randomTestParams.traderDef = DefDatabase<TraderKindDef>.AllDefsListForReading.RandomElement<TraderKindDef>();
				randomTestParams.forTile = ((Find.VisibleMap == null) ? -1 : Find.VisibleMap.Tile);
				randomTestParams.forFaction = (Find.FactionManager.RandomAlliedFaction(false, false, true) ?? Find.FactionManager.RandomEnemyFaction(false, false, true));
				return randomTestParams;
			}
		}

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			TraderKindDef traderDef = parms.traderDef;
			int forTile = parms.forTile;
			Faction forFaction = parms.forFaction;
			for (int i = 0; i < traderDef.stockGenerators.Count; i++)
			{
				StockGenerator stockGenerator = traderDef.stockGenerators[i];
				foreach (Thing current in stockGenerator.GenerateThings(forTile))
				{
					if (current.def.tradeability != Tradeability.Stockable)
					{
						Log.Error(string.Concat(new object[]
						{
							traderDef,
							" generated carrying ",
							current,
							" which has is not Stockable. Ignoring..."
						}));
					}
					else
					{
						current.PostGeneratedForTrader(traderDef, forTile, forFaction);
						outThings.Add(current);
					}
				}
			}
		}

		public float AverageTotalStockValue(TraderKindDef td)
		{
			ItemCollectionGeneratorParams parms = default(ItemCollectionGeneratorParams);
			parms.traderDef = td;
			parms.forTile = -1;
			float num = 0f;
			for (int i = 0; i < 50; i++)
			{
				foreach (Thing current in base.Generate(parms))
				{
					num += current.MarketValue * (float)current.stackCount;
				}
			}
			return num / 50f;
		}

		public string GenerationDataFor(TraderKindDef td)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(td.defName);
			stringBuilder.AppendLine("Average total market value:" + this.AverageTotalStockValue(td).ToString("F0"));
			ItemCollectionGeneratorParams parms = default(ItemCollectionGeneratorParams);
			parms.traderDef = td;
			parms.forTile = -1;
			stringBuilder.AppendLine("Example generated stock:\n\n");
			foreach (Thing current in base.Generate(parms))
			{
				MinifiedThing minifiedThing = current as MinifiedThing;
				Thing thing;
				if (minifiedThing != null)
				{
					thing = minifiedThing.InnerThing;
				}
				else
				{
					thing = current;
				}
				string text = thing.LabelCap;
				text = text + " [" + (thing.MarketValue * (float)thing.stackCount).ToString("F0") + "]";
				stringBuilder.AppendLine(text);
			}
			return stringBuilder.ToString();
		}
	}
}
