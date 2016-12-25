using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Alert_NeedWarmClothes : Alert_High
	{
		private const float MedicinePerColonistThreshold = 2f;

		private const int CheckNextMonthsCount = 3;

		private const float CanShowAlertOnlyIfTempBelow = 5f;

		private static List<Thing> jackets = new List<Thing>();

		private static List<Thing> shirts = new List<Thing>();

		private static List<Thing> pants = new List<Thing>();

		private int NeededWarmClothesCount
		{
			get
			{
				return Find.MapPawns.FreeColonistsSpawnedCount;
			}
		}

		private int ColonistsWithWarmClothesCount
		{
			get
			{
				float lowestTemperatureComing = this.LowestTemperatureComing;
				int num = 0;
				foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
				{
					if (current.GetStatValue(StatDefOf.ComfyTemperatureMin, true) <= lowestTemperatureComing)
					{
						num++;
					}
				}
				return num;
			}
		}

		private int FreeWarmClothesSetsCount
		{
			get
			{
				Alert_NeedWarmClothes.jackets.Clear();
				Alert_NeedWarmClothes.shirts.Clear();
				Alert_NeedWarmClothes.pants.Clear();
				List<Thing> list = Find.ListerThings.ThingsInGroup(ThingRequestGroup.Apparel);
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
				float num = ThingDefOf.Human.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) - this.LowestTemperatureComing;
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
		}

		private int MissingWarmClothesCount
		{
			get
			{
				if (this.LowestTemperatureComing >= ThingDefOf.Human.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null))
				{
					return 0;
				}
				return Mathf.Max(this.NeededWarmClothesCount - this.ColonistsWithWarmClothesCount - this.FreeWarmClothesSetsCount, 0);
			}
		}

		private float LowestTemperatureComing
		{
			get
			{
				Month month = GenDate.CurrentMonth;
				float a = this.GetTemperature(month);
				for (int i = 0; i < 3; i++)
				{
					month = month.NextMonth();
					a = Mathf.Min(a, this.GetTemperature(month));
				}
				return Mathf.Min(a, GenTemperature.OutdoorTemp);
			}
		}

		public override string FullExplanation
		{
			get
			{
				int missingWarmClothesCount = this.MissingWarmClothesCount;
				if (missingWarmClothesCount == this.NeededWarmClothesCount)
				{
					return "NeedWarmClothesDesc1All".Translate() + "\n\n" + "NeedWarmClothesDesc2".Translate(new object[]
					{
						this.LowestTemperatureComing.ToStringTemperature("F0")
					});
				}
				return "NeedWarmClothesDesc1".Translate(new object[]
				{
					missingWarmClothesCount
				}) + "\n\n" + "NeedWarmClothesDesc2".Translate(new object[]
				{
					this.LowestTemperatureComing.ToStringTemperature("F0")
				});
			}
		}

		public override AlertReport Report
		{
			get
			{
				if (this.LowestTemperatureComing >= 5f)
				{
					return false;
				}
				if (this.MissingWarmClothesCount <= 0)
				{
					return false;
				}
				float lowestTemperatureComing = this.LowestTemperatureComing;
				foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
				{
					if (current.GetStatValue(StatDefOf.ComfyTemperatureMin, true) > lowestTemperatureComing)
					{
						return current;
					}
				}
				return true;
			}
		}

		public Alert_NeedWarmClothes()
		{
			this.baseLabel = "NeedWarmClothes".Translate();
		}

		private float GetTemperature(Month month)
		{
			return GenTemperature.AverageTemperatureAtWorldCoordsForMonth(Find.Map.WorldCoords, month);
		}
	}
}
