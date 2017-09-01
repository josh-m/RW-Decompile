using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_ItemCollection : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			ItemCollectionGeneratorDef itemCollectionGeneratorDef = rp.itemCollectionGeneratorDef ?? ItemCollectionGeneratorDefOf.RandomGeneralGoods;
			ItemCollectionGeneratorParams? itemCollectionGeneratorParams = rp.itemCollectionGeneratorParams;
			ItemCollectionGeneratorParams parms;
			if (itemCollectionGeneratorParams.HasValue)
			{
				parms = rp.itemCollectionGeneratorParams.Value;
			}
			else
			{
				parms = default(ItemCollectionGeneratorParams);
				parms.count = rp.rect.Cells.Count((IntVec3 x) => x.Standable(map) && x.GetFirstItem(map) == null);
				parms.techLevel = TechLevel.Spacer;
			}
			List<Thing> list = itemCollectionGeneratorDef.Worker.Generate(parms);
			for (int i = 0; i < list.Count; i++)
			{
				ResolveParams resolveParams = rp;
				resolveParams.singleThingToSpawn = list[i];
				BaseGen.symbolStack.Push("thing", resolveParams);
			}
		}
	}
}
