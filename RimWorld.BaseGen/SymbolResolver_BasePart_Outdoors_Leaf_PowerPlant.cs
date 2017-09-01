using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Outdoors_Leaf_PowerPlant : SymbolResolver
	{
		private const float MaxCoverage = 0.09f;

		private static List<ThingDef> availablePowerPlants = new List<ThingDef>();

		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
			if (BaseGen.globalSettings.basePart_buildingsResolved < BaseGen.globalSettings.minBuildings)
			{
				return false;
			}
			if (BaseGen.globalSettings.basePart_emptyNodesResolved < BaseGen.globalSettings.minEmptyNodes)
			{
				return false;
			}
			if (BaseGen.globalSettings.basePart_powerPlantsCoverage + (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area >= 0.09f)
			{
				return false;
			}
			if (rp.faction != null && rp.faction.def.techLevel < TechLevel.Industrial)
			{
				return false;
			}
			if (rp.rect.Width > 13 || rp.rect.Height > 13)
			{
				return false;
			}
			this.CalculateAvailablePowerPlants(rp.rect);
			return SymbolResolver_BasePart_Outdoors_Leaf_PowerPlant.availablePowerPlants.Any<ThingDef>();
		}

		public override void Resolve(ResolveParams rp)
		{
			this.CalculateAvailablePowerPlants(rp.rect);
			if (!SymbolResolver_BasePart_Outdoors_Leaf_PowerPlant.availablePowerPlants.Any<ThingDef>())
			{
				return;
			}
			BaseGen.symbolStack.Push("refuel", rp);
			ThingDef thingDef = SymbolResolver_BasePart_Outdoors_Leaf_PowerPlant.availablePowerPlants.RandomElement<ThingDef>();
			ResolveParams resolveParams = rp;
			resolveParams.singleThingDef = thingDef;
			int? fillWithThingsPadding = rp.fillWithThingsPadding;
			resolveParams.fillWithThingsPadding = new int?((!fillWithThingsPadding.HasValue) ? Mathf.Max(5 - thingDef.size.x, 1) : fillWithThingsPadding.Value);
			BaseGen.symbolStack.Push("fillWithThings", resolveParams);
			BaseGen.globalSettings.basePart_powerPlantsCoverage += (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area;
		}

		private void CalculateAvailablePowerPlants(CellRect rect)
		{
			Map map = BaseGen.globalSettings.map;
			SymbolResolver_BasePart_Outdoors_Leaf_PowerPlant.availablePowerPlants.Clear();
			if (rect.Width >= ThingDefOf.SolarGenerator.size.x && rect.Height >= ThingDefOf.SolarGenerator.size.z)
			{
				int num = 0;
				CellRect.CellRectIterator iterator = rect.GetIterator();
				while (!iterator.Done())
				{
					if (!iterator.Current.Roofed(map))
					{
						num++;
					}
					iterator.MoveNext();
				}
				if ((float)num / (float)rect.Area >= 0.5f)
				{
					SymbolResolver_BasePart_Outdoors_Leaf_PowerPlant.availablePowerPlants.Add(ThingDefOf.SolarGenerator);
				}
			}
			if (rect.Width >= ThingDefOf.FueledGenerator.size.x && rect.Height >= ThingDefOf.FueledGenerator.size.z)
			{
				SymbolResolver_BasePart_Outdoors_Leaf_PowerPlant.availablePowerPlants.Add(ThingDefOf.FueledGenerator);
			}
		}
	}
}
