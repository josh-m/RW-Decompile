using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Storage : SymbolResolver
	{
		private const float SpawnPassiveCoolerIfTemperatureAbove = 15f;

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			BaseGen.symbolStack.Push("randomGeneralGoods", rp);
			int corner;
			if (map.mapTemperature.OutdoorTemp > 15f && BaseGenUtility.TryFindRandomNonDoorBlockingCorner(rp.rect, BaseGen.globalSettings.map, out corner, null))
			{
				ResolveParams resolveParams = rp;
				resolveParams.singleThingDef = ThingDefOf.PassiveCooler;
				resolveParams.rect = CellRect.SingleCell(BaseGenUtility.GetCornerPos(rp.rect, corner));
				BaseGen.symbolStack.Push("thing", resolveParams);
			}
		}
	}
}
