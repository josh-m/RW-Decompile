using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class GenPlant
	{
		public static bool GrowthSeasonNow(IntVec3 c, Map map)
		{
			Room roomOrAdjacent = c.GetRoomOrAdjacent(map, RegionType.Set_All);
			if (roomOrAdjacent == null)
			{
				return false;
			}
			if (roomOrAdjacent.UsesOutdoorTemperature)
			{
				return map.weatherManager.growthSeasonMemory.GrowthSeasonOutdoorsNow;
			}
			float temperature = c.GetTemperature(map);
			return temperature > 0f && temperature < 58f;
		}

		public static bool SnowAllowsPlanting(IntVec3 c, Map map)
		{
			return c.GetSnowDepth(map) < 0.2f;
		}

		public static bool CanEverPlantAt(this ThingDef plantDef, IntVec3 c, Map map)
		{
			if (plantDef.category != ThingCategory.Plant)
			{
				Log.Error("Checking CanGrowAt with " + plantDef + " which is not a plant.");
			}
			if (!c.InBounds(map))
			{
				return false;
			}
			if (map.fertilityGrid.FertilityAt(c) < plantDef.plant.fertilityMin)
			{
				return false;
			}
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing.def.BlockPlanting)
				{
					return false;
				}
				if (plantDef.passability == Traversability.Impassable && (thing.def.category == ThingCategory.Pawn || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Building || thing.def.category == ThingCategory.Plant))
				{
					return false;
				}
			}
			if (plantDef.passability == Traversability.Impassable)
			{
				for (int j = 0; j < 4; j++)
				{
					IntVec3 c2 = c + GenAdj.CardinalDirections[j];
					if (c2.InBounds(map))
					{
						Building edifice = c2.GetEdifice(map);
						if (edifice != null && edifice.def.IsDoor)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		public static void LogPlantProportions()
		{
			Dictionary<ThingDef, float> dictionary = new Dictionary<ThingDef, float>();
			foreach (ThingDef current in Find.VisibleMap.Biome.AllWildPlants)
			{
				dictionary.Add(current, 0f);
			}
			float num = 0f;
			foreach (IntVec3 current2 in Find.VisibleMap.AllCells)
			{
				Plant plant = current2.GetPlant(Find.VisibleMap);
				if (plant != null && dictionary.ContainsKey(plant.def))
				{
					Dictionary<ThingDef, float> dictionary2;
					Dictionary<ThingDef, float> expr_9B = dictionary2 = dictionary;
					ThingDef key;
					ThingDef expr_A5 = key = plant.def;
					float num2 = dictionary2[key];
					expr_9B[expr_A5] = num2 + 1f;
					num += 1f;
				}
			}
			foreach (ThingDef current3 in Find.VisibleMap.Biome.AllWildPlants)
			{
				Dictionary<ThingDef, float> dictionary3;
				Dictionary<ThingDef, float> expr_10B = dictionary3 = dictionary;
				ThingDef key;
				ThingDef expr_110 = key = current3;
				float num2 = dictionary3[key];
				expr_10B[expr_110] = num2 / num;
			}
			Dictionary<ThingDef, float> dictionary4 = GenPlant.CalculateDesiredPlantProportions(Find.VisibleMap.Biome);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("PLANT           EXPECTED             FOUND");
			foreach (ThingDef current4 in Find.VisibleMap.Biome.AllWildPlants)
			{
				stringBuilder.AppendLine(string.Concat(new string[]
				{
					current4.LabelCap,
					"       ",
					dictionary4[current4].ToStringPercent(),
					"        ",
					dictionary[current4].ToStringPercent()
				}));
			}
			Log.Message(stringBuilder.ToString());
		}

		public static Dictionary<ThingDef, float> CalculateDesiredPlantProportions(BiomeDef biome)
		{
			Dictionary<ThingDef, float> dictionary = new Dictionary<ThingDef, float>();
			float num = 0f;
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (current.plant != null)
				{
					float num2 = biome.CommonalityOfPlant(current);
					dictionary.Add(current, num2);
					num += num2;
				}
			}
			foreach (ThingDef current2 in biome.AllWildPlants)
			{
				Dictionary<ThingDef, float> dictionary2;
				Dictionary<ThingDef, float> expr_7C = dictionary2 = dictionary;
				ThingDef key;
				ThingDef expr_81 = key = current2;
				float num3 = dictionary2[key];
				expr_7C[expr_81] = num3 / num;
			}
			return dictionary;
		}

		[DebuggerHidden]
		public static IEnumerable<ThingDef> ValidPlantTypesForGrowers(List<IPlantToGrowSettable> sel)
		{
			foreach (ThingDef plantDef in from def in DefDatabase<ThingDef>.AllDefs
			where def.category == ThingCategory.Plant
			select def)
			{
				if (sel.TrueForAll((IPlantToGrowSettable x) => GenPlant.CanSowOnGrower(this.<plantDef>__1, x)))
				{
					yield return plantDef;
				}
			}
		}

		public static bool CanSowOnGrower(ThingDef plantDef, object obj)
		{
			if (obj is Zone)
			{
				return plantDef.plant.sowTags.Contains("Ground");
			}
			Thing thing = obj as Thing;
			return thing != null && thing.def.building != null && plantDef.plant.sowTags.Contains(thing.def.building.sowTag);
		}

		public static Thing AdjacentSowBlocker(ThingDef plantDef, IntVec3 c, Map map)
		{
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				if (c2.InBounds(map))
				{
					Plant plant = c2.GetPlant(map);
					if (plant != null && (plant.def.plant.blockAdjacentSow || (plantDef.plant.blockAdjacentSow && plant.sown)))
					{
						return plant;
					}
				}
			}
			return null;
		}

		internal static void LogPlantData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("All plant data");
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (current.plant != null)
				{
					float num = current.plant.growDays * 2f;
					float num2 = current.plant.lifespanFraction / (current.plant.lifespanFraction - 1f);
					float num3 = num2 * num;
					float num4 = (num3 + num * 0.399999976f) / current.plant.reproduceMtbDays;
					stringBuilder.AppendLine(current.defName);
					stringBuilder.AppendLine("  lifeSpanDays:\t\t\t\t" + current.plant.LifespanDays.ToString("F2"));
					stringBuilder.AppendLine("  daysToGrown:\t\t\t\t" + current.plant.growDays);
					stringBuilder.AppendLine("  guess days to grown:\t\t" + num.ToString("F2"));
					stringBuilder.AppendLine("  grown days before death:\t" + num3.ToString("F2"));
					stringBuilder.AppendLine("  percent of life grown:\t" + num2.ToStringPercent());
					if (current.plant.reproduces)
					{
						stringBuilder.AppendLine("  MTB seed emits (days):\t" + current.plant.reproduceMtbDays.ToString("F2"));
						stringBuilder.AppendLine("  average seeds emitted:\t" + num4.ToString("F2"));
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
