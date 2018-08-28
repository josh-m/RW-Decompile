using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	[HasDebugOutput]
	public static class DebugOutputsIncidents
	{
		[Category("Incidents"), DebugOutput]
		public static void MiscIncidentChances()
		{
			List<StorytellerComp> storytellerComps = Find.Storyteller.storytellerComps;
			for (int i = 0; i < storytellerComps.Count; i++)
			{
				StorytellerComp_CategoryMTB storytellerComp_CategoryMTB = storytellerComps[i] as StorytellerComp_CategoryMTB;
				if (storytellerComp_CategoryMTB != null && ((StorytellerCompProperties_CategoryMTB)storytellerComp_CategoryMTB.props).category == IncidentCategoryDefOf.Misc)
				{
					storytellerComp_CategoryMTB.DebugTablesIncidentChances(IncidentCategoryDefOf.Misc);
				}
			}
		}

		[Category("Incidents"), DebugOutput]
		public static void TradeRequestsSampled()
		{
			Map currentMap = Find.CurrentMap;
			IncidentWorker_QuestTradeRequest incidentWorker_QuestTradeRequest = (IncidentWorker_QuestTradeRequest)IncidentDefOf.Quest_TradeRequest.Worker;
			Dictionary<ThingDef, int> counts = new Dictionary<ThingDef, int>();
			for (int i = 0; i < 100; i++)
			{
				SettlementBase settlementBase = IncidentWorker_QuestTradeRequest.RandomNearbyTradeableSettlement(currentMap.Tile);
				if (settlementBase == null)
				{
					break;
				}
				TradeRequestComp component = settlementBase.GetComponent<TradeRequestComp>();
				if (incidentWorker_QuestTradeRequest.TryGenerateTradeRequest(component, currentMap))
				{
					if (!counts.ContainsKey(component.requestThingDef))
					{
						counts.Add(component.requestThingDef, 0);
					}
					Dictionary<ThingDef, int> counts2;
					ThingDef requestThingDef;
					(counts2 = counts)[requestThingDef = component.requestThingDef] = counts2[requestThingDef] + 1;
				}
				settlementBase.GetComponent<TradeRequestComp>().Disable();
			}
			IEnumerable<ThingDef> arg_144_0 = from d in DefDatabase<ThingDef>.AllDefs
			where counts.ContainsKey(d)
			orderby counts[d] descending
			select d;
			TableDataGetter<ThingDef>[] expr_F0 = new TableDataGetter<ThingDef>[2];
			expr_F0[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_F0[1] = new TableDataGetter<ThingDef>("appearance rate in " + 100 + " trade requests", (ThingDef d) => ((float)counts[d] / 100f).ToStringPercent());
			DebugTables.MakeTablesDialog<ThingDef>(arg_144_0, expr_F0);
		}

		[Category("Incidents"), DebugOutput, ModeRestrictionPlay]
		public static void FutureIncidents()
		{
			StorytellerUtility.ShowFutureIncidentsDebugLogFloatMenu(false);
		}

		[Category("Incidents"), DebugOutput, ModeRestrictionPlay]
		public static void FutureIncidentsCurrentMap()
		{
			StorytellerUtility.ShowFutureIncidentsDebugLogFloatMenu(true);
		}

		[Category("Incidents"), DebugOutput, ModeRestrictionPlay]
		public static void IncidentTargetsList()
		{
			StorytellerUtility.DebugLogTestIncidentTargets();
		}

		[Category("Incidents"), DebugOutput]
		public static void TradeRequests()
		{
			Map currentMap = Find.CurrentMap;
			IncidentWorker_QuestTradeRequest incidentWorker_QuestTradeRequest = (IncidentWorker_QuestTradeRequest)IncidentDefOf.Quest_TradeRequest.Worker;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Randomly-generated trade requests for map " + currentMap.ToString() + ":");
			stringBuilder.AppendLine();
			for (int i = 0; i < 50; i++)
			{
				SettlementBase settlementBase = IncidentWorker_QuestTradeRequest.RandomNearbyTradeableSettlement(currentMap.Tile);
				if (settlementBase == null)
				{
					break;
				}
				stringBuilder.AppendLine("Settlement: " + settlementBase.LabelCap);
				TradeRequestComp component = settlementBase.GetComponent<TradeRequestComp>();
				if (incidentWorker_QuestTradeRequest.TryGenerateTradeRequest(component, currentMap))
				{
					stringBuilder.AppendLine("Duration: " + (component.expiration - Find.TickManager.TicksGame).ToStringTicksToDays("F1"));
					string str = GenLabel.ThingLabel(component.requestThingDef, null, component.requestCount) + " ($" + (component.requestThingDef.BaseMarketValue * (float)component.requestCount).ToString("F0") + ")";
					stringBuilder.AppendLine("Request: " + str);
					string str2 = GenThing.ThingsToCommaList(component.rewards, false, true, -1) + " ($" + (from t in component.rewards
					select t.MarketValue * (float)t.stackCount).Sum().ToString("F0") + ")";
					stringBuilder.AppendLine("Reward: " + str2);
				}
				else
				{
					stringBuilder.AppendLine("TryGenerateTradeRequest failed.");
				}
				stringBuilder.AppendLine();
				settlementBase.GetComponent<TradeRequestComp>().Disable();
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		[Category("Incidents"), DebugOutput]
		public static void PawnArrivalCandidates()
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
			Log.Message(stringBuilder.ToString(), false);
		}

		[Category("Incidents"), DebugOutput]
		public static void TraderStockMarketValues()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (TraderKindDef current in DefDatabase<TraderKindDef>.AllDefs)
			{
				stringBuilder.AppendLine(current.defName + " : " + ((ThingSetMaker_TraderStock)ThingSetMakerDefOf.TraderStock.root).AverageTotalStockValue(current).ToString("F0"));
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		[Category("Incidents"), DebugOutput]
		public static void TraderStockGeneration()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (TraderKindDef current in DefDatabase<TraderKindDef>.AllDefs)
			{
				TraderKindDef localDef = current;
				FloatMenuOption item = new FloatMenuOption(localDef.defName, delegate
				{
					Log.Message(((ThingSetMaker_TraderStock)ThingSetMakerDefOf.TraderStock.root).GenerationDataFor(localDef), false);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[Category("Incidents"), DebugOutput]
		public static void TraderStockGeneratorsDefs()
		{
			if (Find.CurrentMap == null)
			{
				Log.Error("Requires visible map.", false);
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
					foreach (Thing current2 in gen.GenerateThings(Find.CurrentMap.Tile))
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
			Log.Message(sb.ToString(), false);
		}

		[Category("Incidents"), DebugOutput]
		public static void PawnGroupGenSampled()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				if (current.def.pawnGroupMakers != null)
				{
					if (current.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == PawnGroupKindDefOf.Combat))
					{
						Faction localFac = current;
						list.Add(new DebugMenuOption(localFac.Name + " (" + localFac.def.defName + ")", DebugMenuOptionMode.Action, delegate
						{
							List<DebugMenuOption> list2 = new List<DebugMenuOption>();
							foreach (float localP2 in Dialog_DebugActionsMenu.PointsOptions(true))
							{
								float localP = localP2;
								float maxPawnCost = PawnGroupMakerUtility.MaxPawnCost(localFac, localP, null, PawnGroupKindDefOf.Combat);
								string defName = (from op in localFac.def.pawnGroupMakers.SelectMany((PawnGroupMaker gm) => gm.options)
								where op.Cost <= maxPawnCost
								select op).MaxBy((PawnGenOption op) => op.Cost).kind.defName;
								string label = string.Concat(new string[]
								{
									localP.ToString(),
									", max ",
									maxPawnCost.ToString("F0"),
									" ",
									defName
								});
								list2.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, delegate
								{
									Dictionary<ThingDef, int>[] weaponsCount = new Dictionary<ThingDef, int>[20];
									string[] pawnKinds = new string[20];
									for (int i = 0; i < 20; i++)
									{
										weaponsCount[i] = new Dictionary<ThingDef, int>();
										List<Pawn> list3 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
										{
											groupKind = PawnGroupKindDefOf.Combat,
											tile = Find.CurrentMap.Tile,
											points = localP,
											faction = localFac
										}, false).ToList<Pawn>();
										pawnKinds[i] = PawnUtility.PawnKindsToCommaList(list3, true);
										foreach (Pawn current2 in list3)
										{
											if (current2.equipment.Primary != null)
											{
												if (!weaponsCount[i].ContainsKey(current2.equipment.Primary.def))
												{
													weaponsCount[i].Add(current2.equipment.Primary.def, 0);
												}
												Dictionary<ThingDef, int> dictionary;
												ThingDef def;
												(dictionary = weaponsCount[i])[def = current2.equipment.Primary.def] = dictionary[def] + 1;
											}
											current2.Destroy(DestroyMode.Vanish);
										}
									}
									int totalPawns = weaponsCount.Sum((Dictionary<ThingDef, int> x) => x.Sum((KeyValuePair<ThingDef, int> y) => y.Value));
									List<TableDataGetter<int>> list4 = new List<TableDataGetter<int>>();
									list4.Add(new TableDataGetter<int>(string.Empty, (int x) => (x != 20) ? (x + 1).ToString() : "avg"));
									list4.Add(new TableDataGetter<int>("pawns", delegate(int x)
									{
										string arg_64_0 = " ";
										string arg_64_1;
										if (x == 20)
										{
											arg_64_1 = ((float)totalPawns / 20f).ToString("0.#");
										}
										else
										{
											arg_64_1 = weaponsCount[x].Sum((KeyValuePair<ThingDef, int> y) => y.Value).ToString();
										}
										return arg_64_0 + arg_64_1;
									}));
									list4.Add(new TableDataGetter<int>("kinds", (int x) => (x != 20) ? pawnKinds[x] : string.Empty));
									list4.AddRange(from x in DefDatabase<ThingDef>.AllDefs
									where x.IsWeapon && !x.weaponTags.NullOrEmpty<string>() && weaponsCount.Any((Dictionary<ThingDef, int> wc) => wc.ContainsKey(x))
									orderby x.IsMeleeWeapon descending, x.techLevel, x.BaseMarketValue
									select new TableDataGetter<int>(x.label.Shorten(), delegate(int y)
									{
										if (y == 20)
										{
											return " " + ((float)weaponsCount.Sum((Dictionary<ThingDef, int> z) => (!z.ContainsKey(x)) ? 0 : z[x]) / 20f).ToString("0.#");
										}
										string arg_104_0;
										if (weaponsCount[y].ContainsKey(x))
										{
											object[] expr_66 = new object[5];
											expr_66[0] = " ";
											expr_66[1] = weaponsCount[y][x];
											expr_66[2] = " (";
											expr_66[3] = ((float)weaponsCount[y][x] / (float)weaponsCount[y].Sum((KeyValuePair<ThingDef, int> z) => z.Value)).ToStringPercent("F0");
											expr_66[4] = ")";
											arg_104_0 = string.Concat(expr_66);
										}
										else
										{
											arg_104_0 = string.Empty;
										}
										return arg_104_0;
									}));
									DebugTables.MakeTablesDialog<int>(Enumerable.Range(0, 21), list4.ToArray());
								}));
							}
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
						}));
					}
				}
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[Category("Incidents"), DebugOutput]
		public static void RaidFactionSampled()
		{
			((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidFactionSampled();
		}

		[Category("Incidents"), DebugOutput]
		public static void RaidStrategySampled()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("Choose factions randomly like a real raid", delegate
			{
				((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidStrategySampled(null);
			}, MenuOptionPriority.Default, null, null, 0f, null, null));
			foreach (Faction f in Find.FactionManager.AllFactions)
			{
				Faction f2 = f;
				list.Add(new FloatMenuOption(f2.Name + " (" + f2.def.defName + ")", delegate
				{
					((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidStrategySampled(f);
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[Category("Incidents"), DebugOutput]
		public static void RaidArrivemodeSampled()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("Choose factions randomly like a real raid", delegate
			{
				((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidArrivalModeSampled(null);
			}, MenuOptionPriority.Default, null, null, 0f, null, null));
			foreach (Faction f in Find.FactionManager.AllFactions)
			{
				Faction f2 = f;
				list.Add(new FloatMenuOption(f2.Name + " (" + f2.def.defName + ")", delegate
				{
					((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidArrivalModeSampled(f);
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
