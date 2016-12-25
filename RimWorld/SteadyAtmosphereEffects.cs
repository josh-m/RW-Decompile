using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	public static class SteadyAtmosphereEffects
	{
		private const float MapFractionCheckPerTick = 0.0006f;

		private const float RainFireCheckInterval = 97f;

		private const float RainFireChanceOverall = 0.02f;

		private const float RainFireChancePerBuilding = 0.2f;

		private const float SnowFallRateFactor = 0.046f;

		private const float SnowMeltRateFactor = 0.0058f;

		private const float AutoIgnitionChanceFactor = 0.7f;

		private const float FireGlowRate = 0.33f;

		private static ModuleBase snowNoise = null;

		private static int cycleIndex = 0;

		private static float outdoorMeltAmount;

		private static float snowRate;

		private static float rainRate;

		private static float deteriorationRate;

		private static readonly FloatRange AutoIgnitionTemperatureRange = new FloatRange(240f, 1000f);

		public static void SteadyAtmosphereEffectsTick()
		{
			if ((float)Find.TickManager.TicksGame % 97f == 0f && Rand.Value < 0.02f)
			{
				SteadyAtmosphereEffects.RollForRainFire();
			}
			SteadyAtmosphereEffects.outdoorMeltAmount = SteadyAtmosphereEffects.MeltAmountAt(GenTemperature.OutdoorTemp);
			SteadyAtmosphereEffects.snowRate = Find.WeatherManager.SnowRate;
			SteadyAtmosphereEffects.rainRate = Find.WeatherManager.RainRate;
			SteadyAtmosphereEffects.deteriorationRate = Mathf.Lerp(1f, 5f, SteadyAtmosphereEffects.rainRate);
			int num = Mathf.RoundToInt((float)Find.Map.Area * 0.0006f);
			for (int i = 0; i < num; i++)
			{
				if (SteadyAtmosphereEffects.cycleIndex >= Find.Map.Area)
				{
					SteadyAtmosphereEffects.cycleIndex = 0;
				}
				IntVec3 c = MapCellsInRandomOrder.Get(SteadyAtmosphereEffects.cycleIndex);
				SteadyAtmosphereEffects.DoCellSteadyEffects(c);
				SteadyAtmosphereEffects.cycleIndex++;
			}
		}

		private static void DoCellSteadyEffects(IntVec3 c)
		{
			Room room = c.GetRoom();
			bool flag = Find.RoofGrid.Roofed(c);
			if (room == null || room.UsesOutdoorTemperature)
			{
				if (SteadyAtmosphereEffects.outdoorMeltAmount > 0f)
				{
					Find.SnowGrid.AddDepth(c, -SteadyAtmosphereEffects.outdoorMeltAmount);
				}
				if (!flag && SteadyAtmosphereEffects.snowRate > 0.001f)
				{
					SteadyAtmosphereEffects.AddFallenSnowAt(c, 0.046f * Find.WeatherManager.SnowRate);
				}
			}
			if (room != null && room.UsesOutdoorTemperature && !flag)
			{
				List<Thing> thingList = c.GetThingList();
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (!(thing is Plant))
					{
						Filth filth = thing as Filth;
						if (filth != null)
						{
							if (thing.def.filth.rainWashes && Rand.Value < SteadyAtmosphereEffects.rainRate)
							{
								((Filth)thing).ThinFilth();
							}
						}
						else
						{
							Corpse corpse = thing as Corpse;
							if (corpse != null && corpse.innerPawn.apparel != null)
							{
								List<Apparel> wornApparel = corpse.innerPawn.apparel.WornApparel;
								for (int j = 0; j < wornApparel.Count; j++)
								{
									SteadyAtmosphereEffects.TryDoDeteriorate(wornApparel[j], c, false);
								}
							}
							SteadyAtmosphereEffects.TryDoDeteriorate(thing, c, true);
						}
					}
				}
			}
			if (room != null && !room.UsesOutdoorTemperature)
			{
				float temperature = room.Temperature;
				if (temperature > 0f)
				{
					float num = SteadyAtmosphereEffects.MeltAmountAt(temperature);
					if (num > 0f)
					{
						Find.SnowGrid.AddDepth(c, -num);
					}
					if (temperature > SteadyAtmosphereEffects.AutoIgnitionTemperatureRange.min)
					{
						float value = Rand.Value;
						if (value < SteadyAtmosphereEffects.AutoIgnitionTemperatureRange.InverseLerpThroughRange(temperature) * 0.7f)
						{
							FireUtility.TryStartFireIn(c, 0.1f);
						}
						if (value < 0.33f)
						{
							MoteMaker.ThrowHeatGlow(c, 2.3f);
						}
					}
				}
			}
			List<MapCondition> activeConditions = Find.MapConditionManager.ActiveConditions;
			for (int k = 0; k < activeConditions.Count; k++)
			{
				activeConditions[k].DoCellSteadyEffects(c);
			}
		}

		private static float MeltAmountAt(float temperature)
		{
			if (temperature < 0f)
			{
				return 0f;
			}
			if (temperature < 10f)
			{
				return temperature * temperature * 0.0058f * 0.1f;
			}
			return temperature * 0.0058f;
		}

		public static void AddFallenSnowAt(IntVec3 c, float baseAmount)
		{
			if (SteadyAtmosphereEffects.snowNoise == null)
			{
				SteadyAtmosphereEffects.snowNoise = new Perlin(0.039999999105930328, 2.0, 0.5, 5, Rand.Range(0, 651431), QualityMode.Medium);
			}
			float num = SteadyAtmosphereEffects.snowNoise.GetValue(c);
			num += 1f;
			num *= 0.5f;
			if (num < 0.5f)
			{
				num = 0.5f;
			}
			float depthToAdd = baseAmount * num;
			Find.SnowGrid.AddDepth(c, depthToAdd);
		}

		private static void TryDoDeteriorate(Thing t, IntVec3 c, bool checkEdifice)
		{
			if (!t.def.useHitPoints || t.def.category != ThingCategory.Item)
			{
				return;
			}
			float statValue = t.GetStatValue(StatDefOf.DeteriorationRate, true);
			if (statValue < 0.001f)
			{
				return;
			}
			float num = SteadyAtmosphereEffects.deteriorationRate * statValue / 36f;
			if (Rand.Value < num)
			{
				Building building = (!checkEdifice) ? null : c.GetEdifice();
				if (building == null || building.def.building == null || !building.def.building.preventDeterioration)
				{
					t.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1, null, null, null));
				}
			}
		}

		private static void RollForRainFire()
		{
			float num = 0.2f * (float)Find.ListerBuildings.allBuildingsColonistElecFire.Count * Find.WeatherManager.RainRate;
			if (Rand.Value > num)
			{
				return;
			}
			Building building = Find.ListerBuildings.allBuildingsColonistElecFire.RandomElement<Building>();
			if (!Find.RoofGrid.Roofed(building.Position))
			{
				ThingWithComps thingWithComps = building;
				CompPowerTrader comp = thingWithComps.GetComp<CompPowerTrader>();
				if ((comp != null && comp.PowerOn && comp.Props.shortCircuitInRain) || (thingWithComps.GetComp<CompPowerBattery>() != null && thingWithComps.GetComp<CompPowerBattery>().StoredEnergy > 100f))
				{
					GenExplosion.DoExplosion(building.OccupiedRect().RandomCell, 1.9f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
					Find.LetterStack.ReceiveLetter("LetterLabelShortCircuit".Translate(), "ShortCircuitRain".Translate(new object[]
					{
						building.Label
					}), LetterType.BadUrgent, building.Position, null);
				}
			}
		}
	}
}
