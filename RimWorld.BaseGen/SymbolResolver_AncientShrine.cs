using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_AncientShrine : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			IntVec3 bottomLeft = rp.rect.BottomLeft;
			Map map = BaseGen.globalSettings.map;
			CellRect rect = new CellRect(bottomLeft.x + rp.rect.Width / 2 - 1, bottomLeft.z + rp.rect.Height / 2, 2, 1);
			foreach (IntVec3 current in rect)
			{
				List<Thing> thingList = current.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (!thingList[i].def.destroyable)
					{
						return;
					}
				}
			}
			ResolveParams resolveParams = rp;
			resolveParams.rect = CellRect.SingleCell(rect.BottomLeft);
			resolveParams.thingRot = new Rot4?(Rot4.East);
			BaseGen.symbolStack.Push("ancientCryptosleepCasket", resolveParams);
			ResolveParams resolveParams2 = rp;
			resolveParams2.rect = rect;
			resolveParams2.floorDef = TerrainDefOf.Concrete;
			BaseGen.symbolStack.Push("floor", resolveParams2);
			ResolveParams resolveParams3 = rp;
			resolveParams3.floorDef = (rp.floorDef ?? TerrainDefOf.MetalTile);
			BaseGen.symbolStack.Push("floor", resolveParams3);
		}
	}
}
