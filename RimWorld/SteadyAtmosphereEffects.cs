using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	public class SteadyAtmosphereEffects
	{
		private const float MapFractionCheckPerTick = 0.0006f;

		private const float RainFireCheckInterval = 97f;

		private const float RainFireChanceOverall = 0.02f;

		private const float RainFireChancePerBuilding = 0.2f;

		private const float SnowFallRateFactor = 0.046f;

		private const float SnowMeltRateFactor = 0.0058f;

		private const float AutoIgnitionChanceFactor = 0.7f;

		private const float FireGlowRate = 0.33f;

		private Map map;

		private ModuleBase snowNoise;

		private int cycleIndex;

		private float outdoorMeltAmount;

		private float snowRate;

		private float rainRate;

		private float deteriorationRate;

		private static readonly FloatRange AutoIgnitionTemperatureRange = new FloatRange(240f, 1000f);

		public SteadyAtmosphereEffects(Map map)
		{
			this.map = map;
		}

		public void SteadyAtmosphereEffectsTick()
		{
			if ((float)Find.TickManager.TicksGame % 97f == 0f && Rand.Value < 0.02f)
			{
				this.RollForRainFire();
			}
			this.outdoorMeltAmount = this.MeltAmountAt(this.map.mapTemperature.OutdoorTemp);
			this.snowRate = this.map.weatherManager.SnowRate;
			this.rainRate = this.map.weatherManager.RainRate;
			this.deteriorationRate = Mathf.Lerp(1f, 5f, this.rainRate);
			int num = Mathf.RoundToInt((float)this.map.Area * 0.0006f);
			for (int i = 0; i < num; i++)
			{
				if (this.cycleIndex >= this.map.Area)
				{
					this.cycleIndex = 0;
				}
				IntVec3 c = this.map.cellsInRandomOrder.Get(this.cycleIndex);
				this.DoCellSteadyEffects(c);
				this.cycleIndex++;
			}
		}

		private void DoCellSteadyEffects(IntVec3 c)
		{
			Room room = c.GetRoom(this.map);
			bool flag = this.map.roofGrid.Roofed(c);
			if (room == null || room.UsesOutdoorTemperature)
			{
				if (this.outdoorMeltAmount > 0f)
				{
					this.map.snowGrid.AddDepth(c, -this.outdoorMeltAmount);
				}
				if (!flag && this.snowRate > 0.001f)
				{
					this.AddFallenSnowAt(c, 0.046f * this.map.weatherManager.SnowRate);
				}
			}
			if (room != null && room.UsesOutdoorTemperature && !flag)
			{
				List<Thing> thingList = c.GetThingList(this.map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (!(thing is Plant))
					{
						Filth filth = thing as Filth;
						if (filth != null)
						{
							if (thing.def.filth.rainWashes && Rand.Value < this.rainRate)
							{
								((Filth)thing).ThinFilth();
							}
						}
						else
						{
							Corpse corpse = thing as Corpse;
							if (corpse != null && corpse.InnerPawn.apparel != null)
							{
								List<Apparel> wornApparel = corpse.InnerPawn.apparel.WornApparel;
								for (int j = 0; j < wornApparel.Count; j++)
								{
									this.TryDoDeteriorate(wornApparel[j], c, false);
								}
							}
							this.TryDoDeteriorate(thing, c, true);
						}
					}
				}
			}
			if (room != null && !room.UsesOutdoorTemperature)
			{
				float temperature = room.Temperature;
				if (temperature > 0f)
				{
					float num = this.MeltAmountAt(temperature);
					if (num > 0f)
					{
						this.map.snowGrid.AddDepth(c, -num);
					}
					if (temperature > SteadyAtmosphereEffects.AutoIgnitionTemperatureRange.min)
					{
						float value = Rand.Value;
						if (value < SteadyAtmosphereEffects.AutoIgnitionTemperatureRange.InverseLerpThroughRange(temperature) * 0.7f)
						{
							FireUtility.TryStartFireIn(c, this.map, 0.1f);
						}
						if (value < 0.33f)
						{
							MoteMaker.ThrowHeatGlow(c, this.map, 2.3f);
						}
					}
				}
			}
			List<MapCondition> activeConditions = this.map.mapConditionManager.ActiveConditions;
			for (int k = 0; k < activeConditions.Count; k++)
			{
				activeConditions[k].DoCellSteadyEffects(c);
			}
		}

		public static bool InDeterioratingPosition(Thing t)
		{
			return !t.Position.Roofed(t.Map) && !SteadyAtmosphereEffects.ProtectedByEdifice(t.Position, t.Map);
		}

		private static bool ProtectedByEdifice(IntVec3 c, Map map)
		{
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice.def.building != null && edifice.def.building.preventDeterioration;
		}

		private float MeltAmountAt(float temperature)
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

		public void AddFallenSnowAt(IntVec3 c, float baseAmount)
		{
			if (this.snowNoise == null)
			{
				this.snowNoise = new Perlin(0.039999999105930328, 2.0, 0.5, 5, Rand.Range(0, 651431), QualityMode.Medium);
			}
			float num = this.snowNoise.GetValue(c);
			num += 1f;
			num *= 0.5f;
			if (num < 0.5f)
			{
				num = 0.5f;
			}
			float depthToAdd = baseAmount * num;
			this.map.snowGrid.AddDepth(c, depthToAdd);
		}

		public static float FinalDeteriorationRate(Thing t)
		{
			if (!t.def.useHitPoints || t.def.category != ThingCategory.Item)
			{
				return 0f;
			}
			return t.GetStatValue(StatDefOf.DeteriorationRate, true);
		}

		private void TryDoDeteriorate(Thing t, IntVec3 c, bool checkEdifice)
		{
			float num = SteadyAtmosphereEffects.FinalDeteriorationRate(t);
			if (num < 0.001f)
			{
				return;
			}
			float num2 = this.deteriorationRate * num / 36f;
			if (Rand.Value < num2 && (!checkEdifice || !SteadyAtmosphereEffects.ProtectedByEdifice(c, t.Map)))
			{
				t.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1, -1f, null, null, null));
			}
		}

		private void RollForRainFire()
		{
			float num = 0.2f * (float)this.map.listerBuildings.allBuildingsColonistElecFire.Count * this.map.weatherManager.RainRate;
			if (Rand.Value > num)
			{
				return;
			}
			Building building = this.map.listerBuildings.allBuildingsColonistElecFire.RandomElement<Building>();
			if (!this.map.roofGrid.Roofed(building.Position))
			{
				ThingWithComps thingWithComps = building;
				CompPowerTrader comp = thingWithComps.GetComp<CompPowerTrader>();
				if ((comp != null && comp.PowerOn && comp.Props.shortCircuitInRain) || (thingWithComps.GetComp<CompPowerBattery>() != null && thingWithComps.GetComp<CompPowerBattery>().StoredEnergy > 100f))
				{
					GenExplosion.DoExplosion(building.OccupiedRect().RandomCell, this.map, 1.9f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
					Find.LetterStack.ReceiveLetter("LetterLabelShortCircuit".Translate(), "ShortCircuitRain".Translate(new object[]
					{
						building.Label
					}), LetterType.BadUrgent, new TargetInfo(building.Position, building.Map, false), null);
				}
			}
		}
	}
}
