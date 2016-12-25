using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Alert_NeedWarmClothes : Alert
	{
		private const float MedicinePerColonistThreshold = 2f;

		private const int CheckNextMonthsCount = 3;

		private const float CanShowAlertOnlyIfTempBelow = 5f;

		private static List<Thing> jackets = new List<Thing>();

		private static List<Thing> shirts = new List<Thing>();

		private static List<Thing> pants = new List<Thing>();

		public Alert_NeedWarmClothes()
		{
			this.defaultLabel = "NeedWarmClothes".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		private int NeededWarmClothesCount(Map map)
		{
			return map.mapPawns.FreeColonistsSpawnedCount;
		}

		private int ColonistsWithWarmClothesCount(Map map)
		{
			float num = this.LowestTemperatureComing(map);
			int num2 = 0;
			foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
			{
				if (current.GetStatValue(StatDefOf.ComfyTemperatureMin, true) <= num)
				{
					num2++;
				}
			}
			return num2;
		}

		private int FreeWarmClothesSetsCount(Map map)
		{
			Alert_NeedWarmClothes.jackets.Clear();
			Alert_NeedWarmClothes.shirts.Clear();
			Alert_NeedWarmClothes.pants.Clear();
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsInAnyStorage())
				{
					if (list[i].GetStatValue(StatDefOf.Insulation_Cold, true) < 0f)
					{
						if (list[i].def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
						{
							if (list[i].def.apparel.layers.Contains(ApparelLayer.OnSkin))
							{
								Alert_NeedWarmClothes.shirts.Add(list[i]);
							}
							else
							{
								Alert_NeedWarmClothes.jackets.Add(list[i]);
							}
						}
						if (list[i].def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
						{
							Alert_NeedWarmClothes.pants.Add(list[i]);
						}
					}
				}
			}
			Alert_NeedWarmClothes.jackets.SortByDescending((Thing x) => x.GetStatValue(StatDefOf.Insulation_Cold, true));
			Alert_NeedWarmClothes.shirts.SortByDescending((Thing x) => x.GetStatValue(StatDefOf.Insulation_Cold, true));
			Alert_NeedWarmClothes.pants.SortByDescending((Thing x) => x.GetStatValue(StatDefOf.Insulation_Cold, true));
			float num = ThingDefOf.Human.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) - this.LowestTemperatureComing(map);
			if (num <= 0f)
			{
				return GenMath.Max(Alert_NeedWarmClothes.jackets.Count, Alert_NeedWarmClothes.shirts.Count, Alert_NeedWarmClothes.pants.Count);
			}
			int num2 = 0;
			while (Alert_NeedWarmClothes.jackets.Any<Thing>() || Alert_NeedWarmClothes.shirts.Any<Thing>() || Alert_NeedWarmClothes.pants.Any<Thing>())
			{
				float num3 = 0f;
				if (Alert_NeedWarmClothes.jackets.Any<Thing>())
				{
					Thing thing = Alert_NeedWarmClothes.jackets[Alert_NeedWarmClothes.jackets.Count - 1];
					Alert_NeedWarmClothes.jackets.RemoveLast<Thing>();
					float num4 = -thing.GetStatValue(StatDefOf.Insulation_Cold, true);
					num3 += num4;
				}
				if (num3 < num && Alert_NeedWarmClothes.shirts.Any<Thing>())
				{
					Thing thing2 = Alert_NeedWarmClothes.shirts[Alert_NeedWarmClothes.shirts.Count - 1];
					Alert_NeedWarmClothes.shirts.RemoveLast<Thing>();
					float num5 = -thing2.GetStatValue(StatDefOf.Insulation_Cold, true);
					num3 += num5;
				}
				if (num3 < num && Alert_NeedWarmClothes.pants.Any<Thing>())
				{
					for (int j = 0; j < Alert_NeedWarmClothes.pants.Count; j++)
					{
						float num6 = -Alert_NeedWarmClothes.pants[j].GetStatValue(StatDefOf.Insulation_Cold, true);
						if (num6 + num3 >= num)
						{
							num3 += num6;
							Alert_NeedWarmClothes.pants.RemoveAt(j);
							break;
						}
					}
				}
				if (num3 < num)
				{
					break;
				}
				num2++;
			}
			return num2;
		}

		private int MissingWarmClothesCount(Map map)
		{
			if (this.LowestTemperatureComing(map) >= ThingDefOf.Human.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null))
			{
				return 0;
			}
			return Mathf.Max(this.NeededWarmClothesCount(map) - this.ColonistsWithWarmClothesCount(map) - this.FreeWarmClothesSetsCount(map), 0);
		}

		private float LowestTemperatureComing(Map map)
		{
			Month month = GenLocalDate.Month(map);
			float a = this.GetTemperature(month, map);
			for (int i = 0; i < 3; i++)
			{
				month = month.NextMonth();
				a = Mathf.Min(a, this.GetTemperature(month, map));
			}
			return Mathf.Min(a, map.mapTemperature.OutdoorTemp);
		}

		public override string GetExplanation()
		{
			Map map = this.MapWithMissingWarmClothes();
			if (map == null)
			{
				return string.Empty;
			}
			int num = this.MissingWarmClothesCount(map);
			if (num == this.NeededWarmClothesCount(map))
			{
				return "NeedWarmClothesDesc1All".Translate() + "\n\n" + "NeedWarmClothesDesc2".Translate(new object[]
				{
					this.LowestTemperatureComing(map).ToStringTemperature("F0")
				});
			}
			return "NeedWarmClothesDesc1".Translate(new object[]
			{
				num
			}) + "\n\n" + "NeedWarmClothesDesc2".Translate(new object[]
			{
				this.LowestTemperatureComing(map).ToStringTemperature("F0")
			});
		}

		public override AlertReport GetReport()
		{
			Map map = this.MapWithMissingWarmClothes();
			if (map == null)
			{
				return false;
			}
			float num = this.LowestTemperatureComing(map);
			foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
			{
				if (current.GetStatValue(StatDefOf.ComfyTemperatureMin, true) > num)
				{
					return current;
				}
			}
			return true;
		}

		private Map MapWithMissingWarmClothes()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome)
				{
					if (this.LowestTemperatureComing(map) < 5f)
					{
						if (this.MissingWarmClothesCount(map) > 0)
						{
							return map;
						}
					}
				}
			}
			return null;
		}

		private float GetTemperature(Month month, Map map)
		{
			return GenTemperature.AverageTemperatureAtTileForMonth(map.Tile, month);
		}
	}
}
