using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	[HasDebugOutput]
	public static class DebugOutputsMisc
	{
		[DebugOutput]
		public static void MiningResourceGeneration()
		{
			Func<ThingDef, ThingDef> mineable = delegate(ThingDef d)
			{
				List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (allDefsListForReading[i].building != null && allDefsListForReading[i].building.mineableThing == d)
					{
						return allDefsListForReading[i];
					}
				}
				return null;
			};
			Func<ThingDef, float> mineableCommonality = delegate(ThingDef d)
			{
				if (mineable(d) != null)
				{
					return mineable(d).building.mineableScatterCommonality;
				}
				return 0f;
			};
			Func<ThingDef, IntRange> mineableLumpSizeRange = delegate(ThingDef d)
			{
				if (mineable(d) != null)
				{
					return mineable(d).building.mineableScatterLumpSizeRange;
				}
				return IntRange.zero;
			};
			Func<ThingDef, float> mineableYield = delegate(ThingDef d)
			{
				if (mineable(d) != null)
				{
					return (float)mineable(d).building.mineableYield;
				}
				return 0f;
			};
			IEnumerable<ThingDef> arg_219_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.deepCommonality > 0f || mineableCommonality(d) > 0f
			select d;
			TableDataGetter<ThingDef>[] expr_7C = new TableDataGetter<ThingDef>[11];
			expr_7C[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_7C[1] = new TableDataGetter<ThingDef>("market value", (ThingDef d) => d.BaseMarketValue.ToString("F2"));
			expr_7C[2] = new TableDataGetter<ThingDef>("stackLimit", (ThingDef d) => d.stackLimit);
			expr_7C[3] = new TableDataGetter<ThingDef>("deep\ncommonality", (ThingDef d) => d.deepCommonality.ToString("F2"));
			expr_7C[4] = new TableDataGetter<ThingDef>("deep\nlump size", (ThingDef d) => d.deepLumpSizeRange);
			expr_7C[5] = new TableDataGetter<ThingDef>("deep count\nper cell", (ThingDef d) => d.deepCountPerCell);
			expr_7C[6] = new TableDataGetter<ThingDef>("deep count\nper portion", (ThingDef d) => d.deepCountPerPortion);
			expr_7C[7] = new TableDataGetter<ThingDef>("deep portion value", (ThingDef d) => ((float)d.deepCountPerPortion * d.BaseMarketValue).ToStringMoney(null));
			expr_7C[8] = new TableDataGetter<ThingDef>("mineable\ncommonality", (ThingDef d) => mineableCommonality(d).ToString("F2"));
			expr_7C[9] = new TableDataGetter<ThingDef>("mineable\nlump size", (ThingDef d) => mineableLumpSizeRange(d));
			expr_7C[10] = new TableDataGetter<ThingDef>("mineable yield\nper cell", (ThingDef d) => mineableYield(d));
			DebugTables.MakeTablesDialog<ThingDef>(arg_219_0, expr_7C);
		}

		[DebugOutput]
		public static void DefaultStuffs()
		{
			IEnumerable<ThingDef> arg_D5_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.MadeFromStuff && !d.IsFrame
			select d;
			TableDataGetter<ThingDef>[] expr_2D = new TableDataGetter<ThingDef>[4];
			expr_2D[0] = new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString());
			expr_2D[1] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_2D[2] = new TableDataGetter<ThingDef>("default stuff", (ThingDef d) => GenStuff.DefaultStuffFor(d).defName);
			expr_2D[3] = new TableDataGetter<ThingDef>("stuff categories", (ThingDef d) => (from c in d.stuffCategories
			select c.defName).ToCommaList(false));
			DebugTables.MakeTablesDialog<ThingDef>(arg_D5_0, expr_2D);
		}

		[DebugOutput]
		public static void Beauties()
		{
			IEnumerable<BuildableDef> arg_15F_0 = from d in DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>()).Where(delegate(BuildableDef d)
			{
				ThingDef thingDef = d as ThingDef;
				if (thingDef != null)
				{
					return BeautyUtility.BeautyRelevant(thingDef.category);
				}
				return d is TerrainDef;
			})
			orderby (int)d.GetStatValueAbstract(StatDefOf.Beauty, null) descending
			select d;
			TableDataGetter<BuildableDef>[] expr_63 = new TableDataGetter<BuildableDef>[6];
			expr_63[0] = new TableDataGetter<BuildableDef>("category", (BuildableDef d) => (!(d is ThingDef)) ? "Terrain" : ((ThingDef)d).category.ToString());
			expr_63[1] = new TableDataGetter<BuildableDef>("defName", (BuildableDef d) => d.defName);
			expr_63[2] = new TableDataGetter<BuildableDef>("beauty", (BuildableDef d) => d.GetStatValueAbstract(StatDefOf.Beauty, null).ToString());
			expr_63[3] = new TableDataGetter<BuildableDef>("market value", (BuildableDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue, null).ToString("F1"));
			expr_63[4] = new TableDataGetter<BuildableDef>("work to produce", (BuildableDef d) => DebugOutputsEconomy.WorkToProduceBest(d).ToString());
			expr_63[5] = new TableDataGetter<BuildableDef>("beauty per market value", (BuildableDef d) => (d.GetStatValueAbstract(StatDefOf.Beauty, null) <= 0f) ? string.Empty : (d.GetStatValueAbstract(StatDefOf.Beauty, null) / d.GetStatValueAbstract(StatDefOf.MarketValue, null)).ToString("F5"));
			DebugTables.MakeTablesDialog<BuildableDef>(arg_15F_0, expr_63);
		}

		[DebugOutput]
		public static void ThingsPowerAndHeat()
		{
			Func<ThingDef, CompProperties_HeatPusher> heatPusher = delegate(ThingDef d)
			{
				if (d.comps == null)
				{
					return null;
				}
				for (int i = 0; i < d.comps.Count; i++)
				{
					CompProperties_HeatPusher compProperties_HeatPusher = d.comps[i] as CompProperties_HeatPusher;
					if (compProperties_HeatPusher != null)
					{
						return compProperties_HeatPusher;
					}
				}
				return null;
			};
			IEnumerable<ThingDef> arg_1A7_0 = from d in DefDatabase<ThingDef>.AllDefs
			where (d.category == ThingCategory.Building || d.GetCompProperties<CompProperties_Power>() != null || heatPusher(d) != null) && !d.IsFrame
			select d;
			TableDataGetter<ThingDef>[] expr_46 = new TableDataGetter<ThingDef>[10];
			expr_46[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_46[1] = new TableDataGetter<ThingDef>("base\npower consumption", (ThingDef d) => (d.GetCompProperties<CompProperties_Power>() != null) ? d.GetCompProperties<CompProperties_Power>().basePowerConsumption.ToString() : string.Empty);
			expr_46[2] = new TableDataGetter<ThingDef>("short circuit\nin rain", (ThingDef d) => (d.GetCompProperties<CompProperties_Power>() != null) ? ((!d.GetCompProperties<CompProperties_Power>().shortCircuitInRain) ? string.Empty : "rainfire") : string.Empty);
			expr_46[3] = new TableDataGetter<ThingDef>("transmits\npower", (ThingDef d) => (d.GetCompProperties<CompProperties_Power>() != null) ? ((!d.GetCompProperties<CompProperties_Power>().transmitsPower) ? string.Empty : "transmit") : string.Empty);
			expr_46[4] = new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue);
			expr_46[5] = new TableDataGetter<ThingDef>("cost list", (ThingDef d) => DebugOutputsEconomy.CostListString(d, true, false));
			expr_46[6] = new TableDataGetter<ThingDef>("heat pusher\ncompClass", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).compClass.ToString() : string.Empty);
			expr_46[7] = new TableDataGetter<ThingDef>("heat pusher\nheat per sec", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).heatPerSecond.ToString() : string.Empty);
			expr_46[8] = new TableDataGetter<ThingDef>("heat pusher\nmin temp", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).heatPushMinTemperature.ToStringTemperature("F1") : string.Empty);
			expr_46[9] = new TableDataGetter<ThingDef>("heat pusher\nmax temp", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).heatPushMaxTemperature.ToStringTemperature("F1") : string.Empty);
			DebugTables.MakeTablesDialog<ThingDef>(arg_1A7_0, expr_46);
		}

		[DebugOutput]
		public static void FoodPoisonChances()
		{
			IEnumerable<ThingDef> arg_AB_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.IsIngestible
			select d;
			TableDataGetter<ThingDef>[] expr_2D = new TableDataGetter<ThingDef>[3];
			expr_2D[0] = new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category);
			expr_2D[1] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_2D[2] = new TableDataGetter<ThingDef>("food poison chance", delegate(ThingDef d)
			{
				CompProperties_FoodPoisonable compProperties = d.GetCompProperties<CompProperties_FoodPoisonable>();
				if (compProperties != null)
				{
					return "poisonable by cook";
				}
				float statValueAbstract = d.GetStatValueAbstract(StatDefOf.FoodPoisonChanceFixedHuman, null);
				if (statValueAbstract != 0f)
				{
					return statValueAbstract.ToStringPercent();
				}
				return string.Empty;
			});
			DebugTables.MakeTablesDialog<ThingDef>(arg_AB_0, expr_2D);
		}

		[DebugOutput]
		public static void TechLevels()
		{
			IEnumerable<ThingDef> arg_EF_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Building || d.category == ThingCategory.Item
			where !d.IsFrame && (d.building == null || !d.building.isNaturalRock)
			orderby (int)d.techLevel descending
			select d;
			TableDataGetter<ThingDef>[] expr_71 = new TableDataGetter<ThingDef>[3];
			expr_71[0] = new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString());
			expr_71[1] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_71[2] = new TableDataGetter<ThingDef>("tech level", (ThingDef d) => d.techLevel.ToString());
			DebugTables.MakeTablesDialog<ThingDef>(arg_EF_0, expr_71);
		}

		[DebugOutput]
		public static void Stuffs()
		{
			Func<ThingDef, StatDef, string> getStatFactorString = delegate(ThingDef d, StatDef stat)
			{
				if (d.stuffProps.statFactors == null)
				{
					return string.Empty;
				}
				StatModifier statModifier = d.stuffProps.statFactors.FirstOrDefault((StatModifier fa) => fa.stat == stat);
				if (statModifier == null)
				{
					return string.Empty;
				}
				return stat.ValueToString(statModifier.value, ToStringNumberSense.Absolute);
			};
			Func<ThingDef, float> meleeDpsSharpFactorOverall = delegate(ThingDef d)
			{
				float statValueAbstract = d.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier, null);
				float statFactorFromList = d.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier);
				return statValueAbstract / statFactorFromList;
			};
			Func<ThingDef, float> meleeDpsBluntFactorOverall = delegate(ThingDef d)
			{
				float statValueAbstract = d.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier, null);
				float statFactorFromList = d.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier);
				return statValueAbstract / statFactorFromList;
			};
			IEnumerable<ThingDef> arg_425_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.IsStuff
			orderby d.BaseMarketValue
			select d;
			TableDataGetter<ThingDef>[] expr_BF = new TableDataGetter<ThingDef>[24];
			expr_BF[0] = new TableDataGetter<ThingDef>("fabric", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Fabric).ToStringCheckBlank());
			expr_BF[1] = new TableDataGetter<ThingDef>("leather", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Leathery).ToStringCheckBlank());
			expr_BF[2] = new TableDataGetter<ThingDef>("metal", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Metallic).ToStringCheckBlank());
			expr_BF[3] = new TableDataGetter<ThingDef>("stony", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Stony).ToStringCheckBlank());
			expr_BF[4] = new TableDataGetter<ThingDef>("woody", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Woody).ToStringCheckBlank());
			expr_BF[5] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_BF[6] = new TableDataGetter<ThingDef>("base\nmarket\nvalue", (ThingDef d) => d.BaseMarketValue.ToStringMoney(null));
			expr_BF[7] = new TableDataGetter<ThingDef>("melee\ncooldown\nmultiplier", (ThingDef d) => getStatFactorString(d, StatDefOf.MeleeWeapon_CooldownMultiplier));
			expr_BF[8] = new TableDataGetter<ThingDef>("melee\nsharp\ndamage\nmultiplier", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier, null).ToString("F2"));
			expr_BF[9] = new TableDataGetter<ThingDef>("melee\nsharp\ndps factor\noverall", (ThingDef d) => meleeDpsSharpFactorOverall(d).ToString("F2"));
			expr_BF[10] = new TableDataGetter<ThingDef>("melee\nblunt\ndamage\nmultiplier", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier, null).ToString("F2"));
			expr_BF[11] = new TableDataGetter<ThingDef>("melee\nblunt\ndps factor\noverall", (ThingDef d) => meleeDpsBluntFactorOverall(d).ToString("F2"));
			expr_BF[12] = new TableDataGetter<ThingDef>("armor power\nsharp", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Sharp, null).ToString("F2"));
			expr_BF[13] = new TableDataGetter<ThingDef>("armor power\nblunt", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Blunt, null).ToString("F2"));
			expr_BF[14] = new TableDataGetter<ThingDef>("armor power\nheat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Heat, null).ToString("F2"));
			expr_BF[15] = new TableDataGetter<ThingDef>("insulation\ncold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Cold, null).ToString("F2"));
			expr_BF[16] = new TableDataGetter<ThingDef>("insulation\nheat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Heat, null).ToString("F2"));
			expr_BF[17] = new TableDataGetter<ThingDef>("flammability", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Flammability, null).ToString("F2"));
			expr_BF[18] = new TableDataGetter<ThingDef>("factor\nFlammability", (ThingDef d) => getStatFactorString(d, StatDefOf.Flammability));
			expr_BF[19] = new TableDataGetter<ThingDef>("factor\nWorkToMake", (ThingDef d) => getStatFactorString(d, StatDefOf.WorkToMake));
			expr_BF[20] = new TableDataGetter<ThingDef>("factor\nWorkToBuild", (ThingDef d) => getStatFactorString(d, StatDefOf.WorkToBuild));
			expr_BF[21] = new TableDataGetter<ThingDef>("factor\nMaxHp", (ThingDef d) => getStatFactorString(d, StatDefOf.MaxHitPoints));
			expr_BF[22] = new TableDataGetter<ThingDef>("factor\nBeauty", (ThingDef d) => getStatFactorString(d, StatDefOf.Beauty));
			expr_BF[23] = new TableDataGetter<ThingDef>("factor\nDoorspeed", (ThingDef d) => getStatFactorString(d, StatDefOf.DoorOpenSpeed));
			DebugTables.MakeTablesDialog<ThingDef>(arg_425_0, expr_BF);
		}

		[DebugOutput]
		public static void Drugs()
		{
			IEnumerable<ThingDef> arg_AB_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.IsDrug
			select d;
			TableDataGetter<ThingDef>[] expr_2D = new TableDataGetter<ThingDef>[3];
			expr_2D[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_2D[1] = new TableDataGetter<ThingDef>("pleasure", (ThingDef d) => (!d.IsPleasureDrug) ? string.Empty : "pleasure");
			expr_2D[2] = new TableDataGetter<ThingDef>("non-medical", (ThingDef d) => (!d.IsNonMedicalDrug) ? string.Empty : "non-medical");
			DebugTables.MakeTablesDialog<ThingDef>(arg_AB_0, expr_2D);
		}

		[DebugOutput]
		public static void Medicines()
		{
			List<float> list = new List<float>();
			list.Add(0.3f);
			list.AddRange(from d in DefDatabase<ThingDef>.AllDefs
			where typeof(Medicine).IsAssignableFrom(d.thingClass)
			select d.GetStatValueAbstract(StatDefOf.MedicalPotency, null));
			SkillNeed_Direct skillNeed_Direct = (SkillNeed_Direct)StatDefOf.MedicalTendQuality.skillNeedFactors[0];
			TableDataGetter<float>[] array = new TableDataGetter<float>[21];
			array[0] = new TableDataGetter<float>("potency", new Func<float, string>(GenText.ToStringPercent));
			for (int i = 0; i < 20; i++)
			{
				float factor = skillNeed_Direct.valuesPerLevel[i];
				array[i + 1] = new TableDataGetter<float>((i + 1).ToString(), (float p) => (p * factor).ToStringPercent());
			}
			DebugTables.MakeTablesDialog<float>(list, array);
		}

		[DebugOutput]
		public static void ShootingAccuracy()
		{
			StatDef stat = StatDefOf.ShootingAccuracyPawn;
			Func<int, float, int, float> accAtDistance = delegate(int level, float dist, int traitDegree)
			{
				float num = 1f;
				if (traitDegree != 0)
				{
					float value = TraitDef.Named("ShootingAccuracy").DataAtDegree(traitDegree).statOffsets.First((StatModifier so) => so.stat == stat).value;
					num += value;
				}
				foreach (SkillNeed current in stat.skillNeedFactors)
				{
					SkillNeed_Direct skillNeed_Direct = current as SkillNeed_Direct;
					num *= skillNeed_Direct.valuesPerLevel[level];
				}
				num = stat.postProcessCurve.Evaluate(num);
				return Mathf.Pow(num, dist);
			};
			List<int> list = new List<int>();
			for (int i = 0; i <= 20; i++)
			{
				list.Add(i);
			}
			IEnumerable<int> arg_249_0 = list;
			TableDataGetter<int>[] expr_4B = new TableDataGetter<int>[18];
			expr_4B[0] = new TableDataGetter<int>("No trait skill", (int lev) => lev.ToString());
			expr_4B[1] = new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, 0).ToStringPercent("F2"));
			expr_4B[2] = new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, 0).ToStringPercent("F2"));
			expr_4B[3] = new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, 0).ToStringPercent("F2"));
			expr_4B[4] = new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, 0).ToStringPercent("F2"));
			expr_4B[5] = new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, 0).ToStringPercent("F2"));
			expr_4B[6] = new TableDataGetter<int>("Careful shooter skill", (int lev) => lev.ToString());
			expr_4B[7] = new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, 1).ToStringPercent("F2"));
			expr_4B[8] = new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, 1).ToStringPercent("F2"));
			expr_4B[9] = new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, 1).ToStringPercent("F2"));
			expr_4B[10] = new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, 1).ToStringPercent("F2"));
			expr_4B[11] = new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, 1).ToStringPercent("F2"));
			expr_4B[12] = new TableDataGetter<int>("Trigger-happy skill", (int lev) => lev.ToString());
			expr_4B[13] = new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, -1).ToStringPercent("F2"));
			expr_4B[14] = new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, -1).ToStringPercent("F2"));
			expr_4B[15] = new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, -1).ToStringPercent("F2"));
			expr_4B[16] = new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, -1).ToStringPercent("F2"));
			expr_4B[17] = new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, -1).ToStringPercent("F2"));
			DebugTables.MakeTablesDialog<int>(arg_249_0, expr_4B);
		}

		[DebugOutput, ModeRestrictionPlay]
		public static void TemperatureData()
		{
			Find.CurrentMap.mapTemperature.DebugLogTemps();
		}

		[DebugOutput, ModeRestrictionPlay]
		public static void WeatherChances()
		{
			Find.CurrentMap.weatherDecider.LogWeatherChances();
		}

		[DebugOutput, ModeRestrictionPlay]
		public static void CelestialGlow()
		{
			GenCelestial.LogSunGlowForYear();
		}

		[DebugOutput, ModeRestrictionPlay]
		public static void SunAngle()
		{
			GenCelestial.LogSunAngleForYear();
		}

		[DebugOutput, ModeRestrictionPlay]
		public static void FallColor()
		{
			PlantUtility.LogFallColorForYear();
		}

		[DebugOutput, ModeRestrictionPlay]
		public static void PawnsListAllOnMap()
		{
			Find.CurrentMap.mapPawns.LogListedPawns();
		}

		[DebugOutput, ModeRestrictionPlay]
		public static void WindSpeeds()
		{
			Find.CurrentMap.windManager.LogWindSpeeds();
		}

		[DebugOutput, ModeRestrictionPlay]
		public static void MapPawnsList()
		{
			Find.CurrentMap.mapPawns.LogListedPawns();
		}

		[DebugOutput]
		public static void Lords()
		{
			Find.CurrentMap.lordManager.LogLords();
		}

		[DebugOutput]
		public static void DamageTest()
		{
			ThingDef thingDef = ThingDef.Named("Bullet_BoltActionRifle");
			PawnKindDef slave = PawnKindDefOf.Slave;
			Faction faction = FactionUtility.DefaultFactionFrom(slave.defaultFactionType);
			DamageInfo dinfo = new DamageInfo(thingDef.projectile.damageDef, (float)thingDef.projectile.GetDamageAmount(null, null), thingDef.projectile.GetArmorPenetration(null, null), -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
			int num = 0;
			int num2 = 0;
			DefMap<BodyPartDef, int> defMap = new DefMap<BodyPartDef, int>();
			for (int i = 0; i < 500; i++)
			{
				PawnGenerationRequest request = new PawnGenerationRequest(slave, faction, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null, null);
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				List<BodyPartDef> list = (from hd in pawn.health.hediffSet.GetMissingPartsCommonAncestors()
				select hd.Part.def).ToList<BodyPartDef>();
				for (int j = 0; j < 2; j++)
				{
					pawn.TakeDamage(dinfo);
					if (pawn.Dead)
					{
						num++;
						break;
					}
				}
				List<BodyPartDef> list2 = (from hd in pawn.health.hediffSet.GetMissingPartsCommonAncestors()
				select hd.Part.def).ToList<BodyPartDef>();
				if (list2.Count > list.Count)
				{
					num2++;
					foreach (BodyPartDef current in list2)
					{
						DefMap<BodyPartDef, int> defMap2;
						BodyPartDef def;
						(defMap2 = defMap)[def = current] = defMap2[def] + 1;
					}
					foreach (BodyPartDef current2 in list)
					{
						DefMap<BodyPartDef, int> defMap2;
						BodyPartDef def2;
						(defMap2 = defMap)[def2 = current2] = defMap2[def2] - 1;
					}
				}
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Damage test");
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"Hit ",
				500,
				" ",
				slave.label,
				"s with ",
				2,
				"x ",
				thingDef.label,
				" (",
				thingDef.projectile.GetDamageAmount(null, null),
				" damage) each. Results:"
			}));
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"Killed: ",
				num,
				" / ",
				500,
				" (",
				((float)num / 500f).ToStringPercent(),
				")"
			}));
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"Part losers: ",
				num2,
				" / ",
				500,
				" (",
				((float)num2 / 500f).ToStringPercent(),
				")"
			}));
			stringBuilder.AppendLine("Parts lost:");
			foreach (BodyPartDef current3 in DefDatabase<BodyPartDef>.AllDefs)
			{
				if (defMap[current3] > 0)
				{
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"   ",
						current3.label,
						": ",
						defMap[current3]
					}));
				}
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		[DebugOutput]
		public static void BodyPartTagGroups()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (BodyDef current in DefDatabase<BodyDef>.AllDefs)
			{
				BodyDef localBd = current;
				FloatMenuOption item = new FloatMenuOption(localBd.defName, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(localBd.defName + "\n----------------");
					foreach (BodyPartTagDef tag in (from elem in localBd.AllParts.SelectMany((BodyPartRecord part) => part.def.tags)
					orderby elem
					select elem).Distinct<BodyPartTagDef>())
					{
						stringBuilder.AppendLine(tag.defName);
						foreach (BodyPartRecord current2 in from part in localBd.AllParts
						where part.def.tags.Contains(tag)
						orderby part.def.defName
						select part)
						{
							stringBuilder.AppendLine("  " + current2.def.defName);
						}
					}
					Log.Message(stringBuilder.ToString(), false);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void MinifiableTags()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (current.Minifiable)
				{
					stringBuilder.Append(current.defName);
					if (!current.tradeTags.NullOrEmpty<string>())
					{
						stringBuilder.Append(" - ");
						stringBuilder.Append(current.tradeTags.ToCommaList(false));
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		[DebugOutput]
		public static void ListSolidBackstories()
		{
			IEnumerable<string> enumerable = SolidBioDatabase.allBios.SelectMany((PawnBio bio) => bio.adulthood.spawnCategories).Distinct<string>();
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (string current in enumerable)
			{
				string catInner = current;
				FloatMenuOption item = new FloatMenuOption(catInner, delegate
				{
					IEnumerable<PawnBio> enumerable2 = from b in SolidBioDatabase.allBios
					where b.adulthood.spawnCategories.Contains(catInner)
					select b;
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"Backstories with category: ",
						catInner,
						" (",
						enumerable2.Count<PawnBio>(),
						")"
					}));
					foreach (PawnBio current2 in enumerable2)
					{
						stringBuilder.AppendLine(current2.ToString());
					}
					Log.Message(stringBuilder.ToString(), false);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void ThingSetMakerTest()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ThingSetMakerDef current in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef localDef = current;
				DebugMenuOption item = new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
				{
					Action<ThingSetMakerParams> generate = delegate(ThingSetMakerParams parms)
					{
						StringBuilder stringBuilder = new StringBuilder();
						float num = 0f;
						float num2 = 0f;
						for (int i = 0; i < 50; i++)
						{
							List<Thing> list3 = localDef.root.Generate(parms);
							if (stringBuilder.Length > 0)
							{
								stringBuilder.AppendLine();
							}
							float num3 = 0f;
							float num4 = 0f;
							for (int j = 0; j < list3.Count; j++)
							{
								stringBuilder.AppendLine("-" + list3[j].LabelCap + " - $" + (list3[j].MarketValue * (float)list3[j].stackCount).ToString("F0"));
								num3 += list3[j].MarketValue * (float)list3[j].stackCount;
								if (!(list3[j] is Pawn))
								{
									num4 += list3[j].GetStatValue(StatDefOf.Mass, true) * (float)list3[j].stackCount;
								}
								list3[j].Destroy(DestroyMode.Vanish);
							}
							num += num3;
							num2 += num4;
							stringBuilder.AppendLine("   Total market value: $" + num3.ToString("F0"));
							stringBuilder.AppendLine("   Total mass: " + num4.ToStringMass());
						}
						StringBuilder stringBuilder2 = new StringBuilder();
						stringBuilder2.AppendLine("Default thing sets generated by: " + localDef.defName);
						string nonNullFieldsDebugInfo = Gen.GetNonNullFieldsDebugInfo(localDef.root.fixedParams);
						stringBuilder2.AppendLine("root fixedParams: " + ((!nonNullFieldsDebugInfo.NullOrEmpty()) ? nonNullFieldsDebugInfo : "none"));
						string nonNullFieldsDebugInfo2 = Gen.GetNonNullFieldsDebugInfo(parms);
						if (!nonNullFieldsDebugInfo2.NullOrEmpty())
						{
							stringBuilder2.AppendLine("(used custom debug params: " + nonNullFieldsDebugInfo2 + ")");
						}
						stringBuilder2.AppendLine("Average market value: $" + (num / 50f).ToString("F1"));
						stringBuilder2.AppendLine("Average mass: " + (num2 / 50f).ToStringMass());
						stringBuilder2.AppendLine();
						stringBuilder2.Append(stringBuilder.ToString());
						Log.Message(stringBuilder2.ToString(), false);
					};
					if (localDef == ThingSetMakerDefOf.TraderStock)
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (Faction current2 in Find.FactionManager.AllFactions)
						{
							if (current2 != Faction.OfPlayer)
							{
								Faction localF = current2;
								list2.Add(new DebugMenuOption(localF.Name + " (" + localF.def.defName + ")", DebugMenuOptionMode.Action, delegate
								{
									List<DebugMenuOption> list3 = new List<DebugMenuOption>();
									foreach (TraderKindDef current3 in (from x in DefDatabase<TraderKindDef>.AllDefs
									where x.orbital
									select x).Concat(localF.def.caravanTraderKinds).Concat(localF.def.visitorTraderKinds).Concat(localF.def.baseTraderKinds))
									{
										TraderKindDef localKind = current3;
										list3.Add(new DebugMenuOption(localKind.defName, DebugMenuOptionMode.Action, delegate
										{
											ThingSetMakerParams obj = default(ThingSetMakerParams);
											obj.traderFaction = localF;
											obj.traderDef = localKind;
											generate(obj);
										}));
									}
									Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
								}));
							}
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}
					else
					{
						generate(localDef.debugParams);
					}
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void ThingSetMakerPossibleDefs()
		{
			Dictionary<ThingSetMakerDef, List<ThingDef>> generatableThings = new Dictionary<ThingSetMakerDef, List<ThingDef>>();
			foreach (ThingSetMakerDef current in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef thingSetMakerDef = current;
				generatableThings[current] = thingSetMakerDef.root.AllGeneratableThingsDebug(thingSetMakerDef.debugParams).ToList<ThingDef>();
			}
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName));
			list.Add(new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue.ToStringMoney(null)));
			list.Add(new TableDataGetter<ThingDef>("mass", (ThingDef d) => d.BaseMass.ToStringMass()));
			foreach (ThingSetMakerDef current2 in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef localDef = current2;
				list.Add(new TableDataGetter<ThingDef>(localDef.defName.Shorten(), (ThingDef d) => generatableThings[localDef].Contains(d).ToStringCheckBlank()));
			}
			DebugTables.MakeTablesDialog<ThingDef>(from d in DefDatabase<ThingDef>.AllDefs
			where (d.category == ThingCategory.Item && !d.IsCorpse && !d.isUnfinishedThing) || (d.category == ThingCategory.Building && d.Minifiable) || d.category == ThingCategory.Pawn
			orderby d.BaseMarketValue descending
			select d, list.ToArray());
		}

		[DebugOutput]
		public static void ThingSetMakerSampled()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ThingSetMakerDef current in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef localDef = current;
				DebugMenuOption item = new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
				{
					Action<ThingSetMakerParams> generate = delegate(ThingSetMakerParams parms)
					{
						Dictionary<ThingDef, int> counts = new Dictionary<ThingDef, int>();
						for (int i = 0; i < 500; i++)
						{
							List<Thing> list3 = localDef.root.Generate(parms);
							foreach (ThingDef current3 in (from th in list3
							select th.GetInnerIfMinified().def).Distinct<ThingDef>())
							{
								if (!counts.ContainsKey(current3))
								{
									counts.Add(current3, 0);
								}
								Dictionary<ThingDef, int> counts2;
								ThingDef key;
								(counts2 = counts)[key = current3] = counts2[key] + 1;
							}
							for (int j = 0; j < list3.Count; j++)
							{
								list3[j].Destroy(DestroyMode.Vanish);
							}
						}
						IEnumerable<ThingDef> arg_1E3_0 = from d in DefDatabase<ThingDef>.AllDefs
						where counts.ContainsKey(d)
						orderby counts[d] descending
						select d;
						TableDataGetter<ThingDef>[] expr_137 = new TableDataGetter<ThingDef>[4];
						expr_137[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
						expr_137[1] = new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue.ToStringMoney(null));
						expr_137[2] = new TableDataGetter<ThingDef>("mass", (ThingDef d) => d.BaseMass.ToStringMass());
						expr_137[3] = new TableDataGetter<ThingDef>("appearance rate in " + localDef.defName, (ThingDef d) => ((float)counts[d] / 500f).ToStringPercent());
						DebugTables.MakeTablesDialog<ThingDef>(arg_1E3_0, expr_137);
					};
					if (localDef == ThingSetMakerDefOf.TraderStock)
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (Faction current2 in Find.FactionManager.AllFactions)
						{
							if (current2 != Faction.OfPlayer)
							{
								Faction localF = current2;
								list2.Add(new DebugMenuOption(localF.Name + " (" + localF.def.defName + ")", DebugMenuOptionMode.Action, delegate
								{
									List<DebugMenuOption> list3 = new List<DebugMenuOption>();
									foreach (TraderKindDef current3 in (from x in DefDatabase<TraderKindDef>.AllDefs
									where x.orbital
									select x).Concat(localF.def.caravanTraderKinds).Concat(localF.def.visitorTraderKinds).Concat(localF.def.baseTraderKinds))
									{
										TraderKindDef localKind = current3;
										list3.Add(new DebugMenuOption(localKind.defName, DebugMenuOptionMode.Action, delegate
										{
											ThingSetMakerParams obj = default(ThingSetMakerParams);
											obj.traderFaction = localF;
											obj.traderDef = localKind;
											generate(obj);
										}));
									}
									Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
								}));
							}
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}
					else
					{
						generate(localDef.debugParams);
					}
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void WorkDisables()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (PawnKindDef current in from ki in DefDatabase<PawnKindDef>.AllDefs
			where ki.RaceProps.Humanlike
			select ki)
			{
				PawnKindDef pkInner = current;
				Faction faction = FactionUtility.DefaultFactionFrom(pkInner.defaultFactionType);
				FloatMenuOption item = new FloatMenuOption(pkInner.defName, delegate
				{
					int num = 500;
					DefMap<WorkTypeDef, int> defMap = new DefMap<WorkTypeDef, int>();
					for (int i = 0; i < num; i++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(pkInner, faction);
						foreach (WorkTypeDef current2 in pawn.story.DisabledWorkTypes)
						{
							DefMap<WorkTypeDef, int> defMap2;
							WorkTypeDef def;
							(defMap2 = defMap)[def = current2] = defMap2[def] + 1;
						}
					}
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"Generated ",
						num,
						" pawns of kind ",
						pkInner.defName,
						" on faction ",
						faction.ToStringSafe<Faction>()
					}));
					stringBuilder.AppendLine("Work types disabled:");
					foreach (WorkTypeDef current3 in DefDatabase<WorkTypeDef>.AllDefs)
					{
						if (current3.workTags != WorkTags.None)
						{
							stringBuilder.AppendLine(string.Concat(new object[]
							{
								"   ",
								current3.defName,
								": ",
								defMap[current3],
								"        ",
								((float)defMap[current3] / (float)num).ToStringPercent()
							}));
						}
					}
					IEnumerable<Backstory> enumerable = BackstoryDatabase.allBackstories.Select((KeyValuePair<string, Backstory> kvp) => kvp.Value);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Backstories WorkTypeDef disable rates (there are " + enumerable.Count<Backstory>() + " backstories):");
					foreach (WorkTypeDef wt in DefDatabase<WorkTypeDef>.AllDefs)
					{
						int num2 = 0;
						foreach (Backstory current4 in enumerable)
						{
							if (current4.DisabledWorkTypes.Any((WorkTypeDef wd) => wt == wd))
							{
								num2++;
							}
						}
						stringBuilder.AppendLine(string.Concat(new object[]
						{
							"   ",
							wt.defName,
							": ",
							num2,
							"     ",
							((float)num2 / (float)BackstoryDatabase.allBackstories.Count).ToStringPercent()
						}));
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Backstories WorkTag disable rates (there are " + enumerable.Count<Backstory>() + " backstories):");
					foreach (WorkTags workTags in Enum.GetValues(typeof(WorkTags)))
					{
						int num3 = 0;
						foreach (Backstory current5 in enumerable)
						{
							if ((workTags & current5.workDisables) != WorkTags.None)
							{
								num3++;
							}
						}
						stringBuilder.AppendLine(string.Concat(new object[]
						{
							"   ",
							workTags,
							": ",
							num3,
							"     ",
							((float)num3 / (float)BackstoryDatabase.allBackstories.Count).ToStringPercent()
						}));
					}
					Log.Message(stringBuilder.ToString(), false);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void KeyStrings()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
			{
				stringBuilder.AppendLine(k.ToString() + " - " + k.ToStringReadable());
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		[DebugOutput]
		public static void SocialPropernessMatters()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Social-properness-matters things:");
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (current.socialPropernessMatters)
				{
					stringBuilder.AppendLine(string.Format("  {0}", current.defName));
				}
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		[DebugOutput]
		public static void FoodPreferability()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Food, ordered by preferability:");
			foreach (ThingDef current in from td in DefDatabase<ThingDef>.AllDefs
			where td.ingestible != null
			orderby td.ingestible.preferability
			select td)
			{
				stringBuilder.AppendLine(string.Format("  {0}: {1}", current.ingestible.preferability, current.defName));
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		[DebugOutput]
		public static void MapDanger()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Map danger status:");
			foreach (Map current in Find.Maps)
			{
				stringBuilder.AppendLine(string.Format("{0}: {1}", current, current.dangerWatcher.DangerRating));
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		[DebugOutput]
		public static void DefNames()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type type in from def in GenDefDatabase.AllDefTypesWithDatabases()
			orderby def.Name
			select def)
			{
				DebugMenuOption item = new DebugMenuOption(type.Name, DebugMenuOptionMode.Action, delegate
				{
					IEnumerable source = (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), type, "AllDefs");
					int num = 0;
					StringBuilder stringBuilder = new StringBuilder();
					foreach (Def current in source.Cast<Def>())
					{
						stringBuilder.AppendLine(current.defName);
						num++;
						if (num >= 500)
						{
							Log.Message(stringBuilder.ToString(), false);
							stringBuilder = new StringBuilder();
							num = 0;
						}
					}
					Log.Message(stringBuilder.ToString(), false);
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void DefNamesAll()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (Type current in from def in GenDefDatabase.AllDefTypesWithDatabases()
			orderby def.Name
			select def)
			{
				IEnumerable source = (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), current, "AllDefs");
				stringBuilder.AppendLine("--    " + current.ToString());
				foreach (Def current2 in source.Cast<Def>().OrderBy((Def def) => def.defName))
				{
					stringBuilder.AppendLine(current2.defName);
					num++;
					if (num >= 500)
					{
						Log.Message(stringBuilder.ToString(), false);
						stringBuilder = new StringBuilder();
						num = 0;
					}
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		[DebugOutput]
		public static void DefLabels()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type type in from def in GenDefDatabase.AllDefTypesWithDatabases()
			orderby def.Name
			select def)
			{
				DebugMenuOption item = new DebugMenuOption(type.Name, DebugMenuOptionMode.Action, delegate
				{
					IEnumerable source = (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), type, "AllDefs");
					int num = 0;
					StringBuilder stringBuilder = new StringBuilder();
					foreach (Def current in source.Cast<Def>())
					{
						stringBuilder.AppendLine(current.label);
						num++;
						if (num >= 500)
						{
							Log.Message(stringBuilder.ToString(), false);
							stringBuilder = new StringBuilder();
							num = 0;
						}
					}
					Log.Message(stringBuilder.ToString(), false);
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void Bodies()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (BodyDef current in DefDatabase<BodyDef>.AllDefs)
			{
				BodyDef localBd = current;
				list.Add(new FloatMenuOption(localBd.defName, delegate
				{
					IEnumerable<BodyPartRecord> arg_159_0 = from d in localBd.AllParts
					orderby d.height descending
					select d;
					TableDataGetter<BodyPartRecord>[] expr_33 = new TableDataGetter<BodyPartRecord>[7];
					expr_33[0] = new TableDataGetter<BodyPartRecord>("defName", (BodyPartRecord d) => d.def.defName);
					expr_33[1] = new TableDataGetter<BodyPartRecord>("hitPoints\n(non-adjusted)", (BodyPartRecord d) => d.def.hitPoints);
					expr_33[2] = new TableDataGetter<BodyPartRecord>("coverage", (BodyPartRecord d) => d.coverage.ToStringPercent());
					expr_33[3] = new TableDataGetter<BodyPartRecord>("coverageAbsWithChildren", (BodyPartRecord d) => d.coverageAbsWithChildren.ToStringPercent());
					expr_33[4] = new TableDataGetter<BodyPartRecord>("coverageAbs", (BodyPartRecord d) => d.coverageAbs.ToStringPercent());
					expr_33[5] = new TableDataGetter<BodyPartRecord>("depth", (BodyPartRecord d) => d.depth.ToString());
					expr_33[6] = new TableDataGetter<BodyPartRecord>("height", (BodyPartRecord d) => d.height.ToString());
					DebugTables.MakeTablesDialog<BodyPartRecord>(arg_159_0, expr_33);
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void BodyParts()
		{
			IEnumerable<BodyPartDef> arg_2B3_0 = DefDatabase<BodyPartDef>.AllDefs;
			TableDataGetter<BodyPartDef>[] expr_0C = new TableDataGetter<BodyPartDef>[16];
			expr_0C[0] = new TableDataGetter<BodyPartDef>("defName", (BodyPartDef d) => d.defName);
			expr_0C[1] = new TableDataGetter<BodyPartDef>("hit\npoints", (BodyPartDef d) => d.hitPoints);
			expr_0C[2] = new TableDataGetter<BodyPartDef>("bleeding\nate\nmultiplier", (BodyPartDef d) => d.bleedRate.ToStringPercent());
			expr_0C[3] = new TableDataGetter<BodyPartDef>("perm injury\nchance factor", (BodyPartDef d) => d.permanentInjuryChanceFactor.ToStringPercent());
			expr_0C[4] = new TableDataGetter<BodyPartDef>("frostbite\nvulnerability", (BodyPartDef d) => d.frostbiteVulnerability);
			expr_0C[5] = new TableDataGetter<BodyPartDef>("solid", (BodyPartDef d) => (!d.IsSolidInDefinition_Debug) ? string.Empty : "S");
			expr_0C[6] = new TableDataGetter<BodyPartDef>("beauty\nrelated", (BodyPartDef d) => (!d.beautyRelated) ? string.Empty : "B");
			expr_0C[7] = new TableDataGetter<BodyPartDef>("alive", (BodyPartDef d) => (!d.alive) ? string.Empty : "A");
			expr_0C[8] = new TableDataGetter<BodyPartDef>("conceptual", (BodyPartDef d) => (!d.conceptual) ? string.Empty : "C");
			expr_0C[9] = new TableDataGetter<BodyPartDef>("can\nsuggest\namputation", (BodyPartDef d) => (!d.canSuggestAmputation) ? "no A" : string.Empty);
			expr_0C[10] = new TableDataGetter<BodyPartDef>("socketed", (BodyPartDef d) => (!d.socketed) ? string.Empty : "DoL");
			expr_0C[11] = new TableDataGetter<BodyPartDef>("skin covered", (BodyPartDef d) => (!d.IsSkinCoveredInDefinition_Debug) ? string.Empty : "skin");
			expr_0C[12] = new TableDataGetter<BodyPartDef>("pawn generator\ncan amputate", (BodyPartDef d) => (!d.pawnGeneratorCanAmputate) ? string.Empty : "amp");
			expr_0C[13] = new TableDataGetter<BodyPartDef>("spawn thing\non removed", (BodyPartDef d) => d.spawnThingOnRemoved);
			expr_0C[14] = new TableDataGetter<BodyPartDef>("hitChanceFactors", delegate(BodyPartDef d)
			{
				string arg_43_0;
				if (d.hitChanceFactors == null)
				{
					arg_43_0 = string.Empty;
				}
				else
				{
					arg_43_0 = (from kvp in d.hitChanceFactors
					select kvp.ToString()).ToCommaList(false);
				}
				return arg_43_0;
			});
			expr_0C[15] = new TableDataGetter<BodyPartDef>("tags", delegate(BodyPartDef d)
			{
				string arg_43_0;
				if (d.tags == null)
				{
					arg_43_0 = string.Empty;
				}
				else
				{
					arg_43_0 = (from t in d.tags
					select t.defName).ToCommaList(false);
				}
				return arg_43_0;
			});
			DebugTables.MakeTablesDialog<BodyPartDef>(arg_2B3_0, expr_0C);
		}

		[DebugOutput]
		public static void TraderKinds()
		{
			IEnumerable<TraderKindDef> arg_5F_0 = DefDatabase<TraderKindDef>.AllDefs;
			TableDataGetter<TraderKindDef>[] expr_0B = new TableDataGetter<TraderKindDef>[2];
			expr_0B[0] = new TableDataGetter<TraderKindDef>("defName", (TraderKindDef d) => d.defName);
			expr_0B[1] = new TableDataGetter<TraderKindDef>("commonality", (TraderKindDef d) => d.CalculatedCommonality.ToString("F2"));
			DebugTables.MakeTablesDialog<TraderKindDef>(arg_5F_0, expr_0B);
		}

		[DebugOutput]
		public static void TraderKindThings()
		{
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName));
			foreach (TraderKindDef current in DefDatabase<TraderKindDef>.AllDefs)
			{
				TraderKindDef localTk = current;
				string text = localTk.defName;
				text = text.Replace("Caravan", "Car");
				text = text.Replace("Visitor", "Vis");
				text = text.Replace("Orbital", "Orb");
				text = text.Replace("Neolithic", "Ne");
				text = text.Replace("Outlander", "Out");
				text = text.Replace("_", " ");
				text = text.Shorten();
				list.Add(new TableDataGetter<ThingDef>(text, (ThingDef td) => localTk.WillTrade(td).ToStringCheckBlank()));
			}
			DebugTables.MakeTablesDialog<ThingDef>(from d in DefDatabase<ThingDef>.AllDefs
			where (d.category == ThingCategory.Item && d.BaseMarketValue > 0.001f && !d.isUnfinishedThing && !d.IsCorpse && !d.destroyOnDrop && d != ThingDefOf.Silver && !d.thingCategories.NullOrEmpty<ThingCategoryDef>()) || (d.category == ThingCategory.Building && d.Minifiable) || d.category == ThingCategory.Pawn
			orderby d.thingCategories.NullOrEmpty<ThingCategoryDef>() ? "zzzzzzz" : d.thingCategories[0].defName, d.BaseMarketValue
			select d, list.ToArray());
		}

		[DebugOutput]
		public static void Surgeries()
		{
			IEnumerable<RecipeDef> arg_14B_0 = from d in DefDatabase<RecipeDef>.AllDefs
			where d.IsSurgery
			orderby d.WorkAmountTotal(null) descending
			select d;
			TableDataGetter<RecipeDef>[] expr_4F = new TableDataGetter<RecipeDef>[6];
			expr_4F[0] = new TableDataGetter<RecipeDef>("defName", (RecipeDef d) => d.defName);
			expr_4F[1] = new TableDataGetter<RecipeDef>("work", (RecipeDef d) => d.WorkAmountTotal(null).ToString("F0"));
			expr_4F[2] = new TableDataGetter<RecipeDef>("ingredients", (RecipeDef d) => (from ing in d.ingredients
			select ing.ToString()).ToCommaList(false));
			expr_4F[3] = new TableDataGetter<RecipeDef>("skillRequirements", delegate(RecipeDef d)
			{
				string arg_43_0;
				if (d.skillRequirements == null)
				{
					arg_43_0 = "-";
				}
				else
				{
					arg_43_0 = (from ing in d.skillRequirements
					select ing.ToString()).ToCommaList(false);
				}
				return arg_43_0;
			});
			expr_4F[4] = new TableDataGetter<RecipeDef>("surgerySuccessChanceFactor", (RecipeDef d) => d.surgerySuccessChanceFactor.ToStringPercent());
			expr_4F[5] = new TableDataGetter<RecipeDef>("deathOnFailChance", (RecipeDef d) => d.deathOnFailedSurgeryChance.ToStringPercent());
			DebugTables.MakeTablesDialog<RecipeDef>(arg_14B_0, expr_4F);
		}

		[DebugOutput]
		public static void HitsToKill()
		{
			Dictionary<ThingDef, <>__AnonType1<ThingDef, float, int>> data = (from d in DefDatabase<ThingDef>.AllDefs
			where d.race != null
			select d).Select(delegate(ThingDef x)
			{
				int num = 0;
				int num2 = 0;
				for (int i = 0; i < 15; i++)
				{
					PawnGenerationRequest request = new PawnGenerationRequest(x.race.AnyPawnKind, null, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null, null);
					Pawn pawn = PawnGenerator.GeneratePawn(request);
					for (int j = 0; j < 1000; j++)
					{
						pawn.TakeDamage(new DamageInfo(DamageDefOf.Crush, 10f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
						if (pawn.Destroyed)
						{
							num += j + 1;
							break;
						}
					}
					if (!pawn.Destroyed)
					{
						Log.Error("Could not kill pawn " + pawn.ToStringSafe<Pawn>(), false);
					}
					if (pawn.health.ShouldBeDeadFromLethalDamageThreshold())
					{
						num2++;
					}
					if (Find.WorldPawns.Contains(pawn))
					{
						Find.WorldPawns.RemovePawn(pawn);
					}
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
				float hits = (float)num / 15f;
				return new
				{
					Race = x,
					Hits = hits,
					DiedDueToDamageThreshold = num2
				};
			}).ToDictionary(x => x.Race);
			IEnumerable<ThingDef> arg_14C_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.race != null
			orderby d.race.baseHealthScale descending
			select d;
			TableDataGetter<ThingDef>[] expr_C6 = new TableDataGetter<ThingDef>[4];
			expr_C6[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_C6[1] = new TableDataGetter<ThingDef>("10 damage hits", (ThingDef d) => data[d].Hits.ToString("F0"));
			expr_C6[2] = new TableDataGetter<ThingDef>("died due to\ndam. thresh.", (ThingDef d) => data[d].DiedDueToDamageThreshold + "/" + 15);
			expr_C6[3] = new TableDataGetter<ThingDef>("mech", (ThingDef d) => (!d.race.IsMechanoid) ? string.Empty : "mech");
			DebugTables.MakeTablesDialog<ThingDef>(arg_14C_0, expr_C6);
		}

		[DebugOutput]
		public static void Terrains()
		{
			IEnumerable<TerrainDef> arg_2B3_0 = DefDatabase<TerrainDef>.AllDefs;
			TableDataGetter<TerrainDef>[] expr_0C = new TableDataGetter<TerrainDef>[16];
			expr_0C[0] = new TableDataGetter<TerrainDef>("defName", (TerrainDef d) => d.defName);
			expr_0C[1] = new TableDataGetter<TerrainDef>("work", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.WorkToBuild, null).ToString());
			expr_0C[2] = new TableDataGetter<TerrainDef>("beauty", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Beauty, null).ToString());
			expr_0C[3] = new TableDataGetter<TerrainDef>("cleanliness", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Cleanliness, null).ToString());
			expr_0C[4] = new TableDataGetter<TerrainDef>("path\ncost", (TerrainDef d) => d.pathCost.ToString());
			expr_0C[5] = new TableDataGetter<TerrainDef>("fertility", (TerrainDef d) => d.fertility.ToStringPercentEmptyZero("F0"));
			expr_0C[6] = new TableDataGetter<TerrainDef>("accept\nfilth", (TerrainDef d) => d.acceptFilth.ToStringCheckBlank());
			expr_0C[7] = new TableDataGetter<TerrainDef>("accept terrain\nsource filth", (TerrainDef d) => d.acceptTerrainSourceFilth.ToStringCheckBlank());
			expr_0C[8] = new TableDataGetter<TerrainDef>("generated\nfilth", (TerrainDef d) => (d.generatedFilth == null) ? string.Empty : d.generatedFilth.defName);
			expr_0C[9] = new TableDataGetter<TerrainDef>("hold\nsnow", (TerrainDef d) => d.holdSnow.ToStringCheckBlank());
			expr_0C[10] = new TableDataGetter<TerrainDef>("take\nfootprints", (TerrainDef d) => d.takeFootprints.ToStringCheckBlank());
			expr_0C[11] = new TableDataGetter<TerrainDef>("avoid\nwander", (TerrainDef d) => d.avoidWander.ToStringCheckBlank());
			expr_0C[12] = new TableDataGetter<TerrainDef>("buildable", (TerrainDef d) => d.BuildableByPlayer.ToStringCheckBlank());
			expr_0C[13] = new TableDataGetter<TerrainDef>("cost\nlist", (TerrainDef d) => DebugOutputsEconomy.CostListString(d, false, false));
			expr_0C[14] = new TableDataGetter<TerrainDef>("research", delegate(TerrainDef d)
			{
				string arg_43_0;
				if (d.researchPrerequisites != null)
				{
					arg_43_0 = (from pr in d.researchPrerequisites
					select pr.defName).ToCommaList(false);
				}
				else
				{
					arg_43_0 = string.Empty;
				}
				return arg_43_0;
			});
			expr_0C[15] = new TableDataGetter<TerrainDef>("affordances", (TerrainDef d) => (from af in d.affordances
			select af.defName).ToCommaList(false));
			DebugTables.MakeTablesDialog<TerrainDef>(arg_2B3_0, expr_0C);
		}

		[DebugOutput]
		public static void TerrainAffordances()
		{
			IEnumerable<BuildableDef> arg_BF_0 = (from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Building && !d.IsFrame
			select d).Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>());
			TableDataGetter<BuildableDef>[] expr_41 = new TableDataGetter<BuildableDef>[3];
			expr_41[0] = new TableDataGetter<BuildableDef>("type", (BuildableDef d) => (!(d is TerrainDef)) ? "building" : "terrain");
			expr_41[1] = new TableDataGetter<BuildableDef>("defName", (BuildableDef d) => d.defName);
			expr_41[2] = new TableDataGetter<BuildableDef>("terrain\naffordance\nneeded", (BuildableDef d) => (d.terrainAffordanceNeeded == null) ? string.Empty : d.terrainAffordanceNeeded.defName);
			DebugTables.MakeTablesDialog<BuildableDef>(arg_BF_0, expr_41);
		}

		[DebugOutput]
		public static void MentalBreaks()
		{
			IEnumerable<MentalBreakDef> arg_220_0 = from d in DefDatabase<MentalBreakDef>.AllDefs
			orderby d.intensity, d.defName
			select d;
			TableDataGetter<MentalBreakDef>[] expr_50 = new TableDataGetter<MentalBreakDef>[11];
			expr_50[0] = new TableDataGetter<MentalBreakDef>("defName", (MentalBreakDef d) => d.defName);
			expr_50[1] = new TableDataGetter<MentalBreakDef>("intensity", (MentalBreakDef d) => d.intensity.ToString());
			expr_50[2] = new TableDataGetter<MentalBreakDef>("chance in intensity", (MentalBreakDef d) => (d.baseCommonality / (from x in DefDatabase<MentalBreakDef>.AllDefs
			where x.intensity == d.intensity
			select x).Sum((MentalBreakDef x) => x.baseCommonality)).ToStringPercent());
			expr_50[3] = new TableDataGetter<MentalBreakDef>("min duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? ((float)d.mentalState.minTicksBeforeRecovery / 60000f).ToString("0.##") : string.Empty);
			expr_50[4] = new TableDataGetter<MentalBreakDef>("avg duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? (Mathf.Min((float)d.mentalState.minTicksBeforeRecovery + d.mentalState.recoveryMtbDays * 60000f, (float)d.mentalState.maxTicksBeforeRecovery) / 60000f).ToString("0.##") : string.Empty);
			expr_50[5] = new TableDataGetter<MentalBreakDef>("max duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? ((float)d.mentalState.maxTicksBeforeRecovery / 60000f).ToString("0.##") : string.Empty);
			expr_50[6] = new TableDataGetter<MentalBreakDef>("recoverFromSleep", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.recoverFromSleep) ? string.Empty : "recoverFromSleep");
			expr_50[7] = new TableDataGetter<MentalBreakDef>("recoveryThought", (MentalBreakDef d) => (d.mentalState != null) ? d.mentalState.moodRecoveryThought.ToStringSafe<ThoughtDef>() : string.Empty);
			expr_50[8] = new TableDataGetter<MentalBreakDef>("category", (MentalBreakDef d) => (d.mentalState == null) ? string.Empty : d.mentalState.category.ToString());
			expr_50[9] = new TableDataGetter<MentalBreakDef>("blockNormalThoughts", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.blockNormalThoughts) ? string.Empty : "blockNormalThoughts");
			expr_50[10] = new TableDataGetter<MentalBreakDef>("allowBeatfire", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.allowBeatfire) ? string.Empty : "allowBeatfire");
			DebugTables.MakeTablesDialog<MentalBreakDef>(arg_220_0, expr_50);
		}

		[DebugOutput]
		public static void TraitsSampled()
		{
			List<Pawn> testColonists = new List<Pawn>();
			for (int i = 0; i < 4000; i++)
			{
				testColonists.Add(PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer));
			}
			Func<TraitDegreeData, TraitDef> getTrait = (TraitDegreeData d) => DefDatabase<TraitDef>.AllDefs.First((TraitDef td) => td.degreeDatas.Contains(d));
			Func<TraitDegreeData, float> getPrevalence = delegate(TraitDegreeData d)
			{
				float num = 0f;
				foreach (Pawn current in testColonists)
				{
					Trait trait = current.story.traits.GetTrait(getTrait(d));
					if (trait != null && trait.Degree == d.degree)
					{
						num += 1f;
					}
				}
				return num / 4000f;
			};
			IEnumerable<TraitDegreeData> arg_1A1_0 = DefDatabase<TraitDef>.AllDefs.SelectMany((TraitDef tr) => tr.degreeDatas);
			TableDataGetter<TraitDegreeData>[] expr_A3 = new TableDataGetter<TraitDegreeData>[8];
			expr_A3[0] = new TableDataGetter<TraitDegreeData>("trait", (TraitDegreeData d) => getTrait(d).defName);
			expr_A3[1] = new TableDataGetter<TraitDegreeData>("trait commonality", (TraitDegreeData d) => getTrait(d).GetGenderSpecificCommonality(Gender.None).ToString("F2"));
			expr_A3[2] = new TableDataGetter<TraitDegreeData>("trait commonalityFemale", (TraitDegreeData d) => getTrait(d).GetGenderSpecificCommonality(Gender.Female).ToString("F2"));
			expr_A3[3] = new TableDataGetter<TraitDegreeData>("degree", (TraitDegreeData d) => d.label);
			expr_A3[4] = new TableDataGetter<TraitDegreeData>("degree num", (TraitDegreeData d) => (getTrait(d).degreeDatas.Count <= 0) ? string.Empty : d.degree.ToString());
			expr_A3[5] = new TableDataGetter<TraitDegreeData>("degree commonality", (TraitDegreeData d) => (getTrait(d).degreeDatas.Count <= 0) ? string.Empty : d.commonality.ToString("F2"));
			expr_A3[6] = new TableDataGetter<TraitDegreeData>("marketValueFactorOffset", (TraitDegreeData d) => d.marketValueFactorOffset.ToString("F0"));
			expr_A3[7] = new TableDataGetter<TraitDegreeData>("prevalence among " + 4000 + "\ngenerated Colonists", (TraitDegreeData d) => getPrevalence(d).ToStringPercent());
			DebugTables.MakeTablesDialog<TraitDegreeData>(arg_1A1_0, expr_A3);
		}

		[DebugOutput]
		public static void BestThingRequestGroup()
		{
			IEnumerable<ThingDef> arg_CD_0 = from x in DefDatabase<ThingDef>.AllDefs
			where ListerThings.EverListable(x, ListerThingsUse.Global) || ListerThings.EverListable(x, ListerThingsUse.Region)
			orderby x.label
			select x;
			TableDataGetter<ThingDef>[] expr_4F = new TableDataGetter<ThingDef>[3];
			expr_4F[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_4F[1] = new TableDataGetter<ThingDef>("best local", delegate(ThingDef d)
			{
				IEnumerable<ThingRequestGroup> source;
				if (!ListerThings.EverListable(d, ListerThingsUse.Region))
				{
					source = Enumerable.Empty<ThingRequestGroup>();
				}
				else
				{
					source = from x in (ThingRequestGroup[])Enum.GetValues(typeof(ThingRequestGroup))
					where x.StoreInRegion() && x.Includes(d)
					select x;
				}
				if (!source.Any<ThingRequestGroup>())
				{
					return "-";
				}
				ThingRequestGroup best = source.MinBy((ThingRequestGroup x) => DefDatabase<ThingDef>.AllDefs.Count((ThingDef y) => ListerThings.EverListable(y, ListerThingsUse.Region) && x.Includes(y)));
				return string.Concat(new object[]
				{
					best,
					" (defs: ",
					DefDatabase<ThingDef>.AllDefs.Count((ThingDef x) => ListerThings.EverListable(x, ListerThingsUse.Region) && best.Includes(x)),
					")"
				});
			});
			expr_4F[2] = new TableDataGetter<ThingDef>("best global", delegate(ThingDef d)
			{
				IEnumerable<ThingRequestGroup> source;
				if (!ListerThings.EverListable(d, ListerThingsUse.Global))
				{
					source = Enumerable.Empty<ThingRequestGroup>();
				}
				else
				{
					source = from x in (ThingRequestGroup[])Enum.GetValues(typeof(ThingRequestGroup))
					where x.Includes(d)
					select x;
				}
				if (!source.Any<ThingRequestGroup>())
				{
					return "-";
				}
				ThingRequestGroup best = source.MinBy((ThingRequestGroup x) => DefDatabase<ThingDef>.AllDefs.Count((ThingDef y) => ListerThings.EverListable(y, ListerThingsUse.Global) && x.Includes(y)));
				return string.Concat(new object[]
				{
					best,
					" (defs: ",
					DefDatabase<ThingDef>.AllDefs.Count((ThingDef x) => ListerThings.EverListable(x, ListerThingsUse.Global) && best.Includes(x)),
					")"
				});
			});
			DebugTables.MakeTablesDialog<ThingDef>(arg_CD_0, expr_4F);
		}

		[DebugOutput]
		public static void Prosthetics()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, false, false, false, (Pawn p) => p.health.hediffSet.hediffs.Count == 0, null, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			Action refreshPawn = delegate
			{
				while (pawn.health.hediffSet.hediffs.Count > 0)
				{
					pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs[0]);
				}
			};
			Func<RecipeDef, BodyPartRecord> getApplicationPoint = (RecipeDef recipe) => recipe.appliedOnFixedBodyParts.SelectMany((BodyPartDef bpd) => pawn.def.race.body.GetPartsWithDef(bpd)).FirstOrDefault<BodyPartRecord>();
			Func<RecipeDef, ThingDef> getProstheticItem = (RecipeDef recipe) => (from ic in recipe.ingredients
			select ic.filter.AnyAllowedDef).FirstOrDefault((ThingDef td) => !td.IsMedicine);
			List<TableDataGetter<RecipeDef>> list = new List<TableDataGetter<RecipeDef>>();
			list.Add(new TableDataGetter<RecipeDef>("defName", (RecipeDef r) => r.defName));
			list.Add(new TableDataGetter<RecipeDef>("price", delegate(RecipeDef r)
			{
				ThingDef thingDef = getProstheticItem(r);
				return (thingDef == null) ? 0f : thingDef.BaseMarketValue;
			}));
			list.Add(new TableDataGetter<RecipeDef>("install time", (RecipeDef r) => r.workAmount));
			list.Add(new TableDataGetter<RecipeDef>("install total cost", delegate(RecipeDef r)
			{
				float num = r.ingredients.Sum((IngredientCount ic) => ic.filter.AnyAllowedDef.BaseMarketValue * ic.GetBaseCount());
				float num2 = r.workAmount * 0.0036f;
				return num + num2;
			}));
			list.Add(new TableDataGetter<RecipeDef>("install skill", (RecipeDef r) => (from sr in r.skillRequirements
			select sr.minLevel).Max()));
			foreach (PawnCapacityDef cap in from pc in DefDatabase<PawnCapacityDef>.AllDefs
			orderby pc.listOrder
			select pc)
			{
				list.Add(new TableDataGetter<RecipeDef>(cap.defName, delegate(RecipeDef r)
				{
					refreshPawn();
					r.Worker.ApplyOnPawn(pawn, getApplicationPoint(r), null, null, null);
					float num = pawn.health.capacities.GetLevel(cap) - 1f;
					if ((double)Math.Abs(num) > 0.001)
					{
						return num.ToStringPercent();
					}
					refreshPawn();
					BodyPartRecord bodyPartRecord = getApplicationPoint(r);
					Thing arg_F0_0 = pawn;
					DamageDef executionCut = DamageDefOf.ExecutionCut;
					float amount = pawn.health.hediffSet.GetPartHealth(bodyPartRecord) / 2f;
					float armorPenetration = 999f;
					BodyPartRecord hitPart = bodyPartRecord;
					arg_F0_0.TakeDamage(new DamageInfo(executionCut, amount, armorPenetration, -1f, null, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
					List<PawnCapacityUtility.CapacityImpactor> list2 = new List<PawnCapacityUtility.CapacityImpactor>();
					PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, cap, list2);
					if (list2.Any((PawnCapacityUtility.CapacityImpactor imp) => imp.IsDirect))
					{
						return 0f.ToStringPercent();
					}
					return string.Empty;
				}));
			}
			list.Add(new TableDataGetter<RecipeDef>("tech level", (RecipeDef r) => (getProstheticItem(r) != null) ? getProstheticItem(r).techLevel.ToStringHuman() : string.Empty));
			list.Add(new TableDataGetter<RecipeDef>("thingSetMakerTags", (RecipeDef r) => (getProstheticItem(r) != null) ? getProstheticItem(r).thingSetMakerTags.ToCommaList(false) : string.Empty));
			list.Add(new TableDataGetter<RecipeDef>("techHediffsTags", (RecipeDef r) => (getProstheticItem(r) != null) ? getProstheticItem(r).techHediffsTags.ToCommaList(false) : string.Empty));
			DebugTables.MakeTablesDialog<RecipeDef>(from r in ThingDefOf.Human.AllRecipes
			where r.workerClass == typeof(Recipe_InstallArtificialBodyPart) || r.workerClass == typeof(Recipe_InstallNaturalBodyPart)
			select r, list.ToArray());
			Messages.Clear();
		}

		[DebugOutput]
		public static void JoyGivers()
		{
			IEnumerable<JoyGiverDef> arg_1DC_0 = DefDatabase<JoyGiverDef>.AllDefs;
			TableDataGetter<JoyGiverDef>[] expr_0C = new TableDataGetter<JoyGiverDef>[11];
			expr_0C[0] = new TableDataGetter<JoyGiverDef>("defName", (JoyGiverDef d) => d.defName);
			expr_0C[1] = new TableDataGetter<JoyGiverDef>("joyKind", (JoyGiverDef d) => (d.joyKind != null) ? d.joyKind.defName : "null");
			expr_0C[2] = new TableDataGetter<JoyGiverDef>("baseChance", (JoyGiverDef d) => d.baseChance.ToString());
			expr_0C[3] = new TableDataGetter<JoyGiverDef>("canDoWhileInBed", (JoyGiverDef d) => d.canDoWhileInBed.ToStringCheckBlank());
			expr_0C[4] = new TableDataGetter<JoyGiverDef>("desireSit", (JoyGiverDef d) => d.desireSit.ToStringCheckBlank());
			expr_0C[5] = new TableDataGetter<JoyGiverDef>("unroofedOnly", (JoyGiverDef d) => d.unroofedOnly.ToStringCheckBlank());
			expr_0C[6] = new TableDataGetter<JoyGiverDef>("jobDef", (JoyGiverDef d) => (d.jobDef != null) ? d.jobDef.defName : "null");
			expr_0C[7] = new TableDataGetter<JoyGiverDef>("pctPawnsEverDo", (JoyGiverDef d) => d.pctPawnsEverDo.ToStringPercent());
			expr_0C[8] = new TableDataGetter<JoyGiverDef>("requiredCapacities", delegate(JoyGiverDef d)
			{
				string arg_43_0;
				if (d.requiredCapacities == null)
				{
					arg_43_0 = string.Empty;
				}
				else
				{
					arg_43_0 = (from c in d.requiredCapacities
					select c.defName).ToCommaList(false);
				}
				return arg_43_0;
			});
			expr_0C[9] = new TableDataGetter<JoyGiverDef>("thingDefs", delegate(JoyGiverDef d)
			{
				string arg_43_0;
				if (d.thingDefs == null)
				{
					arg_43_0 = string.Empty;
				}
				else
				{
					arg_43_0 = (from c in d.thingDefs
					select c.defName).ToCommaList(false);
				}
				return arg_43_0;
			});
			expr_0C[10] = new TableDataGetter<JoyGiverDef>("JoyGainFactors", delegate(JoyGiverDef d)
			{
				string arg_43_0;
				if (d.thingDefs == null)
				{
					arg_43_0 = string.Empty;
				}
				else
				{
					arg_43_0 = (from c in d.thingDefs
					select c.GetStatValueAbstract(StatDefOf.JoyGainFactor, null).ToString("F2")).ToCommaList(false);
				}
				return arg_43_0;
			});
			DebugTables.MakeTablesDialog<JoyGiverDef>(arg_1DC_0, expr_0C);
		}

		[DebugOutput]
		public static void JoyJobs()
		{
			IEnumerable<JobDef> arg_153_0 = from j in DefDatabase<JobDef>.AllDefs
			where j.joyKind != null
			select j;
			TableDataGetter<JobDef>[] expr_2D = new TableDataGetter<JobDef>[7];
			expr_2D[0] = new TableDataGetter<JobDef>("defName", (JobDef d) => d.defName);
			expr_2D[1] = new TableDataGetter<JobDef>("joyKind", (JobDef d) => d.joyKind.defName);
			expr_2D[2] = new TableDataGetter<JobDef>("joyDuration", (JobDef d) => d.joyDuration.ToString());
			expr_2D[3] = new TableDataGetter<JobDef>("joyGainRate", (JobDef d) => d.joyGainRate.ToString());
			expr_2D[4] = new TableDataGetter<JobDef>("joyMaxParticipants", (JobDef d) => d.joyMaxParticipants.ToString());
			expr_2D[5] = new TableDataGetter<JobDef>("joySkill", (JobDef d) => (d.joySkill == null) ? string.Empty : d.joySkill.defName);
			expr_2D[6] = new TableDataGetter<JobDef>("joyXpPerTick", (JobDef d) => d.joyXpPerTick.ToString());
			DebugTables.MakeTablesDialog<JobDef>(arg_153_0, expr_2D);
		}

		[DebugOutput]
		public static void Thoughts()
		{
			Func<ThoughtDef, string> stagesText = delegate(ThoughtDef t)
			{
				string text = string.Empty;
				if (t.stages == null)
				{
					return null;
				}
				for (int i = 0; i < t.stages.Count; i++)
				{
					ThoughtStage thoughtStage = t.stages[i];
					string text2 = text;
					text = string.Concat(new object[]
					{
						text2,
						"[",
						i,
						"] "
					});
					if (thoughtStage == null)
					{
						text += "null";
					}
					else
					{
						if (thoughtStage.label != null)
						{
							text += thoughtStage.label;
						}
						if (thoughtStage.labelSocial != null)
						{
							if (thoughtStage.label != null)
							{
								text += "/";
							}
							text += thoughtStage.labelSocial;
						}
						text += " ";
						if (thoughtStage.baseMoodEffect != 0f)
						{
							text = text + "[" + thoughtStage.baseMoodEffect.ToStringWithSign("0.##") + " Mo]";
						}
						if (thoughtStage.baseOpinionOffset != 0f)
						{
							text = text + "(" + thoughtStage.baseOpinionOffset.ToStringWithSign("0.##") + " Op)";
						}
					}
					if (i < t.stages.Count - 1)
					{
						text += "\n";
					}
				}
				return text;
			};
			IEnumerable<ThoughtDef> arg_321_0 = DefDatabase<ThoughtDef>.AllDefs;
			TableDataGetter<ThoughtDef>[] expr_35 = new TableDataGetter<ThoughtDef>[18];
			expr_35[0] = new TableDataGetter<ThoughtDef>("defName", (ThoughtDef d) => d.defName);
			expr_35[1] = new TableDataGetter<ThoughtDef>("type", (ThoughtDef d) => (!d.IsMemory) ? "situ" : "mem");
			expr_35[2] = new TableDataGetter<ThoughtDef>("social", (ThoughtDef d) => (!d.IsSocial) ? "mood" : "soc");
			expr_35[3] = new TableDataGetter<ThoughtDef>("stages", (ThoughtDef d) => stagesText(d));
			expr_35[4] = new TableDataGetter<ThoughtDef>("best\nmood", (ThoughtDef d) => (from st in d.stages
			where st != null
			select st).Max((ThoughtStage st) => st.baseMoodEffect));
			expr_35[5] = new TableDataGetter<ThoughtDef>("worst\nmood", (ThoughtDef d) => (from st in d.stages
			where st != null
			select st).Min((ThoughtStage st) => st.baseMoodEffect));
			expr_35[6] = new TableDataGetter<ThoughtDef>("stack\nlimit", (ThoughtDef d) => d.stackLimit.ToString());
			expr_35[7] = new TableDataGetter<ThoughtDef>("stack\nlimit\nper o. pawn", (ThoughtDef d) => (d.stackLimitForSameOtherPawn >= 0) ? d.stackLimitForSameOtherPawn.ToString() : string.Empty);
			expr_35[8] = new TableDataGetter<ThoughtDef>("stacked\neffect\nmultiplier", (ThoughtDef d) => (d.stackLimit != 1) ? d.stackedEffectMultiplier.ToStringPercent() : string.Empty);
			expr_35[9] = new TableDataGetter<ThoughtDef>("duration\n(days)", (ThoughtDef d) => d.durationDays.ToString());
			expr_35[10] = new TableDataGetter<ThoughtDef>("effect\nmultiplying\nstat", (ThoughtDef d) => (d.effectMultiplyingStat != null) ? d.effectMultiplyingStat.defName : string.Empty);
			expr_35[11] = new TableDataGetter<ThoughtDef>("game\ncondition", (ThoughtDef d) => (d.gameCondition != null) ? d.gameCondition.defName : string.Empty);
			expr_35[12] = new TableDataGetter<ThoughtDef>("hediff", (ThoughtDef d) => (d.hediff != null) ? d.hediff.defName : string.Empty);
			expr_35[13] = new TableDataGetter<ThoughtDef>("lerp opinion\nto zero\nafter duration pct", (ThoughtDef d) => d.lerpOpinionToZeroAfterDurationPct.ToStringPercent());
			expr_35[14] = new TableDataGetter<ThoughtDef>("max cumulated\nopinion\noffset", (ThoughtDef d) => (d.maxCumulatedOpinionOffset <= 99999f) ? d.maxCumulatedOpinionOffset.ToString() : string.Empty);
			expr_35[15] = new TableDataGetter<ThoughtDef>("next\nthought", (ThoughtDef d) => (d.nextThought != null) ? d.nextThought.defName : string.Empty);
			expr_35[16] = new TableDataGetter<ThoughtDef>("nullified\nif not colonist", (ThoughtDef d) => d.nullifiedIfNotColonist.ToStringCheckBlank());
			expr_35[17] = new TableDataGetter<ThoughtDef>("show\nbubble", (ThoughtDef d) => d.showBubble.ToStringCheckBlank());
			DebugTables.MakeTablesDialog<ThoughtDef>(arg_321_0, expr_35);
		}

		[DebugOutput]
		public static void GenSteps()
		{
			IEnumerable<GenStepDef> arg_F7_0 = from x in DefDatabase<GenStepDef>.AllDefsListForReading
			orderby x.order, x.index
			select x;
			TableDataGetter<GenStepDef>[] expr_4F = new TableDataGetter<GenStepDef>[4];
			expr_4F[0] = new TableDataGetter<GenStepDef>("defName", (GenStepDef x) => x.defName);
			expr_4F[1] = new TableDataGetter<GenStepDef>("order", (GenStepDef x) => x.order.ToString("0.##"));
			expr_4F[2] = new TableDataGetter<GenStepDef>("class", (GenStepDef x) => x.genStep.GetType().Name);
			expr_4F[3] = new TableDataGetter<GenStepDef>("site", (GenStepDef x) => (x.linkWithSite == null) ? string.Empty : x.linkWithSite.defName);
			DebugTables.MakeTablesDialog<GenStepDef>(arg_F7_0, expr_4F);
		}

		[DebugOutput]
		public static void WorldGenSteps()
		{
			IEnumerable<WorldGenStepDef> arg_CD_0 = from x in DefDatabase<WorldGenStepDef>.AllDefsListForReading
			orderby x.order, x.index
			select x;
			TableDataGetter<WorldGenStepDef>[] expr_4F = new TableDataGetter<WorldGenStepDef>[3];
			expr_4F[0] = new TableDataGetter<WorldGenStepDef>("defName", (WorldGenStepDef x) => x.defName);
			expr_4F[1] = new TableDataGetter<WorldGenStepDef>("order", (WorldGenStepDef x) => x.order.ToString("0.##"));
			expr_4F[2] = new TableDataGetter<WorldGenStepDef>("class", (WorldGenStepDef x) => x.worldGenStep.GetType().Name);
			DebugTables.MakeTablesDialog<WorldGenStepDef>(arg_CD_0, expr_4F);
		}
	}
}
