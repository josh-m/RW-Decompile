using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using Verse.AI;
using Verse.Profile;
using Verse.Steam;

namespace Verse
{
	internal static class DataAnalysisLogger
	{
		public static void DoLog_QualityGenData()
		{
			QualityUtility.LogGenerationData();
		}

		public static void DoLog_SongSelectionInfo()
		{
			Find.MusicManagerPlay.LogSongSelectionData();
		}

		public static void DoLog_PlantData()
		{
			GenPlant.LogPlantData();
		}

		public static void DoLog_AgeInjuries()
		{
			AgeInjuryUtility.LogOldInjuryCalculations();
		}

		public static void DoLog_PawnGroupsMade()
		{
			PawnGroupMakerUtility.LogPawnGroupsMade();
		}

		public static void DoLog_AllGraphicsInDatabase()
		{
			GraphicDatabase.DebugLogAllGraphics();
		}

		public static void DoLog_RandTests()
		{
			Rand.LogRandTests();
		}

		public static void DoLog_SteamWorkshopStatus()
		{
			Workshop.LogStatus();
		}

		public static void DoLog_MathPerf()
		{
			GenMath.LogTestMathPerf();
		}

		public static void DoLog_MeshPoolStats()
		{
			MeshPool.LogStats();
		}

		public static void DoLog_Lords()
		{
			Find.VisibleMap.lordManager.LogLords();
		}

		[Category("Incident")]
		public static void DoLog_PodContentsTest()
		{
			ItemCollectionGenerator_ResourcePod.DebugLogPodContentsChoices();
		}

		[Category("Incident")]
		public static void DoLog_PodContentsPossible()
		{
			ItemCollectionGenerator_ResourcePod.DebugLogPossiblePodContentsDefs();
		}

		public static void DoLog_PathCostIgnoreRepeaters()
		{
			PathGrid.LogPathCostIgnoreRepeaters();
		}

		public static void DoLog_AnimalStockGen()
		{
			StockGenerator_Animals.LogStockGeneration();
		}

		[Category("Memory")]
		public static void DoLog_ObjectsLoaded()
		{
			MemoryTracker.LogObjectsLoaded();
		}

		[Category("Memory")]
		public static void DoLog_ObjectHoldPaths()
		{
			MemoryTracker.LogObjectHoldPaths();
		}

		public static void DoLog_GrownCollections_Start()
		{
			CollectionsTracker.RememberCollections();
		}

		public static void DoLog_GrownCollections_End()
		{
			CollectionsTracker.LogGrownCollections();
		}

		[Category("Incident")]
		public static void DoLog_PeaceTalksChances()
		{
			PeaceTalks.LogChances();
		}

		public static void DoLog_DamageTest()
		{
			ThingDef thingDef = ThingDef.Named("Bullet_BoltActionRifle");
			PawnKindDef pawnKindDef = PawnKindDef.Named("Slave");
			Faction faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionType);
			DamageInfo dinfo = new DamageInfo(thingDef.projectile.damageDef, thingDef.projectile.damageAmountBase, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
			int num = 0;
			int num2 = 0;
			DefMap<BodyPartDef, int> defMap = new DefMap<BodyPartDef, int>();
			for (int i = 0; i < 500; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
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
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Damage test");
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"Hit ",
				500,
				" ",
				pawnKindDef.label,
				"s with ",
				2,
				"x ",
				thingDef.label,
				" (",
				thingDef.projectile.damageAmountBase,
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
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_ListSolidBackstories()
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
					Log.Message(stringBuilder.ToString());
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DoLog_WorkDisables()
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
						faction
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
					Log.Message(stringBuilder.ToString());
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DoLog_KeyStrings()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
			{
				stringBuilder.AppendLine(k.ToString() + " - " + k.ToStringReadable());
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_PawnKindGear()
		{
			IOrderedEnumerable<PawnKindDef> orderedEnumerable = from k in DefDatabase<PawnKindDef>.AllDefs
			where k.RaceProps.Humanlike
			orderby k.combatPower
			select k;
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (PawnKindDef current in orderedEnumerable)
			{
				Faction fac = FactionUtility.DefaultFactionFrom(current.defaultFactionType);
				PawnKindDef kind = current;
				FloatMenuOption item = new FloatMenuOption(string.Concat(new object[]
				{
					kind.defName,
					"(",
					kind.combatPower,
					", $",
					kind.weaponMoney,
					")"
				}), delegate
				{
					DefMap<ThingDef, int> weapons = new DefMap<ThingDef, int>();
					for (int i = 0; i < 500; i++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(kind, fac);
						if (pawn.equipment.Primary != null)
						{
							DefMap<ThingDef, int> weapons2;
							ThingDef def;
							(weapons2 = weapons)[def = pawn.equipment.Primary.def] = weapons2[def] + 1;
						}
						pawn.Destroy(DestroyMode.Vanish);
					}
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"Weapons spawned from ",
						500,
						"x ",
						kind.defName
					}));
					foreach (ThingDef current2 in from t in DefDatabase<ThingDef>.AllDefs
					orderby weapons[t] descending
					select t)
					{
						int num = weapons[current2];
						if (num > 0)
						{
							stringBuilder.AppendLine("  " + current2.defName + "    " + ((float)num / 500f).ToStringPercent());
						}
					}
					Log.Message(stringBuilder.ToString().TrimEndNewlines());
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DoLog_RecipeSkills()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Recipes per skill, with work speed stats:");
			stringBuilder.AppendLine("(No skill)");
			foreach (RecipeDef current in DefDatabase<RecipeDef>.AllDefs)
			{
				if (current.workSkill == null)
				{
					stringBuilder.Append("    " + current.defName);
					if (current.workSpeedStat != null)
					{
						stringBuilder.Append(" (" + current.workSpeedStat + ")");
					}
					stringBuilder.AppendLine();
				}
			}
			stringBuilder.AppendLine();
			foreach (SkillDef current2 in DefDatabase<SkillDef>.AllDefs)
			{
				stringBuilder.AppendLine(current2.LabelCap);
				foreach (RecipeDef current3 in DefDatabase<RecipeDef>.AllDefs)
				{
					if (current3.workSkill == current2)
					{
						stringBuilder.Append("    " + current3.defName);
						if (current3.workSpeedStat != null)
						{
							stringBuilder.Append(" (" + current3.workSpeedStat);
							if (!current3.workSpeedStat.skillNeedFactors.NullOrEmpty<SkillNeed>())
							{
								stringBuilder.Append(" - " + GenText.ToCommaList(from fac in current3.workSpeedStat.skillNeedFactors
								select fac.skill.defName, false));
							}
							stringBuilder.Append(")");
						}
						stringBuilder.AppendLine();
					}
				}
				stringBuilder.AppendLine();
			}
			Log.Message(stringBuilder.ToString().TrimEndNewlines());
		}

		[Category("Incident")]
		public static void DoLog_RaidStrategies()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Raid strategy chances:");
			float num = (from d in DefDatabase<RaidStrategyDef>.AllDefs
			select d.Worker.SelectionChance(Find.VisibleMap)).Sum();
			foreach (RaidStrategyDef current in DefDatabase<RaidStrategyDef>.AllDefs)
			{
				float num2 = current.Worker.SelectionChance(Find.VisibleMap);
				stringBuilder.AppendLine(string.Concat(new string[]
				{
					current.defName,
					": ",
					num2.ToString("F2"),
					" (",
					(num2 / num).ToStringPercent(),
					")"
				}));
			}
			Log.Message(stringBuilder.ToString());
		}

		[Category("Incident")]
		public static void DoLog_StockGeneratorsDefs()
		{
			if (Find.VisibleMap == null)
			{
				Log.Error("Requires visible map.");
				return;
			}
			StringBuilder sb = new StringBuilder();
			Action<StockGenerator> action = delegate(StockGenerator gen)
			{
				sb.AppendLine(gen.GetType().ToString());
				sb.AppendLine("ALLOWED DEFS:");
				foreach (ThingDef current in from d in DefDatabase<ThingDef>.AllDefs
				where gen.HandlesThingDef(d)
				select d)
				{
					sb.AppendLine(string.Concat(new object[]
					{
						current.defName,
						" [",
						current.BaseMarketValue,
						"]"
					}));
				}
				sb.AppendLine();
				sb.AppendLine("GENERATION TEST:");
				gen.countRange = IntRange.one;
				for (int i = 0; i < 30; i++)
				{
					foreach (Thing current2 in gen.GenerateThings(Find.VisibleMap.Tile))
					{
						sb.AppendLine(string.Concat(new object[]
						{
							current2.Label,
							" [",
							current2.MarketValue,
							"]"
						}));
					}
				}
				sb.AppendLine("---------------------------------------------------------");
			};
			action(new StockGenerator_Armor());
			action(new StockGenerator_WeaponsRanged());
			action(new StockGenerator_Clothes());
			action(new StockGenerator_Art());
			Log.Message(sb.ToString());
		}

		[Category("Incident")]
		public static void DoLog_TraderStockMarketValues()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (TraderKindDef current in DefDatabase<TraderKindDef>.AllDefs)
			{
				stringBuilder.AppendLine(current.defName + " : " + ((ItemCollectionGenerator_TraderStock)ItemCollectionGeneratorDefOf.TraderStock.Worker).AverageTotalStockValue(current).ToString("F0"));
			}
			Log.Message(stringBuilder.ToString());
		}

		[Category("Incident")]
		public static void DoLog_ItemCollectionGeneration()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ItemCollectionGeneratorDef current in DefDatabase<ItemCollectionGeneratorDef>.AllDefs)
			{
				ItemCollectionGeneratorDef localDef = current;
				DebugMenuOption item = new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
				{
					Action<ItemCollectionGeneratorParams> generate = delegate(ItemCollectionGeneratorParams parms)
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int i = 0; i < 50; i++)
						{
							List<Thing> list3 = localDef.Worker.Generate(parms);
							if (stringBuilder.Length > 0)
							{
								stringBuilder.AppendLine();
							}
							float num = 0f;
							for (int j = 0; j < list3.Count; j++)
							{
								stringBuilder.AppendLine("   - " + list3[j].LabelCap);
								num += list3[j].MarketValue * (float)list3[j].stackCount;
								list3[j].Destroy(DestroyMode.Vanish);
							}
							stringBuilder.AppendLine("Total market value: " + num.ToString("0.##"));
						}
						Log.Message(stringBuilder.ToString());
					};
					if (localDef == ItemCollectionGeneratorDefOf.TraderStock)
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
											ItemCollectionGeneratorParams obj = default(ItemCollectionGeneratorParams);
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
						generate(default(ItemCollectionGeneratorParams));
					}
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[Category("Incident")]
		public static void DoLog_TraderStockGeneration()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (TraderKindDef current in DefDatabase<TraderKindDef>.AllDefs)
			{
				TraderKindDef localDef = current;
				FloatMenuOption item = new FloatMenuOption(localDef.defName, delegate
				{
					Log.Message(((ItemCollectionGenerator_TraderStock)ItemCollectionGeneratorDefOf.TraderStock.Worker).GenerationDataFor(localDef));
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DoLog_BodyPartTagGroups()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (BodyDef current in DefDatabase<BodyDef>.AllDefs)
			{
				BodyDef localBd = current;
				FloatMenuOption item = new FloatMenuOption(localBd.defName, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(localBd.defName + "\n----------------");
					foreach (string tag in (from elem in localBd.AllParts.SelectMany((BodyPartRecord part) => part.def.tags)
					orderby elem
					select elem).Distinct<string>())
					{
						stringBuilder.AppendLine(tag);
						foreach (BodyPartRecord current2 in from part in localBd.AllParts
						where part.def.tags.Contains(tag)
						orderby part.def.defName
						select part)
						{
							stringBuilder.AppendLine("  " + current2.def.defName);
						}
					}
					Log.Message(stringBuilder.ToString());
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[Category("Memory")]
		public static void DoLog_LoadedAssets()
		{
			StringBuilder stringBuilder = new StringBuilder();
			UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(Mesh));
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"Meshes: ",
				array.Length,
				" (",
				DataAnalysisLogger.TotalBytes(array).ToStringBytes("F2"),
				")"
			}));
			UnityEngine.Object[] array2 = Resources.FindObjectsOfTypeAll(typeof(Material));
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"Materials: ",
				array2.Length,
				" (",
				DataAnalysisLogger.TotalBytes(array2).ToStringBytes("F2"),
				")"
			}));
			stringBuilder.AppendLine("   Damaged: " + DamagedMatPool.MatCount);
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"   Faded: ",
				FadedMaterialPool.TotalMaterialCount,
				" (",
				FadedMaterialPool.TotalMaterialBytes.ToStringBytes("F2"),
				")"
			}));
			stringBuilder.AppendLine("   SolidColorsSimple: " + SolidColorMaterials.SimpleColorMatCount);
			UnityEngine.Object[] array3 = Resources.FindObjectsOfTypeAll(typeof(Texture));
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"Textures: ",
				array3.Length,
				" (",
				DataAnalysisLogger.TotalBytes(array3).ToStringBytes("F2"),
				")"
			}));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Texture list:");
			UnityEngine.Object[] array4 = array3;
			for (int i = 0; i < array4.Length; i++)
			{
				UnityEngine.Object @object = array4[i];
				string text = ((Texture)@object).name;
				if (text.NullOrEmpty())
				{
					text = "-";
				}
				stringBuilder.AppendLine(text);
			}
			Log.Message(stringBuilder.ToString());
		}

		[Category("Memory")]
		private static long TotalBytes(UnityEngine.Object[] arr)
		{
			long num = 0L;
			for (int i = 0; i < arr.Length; i++)
			{
				UnityEngine.Object o = arr[i];
				num += Profiler.GetRuntimeMemorySizeLong(o);
			}
			return num;
		}

		public static void DoLog_MinifiableTags()
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
						stringBuilder.Append(GenText.ToCommaList(current.tradeTags, true));
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_ItemBeauties()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef current in from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Item && !d.destroyOnDrop
			orderby d.GetStatValueAbstract(StatDefOf.Beauty, null) descending
			select d)
			{
				stringBuilder.AppendLine(current.defName + "  " + current.GetStatValueAbstract(StatDefOf.Beauty, null).ToString("F1"));
			}
			Log.Message(stringBuilder.ToString());
		}

		[Category("Text")]
		public static void DoLog_TestRulepack()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (RulePackDef current in DefDatabase<RulePackDef>.AllDefs)
			{
				RulePackDef localNamer = current;
				FloatMenuOption item = new FloatMenuOption(localNamer.defName, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 200; i++)
					{
						stringBuilder.AppendLine(NameGenerator.GenerateName(localNamer, null, false, null));
					}
					Log.Message(stringBuilder.ToString());
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[Category("Text")]
		public static void DoLog_GeneratedNames()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (RulePackDef current in DefDatabase<RulePackDef>.AllDefs)
			{
				RulePackDef localRp = current;
				FloatMenuOption item = new FloatMenuOption(localRp.defName, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Test names for " + localRp.defName + ":");
					for (int i = 0; i < 200; i++)
					{
						stringBuilder.AppendLine(NameGenerator.GenerateName(localRp, null, false, null));
					}
					Log.Message(stringBuilder.ToString());
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		private static void CreateDamagedDestroyedMenu(Action<Action<List<BodyPartDef>, List<bool>>> callback)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			IEnumerable<int> damagedes = Enumerable.Range(0, 5);
			IEnumerable<int> destroyedes = Enumerable.Range(0, 5);
			foreach (Pair<int, int> damageddestroyed in damagedes.Concat(-1).Cross(destroyedes.Concat(-1)))
			{
				DebugMenuOption item = new DebugMenuOption(string.Format("{0} damaged/{1} destroyed", (damageddestroyed.First != -1) ? damageddestroyed.First.ToString() : "(random)", (damageddestroyed.Second != -1) ? damageddestroyed.Second.ToString() : "(random)"), DebugMenuOptionMode.Action, delegate
				{
					callback(delegate(List<BodyPartDef> bodyparts, List<bool> flags)
					{
						int num = damageddestroyed.First;
						int destroyed = damageddestroyed.Second;
						if (num == -1)
						{
							num = damagedes.RandomElement<int>();
						}
						if (destroyed == -1)
						{
							destroyed = destroyedes.RandomElement<int>();
						}
						Pair<BodyPartDef, bool>[] source = (from idx in Enumerable.Range(0, num + destroyed)
						select new Pair<BodyPartDef, bool>(DefDatabase<BodyPartDef>.AllDefsListForReading.RandomElement<BodyPartDef>(), idx < destroyed)).InRandomOrder(null).ToArray<Pair<BodyPartDef, bool>>();
						bodyparts.Clear();
						flags.Clear();
						bodyparts.AddRange(from part in source
						select part.First);
						flags.AddRange(from part in source
						select part.Second);
					});
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[Category("Text")]
		public static void DoLog_FlavorfulCombatTest()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			IEnumerable<ManeuverDef> maneuvers = DefDatabase<ManeuverDef>.AllDefsListForReading;
			IEnumerable<RulePackDef> results = new RulePackDef[]
			{
				RulePackDefOf.Combat_Hit,
				RulePackDefOf.Combat_Miss,
				RulePackDefOf.Combat_Dodge
			};
			foreach (Pair<ManeuverDef, RulePackDef> maneuverresult in maneuvers.Concat(null).Cross(results.Concat(null)))
			{
				DebugMenuOption item = new DebugMenuOption(string.Format("{0}/{1}", (maneuverresult.First != null) ? maneuverresult.First.defName : "(random)", (maneuverresult.Second != null) ? maneuverresult.Second.defName : "(random)"), DebugMenuOptionMode.Action, delegate
				{
					DataAnalysisLogger.CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartDef>, List<bool>> bodyPartCreator)
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int i = 0; i < 100; i++)
						{
							ManeuverDef maneuver = maneuverresult.First;
							RulePackDef rulePackDef = maneuverresult.Second;
							if (maneuver == null)
							{
								maneuver = maneuvers.RandomElement<ManeuverDef>();
							}
							if (rulePackDef == null)
							{
								rulePackDef = results.RandomElement<RulePackDef>();
							}
							List<BodyPartDef> list2 = null;
							List<bool> list3 = null;
							if (rulePackDef == RulePackDefOf.Combat_Hit)
							{
								list2 = new List<BodyPartDef>();
								list3 = new List<bool>();
								bodyPartCreator(list2, list3);
							}
							Pair<ThingDef, Tool> pair = (from ttp in (from td in DefDatabase<ThingDef>.AllDefsListForReading
							where td.IsMeleeWeapon && !td.tools.NullOrEmpty<Tool>()
							select td).SelectMany((ThingDef td) => from tool in td.tools
							select new Pair<ThingDef, Tool>(td, tool))
							where ttp.Second.capacities.Contains(maneuver.requiredCapacity)
							select ttp).RandomElement<Pair<ThingDef, Tool>>();
							BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = new BattleLogEntry_MeleeCombat(rulePackDef, maneuver.combatLogRules, CombatLogTester.GenerateRandom(), CombatLogTester.GenerateRandom(), (pair.Second == null) ? ImplementOwnerTypeDefOf.Bodypart : ImplementOwnerTypeDefOf.Weapon, (pair.Second == null) ? "body part" : pair.Second.label, pair.First, null);
							battleLogEntry_MeleeCombat.FillTargets(list2, list3);
							battleLogEntry_MeleeCombat.Debug_OverrideTicks(Rand.Int);
							stringBuilder.AppendLine(battleLogEntry_MeleeCombat.ToGameStringFromPOV(null));
						}
						Log.Message(stringBuilder.ToString());
					});
				});
				list.Add(item);
			}
			int rf;
			for (rf = 0; rf < 2; rf++)
			{
				list.Add(new DebugMenuOption((rf != 0) ? "Ranged fire burst" : "Ranged fire singleshot", DebugMenuOptionMode.Action, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 100; i++)
					{
						ThingDef thingDef = (from td in DefDatabase<ThingDef>.AllDefsListForReading
						where td.IsRangedWeapon
						select td).RandomElement<ThingDef>();
						bool flag = Rand.Value < 0.2f;
						bool flag2 = !flag && Rand.Value < 0.95f;
						BattleLogEntry_RangedFire battleLogEntry_RangedFire = new BattleLogEntry_RangedFire(CombatLogTester.GenerateRandom(), (!flag) ? CombatLogTester.GenerateRandom() : null, (!flag2) ? thingDef : null, null, rf != 0);
						battleLogEntry_RangedFire.Debug_OverrideTicks(Rand.Int);
						stringBuilder.AppendLine(battleLogEntry_RangedFire.ToGameStringFromPOV(null));
					}
					Log.Message(stringBuilder.ToString());
				}));
			}
			list.Add(new DebugMenuOption("Ranged impact hit", DebugMenuOptionMode.Action, delegate
			{
				DataAnalysisLogger.CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartDef>, List<bool>> bodyPartCreator)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 100; i++)
					{
						ThingDef weaponDef = (from td in DefDatabase<ThingDef>.AllDefsListForReading
						where td.IsRangedWeapon
						select td).RandomElement<ThingDef>();
						List<BodyPartDef> list2 = new List<BodyPartDef>();
						List<bool> list3 = new List<bool>();
						bodyPartCreator(list2, list3);
						Pawn pawn = CombatLogTester.GenerateRandom();
						BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(CombatLogTester.GenerateRandom(), pawn, pawn, weaponDef, null);
						battleLogEntry_RangedImpact.FillTargets(list2, list3);
						battleLogEntry_RangedImpact.Debug_OverrideTicks(Rand.Int);
						stringBuilder.AppendLine(battleLogEntry_RangedImpact.ToGameStringFromPOV(null));
					}
					Log.Message(stringBuilder.ToString());
				});
			}));
			list.Add(new DebugMenuOption("Ranged impact miss", DebugMenuOptionMode.Action, delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < 100; i++)
				{
					ThingDef weaponDef = (from td in DefDatabase<ThingDef>.AllDefsListForReading
					where td.IsRangedWeapon
					select td).RandomElement<ThingDef>();
					BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(CombatLogTester.GenerateRandom(), null, CombatLogTester.GenerateRandom(), weaponDef, null);
					battleLogEntry_RangedImpact.Debug_OverrideTicks(Rand.Int);
					stringBuilder.AppendLine(battleLogEntry_RangedImpact.ToGameStringFromPOV(null));
				}
				Log.Message(stringBuilder.ToString());
			}));
			list.Add(new DebugMenuOption("Ranged impact hit incorrect", DebugMenuOptionMode.Action, delegate
			{
				DataAnalysisLogger.CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartDef>, List<bool>> bodyPartCreator)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 100; i++)
					{
						ThingDef weaponDef = (from td in DefDatabase<ThingDef>.AllDefsListForReading
						where td.IsRangedWeapon
						select td).RandomElement<ThingDef>();
						List<BodyPartDef> list2 = new List<BodyPartDef>();
						List<bool> list3 = new List<bool>();
						bodyPartCreator(list2, list3);
						BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(CombatLogTester.GenerateRandom(), CombatLogTester.GenerateRandom(), CombatLogTester.GenerateRandom(), weaponDef, null);
						battleLogEntry_RangedImpact.FillTargets(list2, list3);
						battleLogEntry_RangedImpact.Debug_OverrideTicks(Rand.Int);
						stringBuilder.AppendLine(battleLogEntry_RangedImpact.ToGameStringFromPOV(null));
					}
					Log.Message(stringBuilder.ToString());
				});
			}));
			foreach (RulePackDef transition in from def in DefDatabase<RulePackDef>.AllDefsListForReading
			where def.defName.Contains("Transition") && !def.defName.Contains("Include")
			select def)
			{
				list.Add(new DebugMenuOption(transition.defName, DebugMenuOptionMode.Action, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 100; i++)
					{
						Pawn pawn = CombatLogTester.GenerateRandom();
						Pawn initiator = CombatLogTester.GenerateRandom();
						BodyPartRecord partRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).RandomElement<BodyPartRecord>();
						BattleLogEntry_StateTransition battleLogEntry_StateTransition = new BattleLogEntry_StateTransition(pawn, transition, initiator, HediffMaker.MakeHediff(DefDatabase<HediffDef>.AllDefsListForReading.RandomElement<HediffDef>(), pawn, partRecord), DefDatabase<BodyPartDef>.AllDefsListForReading.RandomElement<BodyPartDef>());
						battleLogEntry_StateTransition.Debug_OverrideTicks(Rand.Int);
						stringBuilder.AppendLine(battleLogEntry_StateTransition.ToGameStringFromPOV(null));
					}
					Log.Message(stringBuilder.ToString());
				}));
			}
			foreach (RulePackDef damageEvent in from def in DefDatabase<RulePackDef>.AllDefsListForReading
			where def.defName.Contains("DamageEvent") && !def.defName.Contains("Include")
			select def)
			{
				list.Add(new DebugMenuOption(damageEvent.defName, DebugMenuOptionMode.Action, delegate
				{
					DataAnalysisLogger.CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartDef>, List<bool>> bodyPartCreator)
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int i = 0; i < 100; i++)
						{
							List<BodyPartDef> list2 = new List<BodyPartDef>();
							List<bool> list3 = new List<bool>();
							bodyPartCreator(list2, list3);
							Pawn recipient = CombatLogTester.GenerateRandom();
							BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(recipient, damageEvent, null);
							battleLogEntry_DamageTaken.FillTargets(list2, list3);
							battleLogEntry_DamageTaken.Debug_OverrideTicks(Rand.Int);
							stringBuilder.AppendLine(battleLogEntry_DamageTaken.ToGameStringFromPOV(null));
						}
						Log.Message(stringBuilder.ToString());
					});
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		public static void DoLog_ThingList()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (ThingRequestGroup localRg2 in Enum.GetValues(typeof(ThingRequestGroup)))
			{
				ThingRequestGroup localRg = localRg2;
				FloatMenuOption item = new FloatMenuOption(localRg.ToString(), delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					List<Thing> list2 = Find.VisibleMap.listerThings.ThingsInGroup(localRg);
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"Global things in group ",
						localRg,
						" (count ",
						list2.Count,
						")"
					}));
					Log.Message(DebugLogsUtility.ThingListToUniqueCountString(list2));
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DoLog_SimpleCurveTest()
		{
			StringBuilder stringBuilder = new StringBuilder();
			SimpleCurve simpleCurve = new SimpleCurve
			{
				{
					new CurvePoint(5f, 0f),
					true
				},
				{
					new CurvePoint(10f, 1f),
					true
				},
				{
					new CurvePoint(20f, 3f),
					true
				},
				{
					new CurvePoint(40f, 2f),
					true
				}
			};
			for (int i = 0; i < 50; i++)
			{
				stringBuilder.AppendLine(i + " -> " + simpleCurve.Evaluate((float)i));
			}
			Log.Message(stringBuilder.ToString());
		}

		[Category("Text")]
		public static void DoLog_TestPawnNames()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("------Testing parsing");
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'Nick' Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'Nick' von Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John von Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'Nick Hell' Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'Nick Hell' von Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John Nick Hell von Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John O'Neil"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'O'Neil"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'O'Farley' Neil"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'O'''Farley' Neil"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("O'Shea 'O'Farley' O'Neil"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("Missing 'Lastname'"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("Missing 'Lastnamewithspace'     "));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("Unbalanc'ed 'De'limiters'     "));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("\t"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult(string.Empty));
			stringBuilder.AppendLine("------Testing ResolveMissingPieces consistency");
			for (int i = 0; i < 20; i++)
			{
				NameTriple nameTriple = new NameTriple("John", null, "Last");
				nameTriple.ResolveMissingPieces(null);
				stringBuilder.AppendLine(string.Concat(new string[]
				{
					nameTriple.ToString(),
					"       [",
					nameTriple.First,
					"] [",
					nameTriple.Nick,
					"] [",
					nameTriple.Last,
					"]"
				}));
			}
			Log.Message(stringBuilder.ToString());
		}

		private static string PawnNameTestResult(string rawName)
		{
			NameTriple nameTriple = NameTriple.FromString(rawName);
			nameTriple.ResolveMissingPieces(null);
			return string.Concat(new string[]
			{
				rawName,
				" -> ",
				nameTriple.ToString(),
				"       [",
				nameTriple.First,
				"] [",
				nameTriple.Nick,
				"] [",
				nameTriple.Last,
				"]"
			});
		}

		public static void DoLog_PassabilityFill()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (current.passability != Traversability.Standable || current.fillPercent > 0f)
				{
					stringBuilder.Append(string.Concat(new string[]
					{
						current.defName,
						" - pass=",
						current.passability.ToString(),
						", fill=",
						current.fillPercent.ToStringPercent()
					}));
					if (current.passability == Traversability.Impassable && current.fillPercent < 0.1f)
					{
						stringBuilder.Append("   ALERT, impassable with low fill");
					}
					if (current.passability != Traversability.Impassable && current.fillPercent > 0.8f)
					{
						stringBuilder.Append("    ALERT, passabile with very high fill");
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_AnimalsPerBiome()
		{
			IEnumerable<BiomeDef> enumerable = from d in DefDatabase<BiomeDef>.AllDefs
			where d.animalDensity > 0f
			select d;
			IOrderedEnumerable<PawnKindDef> source = from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race.race.Animal
			orderby (!d.race.race.predator) ? 0 : 1
			select d;
			string text = string.Empty;
			text += "name      commonality     commonalityShare     size\n\n";
			foreach (BiomeDef b in enumerable)
			{
				float num = source.Sum((PawnKindDef a) => b.CommonalityOfAnimal(a));
				float f = (from a in source
				where a.race.race.predator
				select a).Sum((PawnKindDef a) => b.CommonalityOfAnimal(a)) / num;
				float num2 = source.Sum((PawnKindDef a) => b.CommonalityOfAnimal(a) * a.race.race.baseBodySize);
				float f2 = (from a in source
				where a.race.race.predator
				select a).Sum((PawnKindDef a) => b.CommonalityOfAnimal(a) * a.race.race.baseBodySize) / num2;
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					b.label,
					"   (predators: ",
					f.ToStringPercent("F2"),
					", predators by size: ",
					f2.ToStringPercent("F2"),
					")"
				});
				foreach (PawnKindDef current in from a in source
				orderby b.CommonalityOfAnimal(a) descending
				select a)
				{
					float num3 = b.CommonalityOfAnimal(current);
					if (num3 > 0f)
					{
						text2 = text;
						text = string.Concat(new string[]
						{
							text2,
							"\n    ",
							current.label,
							(!current.RaceProps.predator) ? string.Empty : "*",
							"       ",
							num3.ToString("F3"),
							"       ",
							(num3 / num).ToStringPercent("F2"),
							"       ",
							current.race.race.baseBodySize.ToString("F2")
						});
					}
				}
				text += "\n\n";
			}
			Log.Message(text);
		}

		public static void DoLog_SmeltProducts()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				Thing thing = ThingMaker.MakeThing(current, GenStuff.DefaultStuffFor(current));
				if (thing.SmeltProducts(1f).Any<Thing>())
				{
					stringBuilder.Append(thing.LabelCap + ": ");
					foreach (Thing current2 in thing.SmeltProducts(1f))
					{
						stringBuilder.Append(" " + current2.Label);
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		[Category("Incident")]
		public static void DoLog_PawnArrivalCandidates()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(IncidentDefOf.RaidEnemy.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.RaidEnemy.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.RaidFriendly.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.RaidFriendly.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.VisitorGroup.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.VisitorGroup.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.TravelerGroup.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.TravelerGroup.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.TraderCaravanArrival.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.TraderCaravanArrival.Worker).DebugListingOfGroupSources());
			Log.Message(stringBuilder.ToString());
		}

		[Category("Text")]
		public static void DoLog_SpecificTaleDescs()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (TaleDef current in from def in DefDatabase<TaleDef>.AllDefs
			orderby def.defName
			select def)
			{
				TaleDef localDef = current;
				FloatMenuOption item = new FloatMenuOption(localDef.defName, delegate
				{
					TaleTester.LogSpecificTale(localDef, 40);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DoLog_SocialPropernessMatters()
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
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_FoodPreferability()
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
			Log.Message(stringBuilder.ToString());
		}

		[Category("Incident")]
		public static void DoLog_CaravanRequests()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Caravan request sample:");
			Map visibleMap = Find.VisibleMap;
			IncidentWorker_CaravanRequest incidentWorker_CaravanRequest = (IncidentWorker_CaravanRequest)IncidentDefOf.CaravanRequest.Worker;
			for (int i = 0; i < 100; i++)
			{
				Settlement settlement = IncidentWorker_CaravanRequest.RandomNearbyTradeableSettlement(visibleMap.Tile);
				if (settlement == null)
				{
					break;
				}
				CaravanRequestComp component = settlement.GetComponent<CaravanRequestComp>();
				if (incidentWorker_CaravanRequest.TryGenerateCaravanRequest(component, visibleMap))
				{
					stringBuilder.AppendLine(string.Format("  {0} -> {1}", GenLabel.ThingLabel(component.requestThingDef, null, component.requestCount), component.rewards[0].Label, ThingDefOf.Silver.label));
				}
				settlement.GetComponent<CaravanRequestComp>().Disable();
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_MapDanger()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Map danger status:");
			foreach (Map current in Find.Maps)
			{
				stringBuilder.AppendLine(string.Format("{0}: {1}", current, current.dangerWatcher.DangerRating));
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_OreValues()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Ore values:");
			foreach (ThingDef current in from def in DefDatabase<ThingDef>.AllDefsListForReading
			where def.mineable && def.building != null && def.building.mineableThing != null
			orderby def.building.mineableThing.GetStatValueAbstract(StatDefOf.MarketValue, null) * (float)def.building.mineableYield
			select def)
			{
				stringBuilder.AppendLine(string.Format("{0}: {1}", current.building.mineableThing.GetStatValueAbstract(StatDefOf.MarketValue, null) * (float)current.building.mineableYield, current));
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_ItemWorkTime()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Item work time:");
			Func<ThingDef, float> workToBuild = (ThingDef def) => Mathf.Max(def.GetStatValueAbstract(StatDefOf.WorkToMake, null), def.GetStatValueAbstract(StatDefOf.WorkToBuild, null));
			foreach (ThingDef current in (from def in DefDatabase<ThingDef>.AllDefsListForReading
			where workToBuild(def) > 1f
			select def).OrderBy(workToBuild))
			{
				stringBuilder.AppendLine(string.Format("{0} {1}: {2}", workToBuild(current), (current.building == null) ? string.Empty : " B", current));
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_DefLabels()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type type in from def in GenDefDatabase.AllDefTypesWithDatabases()
			orderby def.Name
			select def)
			{
				DebugMenuOption item = new DebugMenuOption(type.Name, DebugMenuOptionMode.Action, delegate
				{
					IEnumerable source = (IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), type, "AllDefs");
					StringBuilder stringBuilder = new StringBuilder();
					foreach (Def current in source.Cast<Def>())
					{
						stringBuilder.AppendLine(current.label);
					}
					Log.Message(stringBuilder.ToString());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[ModeRestrictionPlay]
		public static void DoLog_PlantProportions()
		{
			GenPlant.LogPlantProportions();
		}

		[ModeRestrictionPlay]
		public static void DoLog_DatabaseTalesList()
		{
			Find.TaleManager.LogTales();
		}

		[ModeRestrictionPlay]
		public static void DoLog_DatabaseTalesInterest()
		{
			Find.TaleManager.LogTaleInterestSummary();
		}

		[ModeRestrictionPlay]
		public static void DoLog_DatabaseTalesDescs()
		{
			TaleTester.LogTalesInDatabase();
		}

		[ModeRestrictionPlay]
		public static void DoLog_RandomTalesDescs()
		{
			TaleTester.LogGeneratedTales(40);
		}

		[ModeRestrictionPlay]
		public static void DoLog_TalelessDescs()
		{
			TaleTester.LogDescriptionsTaleless();
		}

		[ModeRestrictionPlay]
		public static void DoLog_TemperatureData()
		{
			Find.VisibleMap.mapTemperature.DebugLogTemps();
		}

		[ModeRestrictionPlay]
		public static void DoLog_WeatherChances()
		{
			Find.VisibleMap.weatherDecider.LogWeatherChances();
		}

		[ModeRestrictionPlay]
		public static void DoLog_CelestialGlow()
		{
			GenCelestial.LogSunGlowForYear();
		}

		[ModeRestrictionPlay]
		public static void DoLog_ListerPawns()
		{
			Find.VisibleMap.mapPawns.LogListedPawns();
		}

		[ModeRestrictionPlay]
		public static void DoLog_WindSpeeds()
		{
			Find.VisibleMap.windManager.LogWindSpeeds();
		}

		[ModeRestrictionPlay]
		public static void DoLog_KidnappedPawns()
		{
			Find.FactionManager.LogKidnappedPawns();
		}

		[Category("World Pawn"), ModeRestrictionPlay]
		public static void DoLog_WorldPawnList()
		{
			Find.WorldPawns.LogWorldPawns();
		}

		[Category("World Pawn"), ModeRestrictionPlay]
		public static void DoLog_WorldPawnMothballInfo()
		{
			Find.WorldPawns.LogWorldPawnMothballPrevention();
		}

		[Category("World Pawn"), ModeRestrictionPlay]
		public static void DoLog_WorldPawnGcBreakdown()
		{
			Find.WorldPawns.gc.LogGC();
		}

		[Category("World Pawn"), ModeRestrictionPlay]
		public static void DoLog_WorldPawnDotgraph()
		{
			Find.WorldPawns.gc.LogDotgraph();
		}

		[Category("World Pawn"), ModeRestrictionPlay]
		public static void DoLog_RunWorldPawnGc()
		{
			Find.WorldPawns.gc.RunGC();
		}

		[Category("World Pawn"), ModeRestrictionPlay]
		public static void DoLog_RunWorldPawnMothball()
		{
			Find.WorldPawns.DebugRunMothballProcessing();
		}

		[ModeRestrictionPlay]
		public static void DoLog_DrawList()
		{
			Find.VisibleMap.dynamicDrawManager.LogDynamicDrawThings();
		}

		[Category("Incident"), ModeRestrictionPlay]
		public static void DoLog_FutureIncidents()
		{
			StorytellerUtility.DebugLogTestFutureIncidents(false);
		}

		[Category("Incident"), ModeRestrictionPlay]
		public static void DoLog_FutureIncidentsVisibleMap()
		{
			StorytellerUtility.DebugLogTestFutureIncidents(true);
		}

		[Category("Incident"), ModeRestrictionPlay]
		public static void DoLog_IncidentTargets()
		{
			StorytellerUtility.DebugLogTestIncidentTargets();
		}

		[ModeRestrictionPlay]
		public static void DoLog_MapPawns()
		{
			Find.VisibleMap.mapPawns.LogListedPawns();
		}
	}
}
