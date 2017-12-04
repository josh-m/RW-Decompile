using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_TraderStock : ItemCollectionGenerator
	{
		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			TraderKindDef traderKindDef = parms.traderDef ?? DefDatabase<TraderKindDef>.AllDefsListForReading.RandomElement<TraderKindDef>();
			Faction traderFaction = parms.traderFaction;
			int? tile = parms.tile;
			int forTile;
			if (tile.HasValue)
			{
				forTile = parms.tile.Value;
			}
			else if (Find.AnyPlayerHomeMap != null)
			{
				forTile = Find.AnyPlayerHomeMap.Tile;
			}
			else if (Find.VisibleMap != null)
			{
				forTile = Find.VisibleMap.Tile;
			}
			else
			{
				forTile = -1;
			}
			for (int i = 0; i < traderKindDef.stockGenerators.Count; i++)
			{
				StockGenerator stockGenerator = traderKindDef.stockGenerators[i];
				foreach (Thing current in stockGenerator.GenerateThings(forTile))
				{
					if (current.def.tradeability != Tradeability.Stockable)
					{
						Log.Error(string.Concat(new object[]
						{
							traderKindDef,
							" generated carrying ",
							current,
							" which has is not Stockable. Ignoring..."
						}));
					}
					else
					{
						current.PostGeneratedForTrader(traderKindDef, forTile, traderFaction);
						outThings.Add(current);
					}
				}
			}
		}

		public float AverageTotalStockValue(TraderKindDef td)
		{
			ItemCollectionGeneratorParams parms = default(ItemCollectionGeneratorParams);
			parms.traderDef = td;
			parms.tile = new int?(-1);
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
			parms.tile = new int?(-1);
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
