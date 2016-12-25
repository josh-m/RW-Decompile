using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Barracks : SymbolResolver
	{
		private const float SpawnCampfireIfTemperatureBelow = -20f;

		private const float SpawnSecondCampfireIfTemperatureBelow = -45f;

		private const float SpawnPassiveCoolerIfTemperatureAbove = 22f;

		private List<int> tmpTakenCorners = new List<int>();

		public override void Resolve(ResolveParams rp)
		{
			this.tmpTakenCorners.Clear();
			Map map = BaseGen.globalSettings.map;
			BaseGen.symbolStack.Push("fillWithBeds", rp);
			int num;
			if (map.mapTemperature.OutdoorTemp > 22f && BaseGenUtility.TryFindRandomNonDoorBlockingCorner(rp.rect, BaseGen.globalSettings.map, out num, this.tmpTakenCorners))
			{
				this.tmpTakenCorners.Add(num);
				ResolveParams resolveParams = rp;
				resolveParams.singleThingDef = ThingDefOf.PassiveCooler;
				resolveParams.rect = CellRect.SingleCell(BaseGenUtility.GetCornerPos(rp.rect, num));
				BaseGen.symbolStack.Push("thing", resolveParams);
			}
			ThingDef singleThingDef = (map.mapTemperature.OutdoorTemp >= -20f) ? ThingDefOf.TorchLamp : ThingDefOf.Campfire;
			int num2 = (map.mapTemperature.OutdoorTemp >= -45f) ? 1 : 2;
			for (int i = 0; i < num2; i++)
			{
				int num3;
				if (BaseGenUtility.TryFindRandomNonDoorBlockingCorner(rp.rect, BaseGen.globalSettings.map, out num3, this.tmpTakenCorners))
				{
					this.tmpTakenCorners.Add(num3);
					ResolveParams resolveParams2 = rp;
					resolveParams2.singleThingDef = singleThingDef;
					resolveParams2.rect = CellRect.SingleCell(BaseGenUtility.GetCornerPos(rp.rect, num3));
					BaseGen.symbolStack.Push("thing", resolveParams2);
				}
			}
		}
	}
}
