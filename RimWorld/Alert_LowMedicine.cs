using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_LowMedicine : Alert
	{
		private const float MedicinePerColonistThreshold = 2f;

		public Alert_LowMedicine()
		{
			this.defaultLabel = "LowMedicine".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		public override string GetExplanation()
		{
			Map map = this.MapWithLowMedicine();
			if (map == null)
			{
				return string.Empty;
			}
			int num = this.MedicineCount(map);
			if (num == 0)
			{
				return string.Format("NoMedicineDesc".Translate(), new object[0]);
			}
			return string.Format("LowMedicineDesc".Translate(), num);
		}

		public override AlertReport GetReport()
		{
			if (Find.TickManager.TicksGame < 150000)
			{
				return false;
			}
			return this.MapWithLowMedicine() != null;
		}

		private Map MapWithLowMedicine()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome)
				{
					if ((float)this.MedicineCount(map) < 2f * (float)map.mapPawns.FreeColonistsSpawnedCount)
					{
						return map;
					}
				}
			}
			return null;
		}

		private int MedicineCount(Map map)
		{
			return map.resourceCounter.GetCountIn(ThingRequestGroup.Medicine);
		}
	}
}
