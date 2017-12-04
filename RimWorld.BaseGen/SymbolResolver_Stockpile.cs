using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Stockpile : SymbolResolver
	{
		private List<IntVec3> cells = new List<IntVec3>();

		private const float FreeCellsFraction = 0.41f;

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			if (rp.stockpileConcreteContents != null)
			{
				this.CalculateFreeCells(rp.rect, 0f);
				int num = 0;
				for (int i = rp.stockpileConcreteContents.Count - 1; i >= 0; i--)
				{
					if (num >= this.cells.Count)
					{
						break;
					}
					GenSpawn.Spawn(rp.stockpileConcreteContents[i], this.cells[num], map);
					num++;
				}
				for (int j = rp.stockpileConcreteContents.Count - 1; j >= 0; j--)
				{
					if (!rp.stockpileConcreteContents[j].Spawned)
					{
						rp.stockpileConcreteContents[j].Destroy(DestroyMode.Vanish);
					}
				}
				rp.stockpileConcreteContents.Clear();
			}
			else
			{
				ItemCollectionGeneratorDef itemCollectionGeneratorDef = rp.itemCollectionGeneratorDef ?? Rand.Element<ItemCollectionGeneratorDef>(ItemCollectionGeneratorDefOf.RandomGeneralGoods, ItemCollectionGeneratorDefOf.Weapons, ItemCollectionGeneratorDefOf.Apparel, ItemCollectionGeneratorDefOf.RawResources);
				ItemCollectionGeneratorParams? itemCollectionGeneratorParams = rp.itemCollectionGeneratorParams;
				ItemCollectionGeneratorParams value;
				if (itemCollectionGeneratorParams.HasValue)
				{
					value = rp.itemCollectionGeneratorParams.Value;
				}
				else
				{
					value = default(ItemCollectionGeneratorParams);
					value.techLevel = new TechLevel?((rp.faction == null) ? TechLevel.Spacer : rp.faction.def.techLevel);
					if (itemCollectionGeneratorDef.Worker is ItemCollectionGenerator_Standard)
					{
						float? stockpileMarketValue = rp.stockpileMarketValue;
						float value2 = (!stockpileMarketValue.HasValue) ? Mathf.Min((float)this.cells.Count * 120f, 1800f) : stockpileMarketValue.Value;
						value.totalMarketValue = new float?(value2);
					}
				}
				int? count = value.count;
				if (!count.HasValue)
				{
					this.CalculateFreeCells(rp.rect, 0.41f);
					value.count = new int?(this.cells.Count);
				}
				ResolveParams resolveParams = rp;
				resolveParams.itemCollectionGeneratorDef = itemCollectionGeneratorDef;
				resolveParams.itemCollectionGeneratorParams = new ItemCollectionGeneratorParams?(value);
				BaseGen.symbolStack.Push("itemCollection", resolveParams);
			}
		}

		private void CalculateFreeCells(CellRect rect, float freeCellsFraction)
		{
			Map map = BaseGen.globalSettings.map;
			this.cells.Clear();
			foreach (IntVec3 current in rect)
			{
				if (current.Standable(map) && current.GetFirstItem(map) == null)
				{
					this.cells.Add(current);
				}
			}
			int num = (int)(freeCellsFraction * (float)this.cells.Count);
			for (int i = 0; i < num; i++)
			{
				this.cells.RemoveAt(Rand.Range(0, this.cells.Count));
			}
			this.cells.Shuffle<IntVec3>();
		}
	}
}
