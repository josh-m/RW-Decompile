using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_ItemStash : GenStep_Scatterer
	{
		public List<ItemCollectionGeneratorDef> itemCollectionGeneratorDefs;

		public FloatRange totalValueRange = new FloatRange(1000f, 2000f);

		private const int Size = 7;

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			if (!c.SupportsStructureType(map, TerrainAffordance.Heavy))
			{
				return false;
			}
			if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
			{
				return false;
			}
			CellRect.CellRectIterator iterator = CellRect.CenteredOn(c, 7, 7).GetIterator();
			while (!iterator.Done())
			{
				if (!iterator.Current.InBounds(map) || iterator.Current.GetEdifice(map) != null)
				{
					return false;
				}
				iterator.MoveNext();
			}
			return true;
		}

		protected override void ScatterAt(IntVec3 loc, Map map, int count = 1)
		{
			CellRect cellRect = CellRect.CenteredOn(loc, 7, 7).ClipInsideMap(map);
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = cellRect;
			resolveParams.faction = map.ParentFaction;
			ItemStashContentsComp component = map.info.parent.GetComponent<ItemStashContentsComp>();
			if (component != null && component.contents.Any)
			{
				resolveParams.stockpileConcreteContents = component.contents;
			}
			else
			{
				resolveParams.stockpileMarketValue = new float?(this.totalValueRange.RandomInRange);
				if (this.itemCollectionGeneratorDefs != null)
				{
					resolveParams.itemCollectionGeneratorDef = this.itemCollectionGeneratorDefs.RandomElement<ItemCollectionGeneratorDef>();
				}
			}
			BaseGen.globalSettings.map = map;
			BaseGen.symbolStack.Push("storage", resolveParams);
			BaseGen.Generate();
			MapGenerator.SetVar<CellRect>("RectOfInterest", cellRect);
		}
	}
}
