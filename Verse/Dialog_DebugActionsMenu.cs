using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using Verse.Profile;

namespace Verse
{
	public class Dialog_DebugActionsMenu : Dialog_DebugOptionLister
	{
		private const float DebugOptionsGap = 7f;

		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public Dialog_DebugActionsMenu()
		{
			this.forcePause = true;
		}

		protected override void DoListingItems()
		{
			if (KeyBindingDefOf.ToggleDebugActionsMenu.KeyDownEvent)
			{
				Event.current.Use();
				this.Close(true);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (WorldRendererUtility.WorldRenderedNow)
				{
					this.DoListingItems_World();
				}
				else if (Find.VisibleMap != null)
				{
					this.DoListingItems_MapActions();
					this.DoListingItems_MapTools();
				}
				this.DoListingItems_AllModePlayActions();
			}
			else
			{
				this.DoListingItems_Entry();
			}
		}

		private void DoListingItems_Entry()
		{
			this.DoLabel("Translation tools");
			base.DebugAction("Write backstory translation file", delegate
			{
				LanguageDataWriter.WriteBackstoryFile();
			});
			base.DebugAction("Output translation report", delegate
			{
				LanguageReportGenerator.OutputTranslationReport();
			});
		}

		private void DoListingItems_AllModePlayActions()
		{
			this.DoGap();
			this.DoLabel("Actions - Map management");
			base.DebugAction("Generate map", delegate
			{
				MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.FactionBase);
				mapParent.Tile = TileFinder.RandomStartingTile();
				mapParent.SetFaction(Faction.OfPlayer);
				Find.WorldObjects.Add(mapParent);
				GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, new IntVec3(50, 1, 50), null);
			});
			base.DebugAction("Destroy map", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					Map map = maps[i];
					list.Add(new DebugMenuOption(map.ToString(), DebugMenuOptionMode.Action, delegate
					{
						Current.Game.DeinitAndRemoveMap(map);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolMap("Transfer", delegate
			{
				List<Thing> toTransfer = Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).ToList<Thing>();
				if (!toTransfer.Any<Thing>())
				{
					return;
				}
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					Map map = maps[i];
					if (map != Find.VisibleMap)
					{
						list.Add(new DebugMenuOption(map.ToString(), DebugMenuOptionMode.Action, delegate
						{
							for (int j = 0; j < toTransfer.Count; j++)
							{
								IntVec3 center;
								if (CellFinder.TryFindRandomCellNear(map.Center, map, Mathf.Max(map.Size.x, map.Size.z), (IntVec3 x) => !x.Fogged(map) && x.Standable(map), out center))
								{
									toTransfer[j].DeSpawn();
									GenPlace.TryPlaceThing(toTransfer[j], center, map, ThingPlaceMode.Near, null);
								}
								else
								{
									Log.Error("Could not find spawn cell.");
								}
							}
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Change map", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					Map map = maps[i];
					if (map != Find.VisibleMap)
					{
						list.Add(new DebugMenuOption(map.ToString(), DebugMenuOptionMode.Action, delegate
						{
							Current.Game.VisibleMap = map;
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
		}

		private void DoListingItems_MapActions()
		{
			Text.Font = GameFont.Tiny;
			this.DoLabel("Incidents");
			this.DoExecuteIncidentDebugAction(Find.VisibleMap, null);
			this.DoExecuteIncidentWithDebugAction(Find.VisibleMap, null);
			base.DebugAction("Execute raid with...", delegate
			{
				StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_ThreatCycle || x is StorytellerComp_RandomMain);
				IncidentParms parms = storytellerComp.GenerateParms(IncidentCategory.ThreatBig, Find.VisibleMap);
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Faction current in Find.FactionManager.AllFactions)
				{
					Faction localFac = current;
					list.Add(new DebugMenuOption(localFac.Name + " (" + localFac.def.defName + ")", DebugMenuOptionMode.Action, delegate
					{
						parms.faction = localFac;
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (float num in Dialog_DebugActionsMenu.PointsOptions())
						{
							float localPoints = num;
							list2.Add(new DebugMenuOption(num + " points", DebugMenuOptionMode.Action, delegate
							{
								parms.points = localPoints;
								List<DebugMenuOption> list3 = new List<DebugMenuOption>();
								foreach (RaidStrategyDef current2 in DefDatabase<RaidStrategyDef>.AllDefs)
								{
									RaidStrategyDef localStrat = current2;
									string text = localStrat.defName;
									if (!localStrat.Worker.CanUseWith(parms))
									{
										text += " [NO]";
									}
									list3.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
									{
										parms.raidStrategy = localStrat;
										this.DoRaid(parms);
									}));
								}
								Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			Action<int> DoRandomEnemyRaid = delegate(int pts)
			{
				this.Close(true);
				IncidentParms incidentParms = new IncidentParms();
				incidentParms.target = Find.VisibleMap;
				incidentParms.points = (float)pts;
				IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
			};
			base.DebugAction("Raid (35 pts)", delegate
			{
				DoRandomEnemyRaid(35);
			});
			base.DebugAction("Raid (75 pts)", delegate
			{
				DoRandomEnemyRaid(75);
			});
			base.DebugAction("Raid (300 pts)", delegate
			{
				DoRandomEnemyRaid(300);
			});
			base.DebugAction("Raid (400 pts)", delegate
			{
				DoRandomEnemyRaid(400);
			});
			base.DebugAction("Raid  (1000 pts)", delegate
			{
				DoRandomEnemyRaid(1000);
			});
			base.DebugAction("Raid  (3000 pts)", delegate
			{
				DoRandomEnemyRaid(3000);
			});
			this.DoGap();
			this.DoLabel("Actions - Misc");
			base.DebugAction("Change weather...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (WeatherDef current in DefDatabase<WeatherDef>.AllDefs)
				{
					WeatherDef localWeather = current;
					list.Add(new DebugMenuOption(localWeather.LabelCap, DebugMenuOptionMode.Action, delegate
					{
						Find.VisibleMap.weatherManager.TransitionTo(localWeather);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Start song...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (SongDef current in DefDatabase<SongDef>.AllDefs)
				{
					SongDef localSong = current;
					list.Add(new DebugMenuOption(localSong.defName, DebugMenuOptionMode.Action, delegate
					{
						Find.MusicManagerPlay.ForceStartSong(localSong, false);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			if (Find.VisibleMap.gameConditionManager.ActiveConditions.Count > 0)
			{
				base.DebugAction("End game condition ...", delegate
				{
					List<DebugMenuOption> list = new List<DebugMenuOption>();
					foreach (GameCondition current in Find.VisibleMap.gameConditionManager.ActiveConditions)
					{
						GameCondition localMc = current;
						list.Add(new DebugMenuOption(localMc.LabelCap, DebugMenuOptionMode.Action, delegate
						{
							localMc.End();
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
				});
			}
			base.DebugAction("Add prisoner", delegate
			{
				this.AddGuest(true);
			});
			base.DebugAction("Add guest", delegate
			{
				this.AddGuest(false);
			});
			base.DebugAction("Force enemy assault", delegate
			{
				foreach (Lord current in Find.VisibleMap.lordManager.lords)
				{
					LordToil_Stage lordToil_Stage = current.CurLordToil as LordToil_Stage;
					if (lordToil_Stage != null)
					{
						foreach (Transition current2 in current.Graph.transitions)
						{
							if (current2.sources.Contains(lordToil_Stage) && current2.target is LordToil_AssaultColony)
							{
								Messages.Message("Debug forcing to assault toil: " + current.faction, MessageSound.SeriousAlert);
								current.GotoToil(current2.target);
								return;
							}
						}
					}
				}
			});
			base.DebugAction("Force enemy flee", delegate
			{
				foreach (Lord current in Find.VisibleMap.lordManager.lords)
				{
					if (current.faction.HostileTo(Faction.OfPlayer) && current.faction.def.autoFlee)
					{
						LordToil lordToil = current.Graph.lordToils.FirstOrDefault((LordToil st) => st is LordToil_PanicFlee);
						if (lordToil != null)
						{
							current.GotoToil(lordToil);
						}
					}
				}
			});
			base.DebugAction("Destroy all things", delegate
			{
				foreach (Thing current in Find.VisibleMap.listerThings.AllThings.ToList<Thing>())
				{
					current.Destroy(DestroyMode.Vanish);
				}
			});
			base.DebugAction("Destroy all plants", delegate
			{
				foreach (Thing current in Find.VisibleMap.listerThings.AllThings.ToList<Thing>())
				{
					if (current is Plant)
					{
						current.Destroy(DestroyMode.Vanish);
					}
				}
			});
			base.DebugAction("Unload unused assets", delegate
			{
				MemoryUtility.UnloadUnusedUnityAssets();
			});
			base.DebugAction("Name colony...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				list.Add(new DebugMenuOption("Faction", DebugMenuOptionMode.Action, delegate
				{
					Find.WindowStack.Add(new Dialog_NamePlayerFaction());
				}));
				if (Find.VisibleMap != null && Find.VisibleMap.IsPlayerHome)
				{
					FactionBase factionBase = (FactionBase)Find.VisibleMap.info.parent;
					list.Add(new DebugMenuOption("Faction base", DebugMenuOptionMode.Action, delegate
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFactionBase(factionBase));
					}));
					list.Add(new DebugMenuOption("Faction and faction base", DebugMenuOptionMode.Action, delegate
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFactionAndBase(factionBase));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Next lesson", delegate
			{
				LessonAutoActivator.DebugForceInitiateBestLessonNow();
			});
			base.DebugAction("Regen all map mesh sections", delegate
			{
				Find.VisibleMap.mapDrawer.RegenerateEverythingNow();
			});
			base.DebugAction("Finish all research", delegate
			{
				Find.ResearchManager.DebugSetAllProjectsFinished();
				Messages.Message("All research finished.", MessageSound.Benefit);
			});
			base.DebugAction("Replace all trade ships", delegate
			{
				Find.VisibleMap.passingShipManager.DebugSendAllShipsAway();
				for (int i = 0; i < 5; i++)
				{
					IncidentParms incidentParms = new IncidentParms();
					incidentParms.target = Find.VisibleMap;
					IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(incidentParms);
				}
			});
			base.DebugAction("Change camera config...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Type current in typeof(CameraMapConfig).AllSubclasses())
				{
					Type localType = current;
					list.Add(new DebugMenuOption(localType.Name, DebugMenuOptionMode.Action, delegate
					{
						Find.CameraDriver.config = (CameraMapConfig)Activator.CreateInstance(localType);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Force ship countdown...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				list.Add(new DebugMenuOption("Player ship", DebugMenuOptionMode.Action, delegate
				{
					ShipCountdown.InitiateCountdown(null, -1);
				}));
				list.Add(new DebugMenuOption("Journey destination", DebugMenuOptionMode.Action, delegate
				{
					ShipCountdown.InitiateCountdown(null, 0);
				}));
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Flash trade drop spot", delegate
			{
				IntVec3 intVec = DropCellFinder.TradeDropSpot(Find.VisibleMap);
				Find.VisibleMap.debugDrawer.FlashCell(intVec, 0f, null);
				Log.Message("trade drop spot: " + intVec);
			});
			base.DebugAction("Kill faction leader", delegate
			{
				Pawn leader = (from x in Find.FactionManager.AllFactions
				where x.leader != null
				select x).RandomElement<Faction>().leader;
				int num = 0;
				while (!leader.Dead)
				{
					if (++num > 1000)
					{
						Log.Warning("Could not kill faction leader.");
						break;
					}
					leader.TakeDamage(new DamageInfo(DamageDefOf.Bullet, 30, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
				}
			});
			base.DebugAction("Refog map", delegate
			{
				FloodFillerFog.DebugRefogMap(Find.VisibleMap);
			});
			base.DebugAction("Use GenStep", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Type current in typeof(GenStep).AllSubclassesNonAbstract())
				{
					Type localGenStep = current;
					list.Add(new DebugMenuOption(localGenStep.Name, DebugMenuOptionMode.Action, delegate
					{
						GenStep genStep = (GenStep)Activator.CreateInstance(localGenStep);
						genStep.Generate(Find.VisibleMap);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Increment time 1 day", delegate
			{
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 60000);
			});
			base.DebugAction("Increment time 1 season", delegate
			{
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 900000);
			});
		}

		private void DoListingItems_MapTools()
		{
			this.DoGap();
			this.DoLabel("Tools - General");
			base.DebugToolMap("Tool: Destroy", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).ToList<Thing>())
				{
					current.Destroy(DestroyMode.Vanish);
				}
			});
			base.DebugToolMap("Tool: Kill", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).ToList<Thing>())
				{
					current.Kill(null);
				}
			});
			base.DebugToolMap("Tool: 10 damage", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).ToList<Thing>())
				{
					current.TakeDamage(new DamageInfo(DamageDefOf.Crush, 10, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
				}
			});
			base.DebugToolMap("Tool: 5000 damage", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).ToList<Thing>())
				{
					current.TakeDamage(new DamageInfo(DamageDefOf.Crush, 5000, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
				}
			});
			base.DebugToolMap("Tool: 5000 flame damage", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).ToList<Thing>())
				{
					current.TakeDamage(new DamageInfo(DamageDefOf.Flame, 5000, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
				}
			});
			base.DebugToolMap("Tool: Clear area 21x21", delegate
			{
				CellRect r = CellRect.CenteredOn(UI.MouseCell(), 10);
				GenDebug.ClearArea(r, Find.VisibleMap);
			});
			base.DebugToolMap("Tool: Rock 21x21", delegate
			{
				CellRect cellRect = CellRect.CenteredOn(UI.MouseCell(), 10);
				cellRect.ClipInsideMap(Find.VisibleMap);
				ThingDef granite = ThingDefOf.Granite;
				foreach (IntVec3 current in cellRect)
				{
					GenSpawn.Spawn(granite, current, Find.VisibleMap);
				}
			});
			this.DoGap();
			base.DebugToolMap("Tool: Explosion (bomb)", delegate
			{
				GenExplosion.DoExplosion(UI.MouseCell(), Find.VisibleMap, 3.9f, DamageDefOf.Bomb, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			});
			base.DebugToolMap("Tool: Explosion (flame)", delegate
			{
				GenExplosion.DoExplosion(UI.MouseCell(), Find.VisibleMap, 3.9f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			});
			base.DebugToolMap("Tool: Explosion (stun)", delegate
			{
				GenExplosion.DoExplosion(UI.MouseCell(), Find.VisibleMap, 3.9f, DamageDefOf.Stun, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			});
			base.DebugToolMap("Tool: Explosion (EMP)", delegate
			{
				GenExplosion.DoExplosion(UI.MouseCell(), Find.VisibleMap, 3.9f, DamageDefOf.EMP, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			});
			base.DebugToolMap("Tool: Explosion (extinguisher)", delegate
			{
				ThingDef filthFireFoam = ThingDefOf.FilthFireFoam;
				GenExplosion.DoExplosion(UI.MouseCell(), Find.VisibleMap, 10f, DamageDefOf.Extinguish, null, null, null, null, filthFireFoam, 1f, 3, true, null, 0f, 1);
			});
			base.DebugToolMap("Tool: Explosion (smoke)", delegate
			{
				ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
				GenExplosion.DoExplosion(UI.MouseCell(), Find.VisibleMap, 10f, DamageDefOf.Smoke, null, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1);
			});
			base.DebugToolMap("Tool: Lightning strike", delegate
			{
				Find.VisibleMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Find.VisibleMap, UI.MouseCell()));
			});
			this.DoGap();
			base.DebugToolMap("Tool: Add snow", delegate
			{
				SnowUtility.AddSnowRadial(UI.MouseCell(), Find.VisibleMap, 5f, 1f);
			});
			base.DebugToolMap("Tool: Remove snow", delegate
			{
				SnowUtility.AddSnowRadial(UI.MouseCell(), Find.VisibleMap, 5f, -1f);
			});
			base.DebugAction("Clear all snow", delegate
			{
				foreach (IntVec3 current in Find.VisibleMap.AllCells)
				{
					Find.VisibleMap.snowGrid.SetDepth(current, 0f);
				}
			});
			base.DebugToolMap("Tool: Push heat (10)", delegate
			{
				GenTemperature.PushHeat(UI.MouseCell(), Find.VisibleMap, 10f);
			});
			base.DebugToolMap("Tool: Push heat (10000)", delegate
			{
				GenTemperature.PushHeat(UI.MouseCell(), Find.VisibleMap, 10000f);
			});
			base.DebugToolMap("Tool: Push heat (-1000)", delegate
			{
				GenTemperature.PushHeat(UI.MouseCell(), Find.VisibleMap, -1000f);
			});
			this.DoGap();
			base.DebugToolMap("Tool: Finish plant growth", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					Plant plant = current as Plant;
					if (plant != null)
					{
						plant.Growth = 1f;
					}
				}
			});
			base.DebugToolMap("Tool: Grow 1 day", delegate
			{
				IntVec3 intVec = UI.MouseCell();
				Plant plant = intVec.GetPlant(Find.VisibleMap);
				if (plant != null && plant.def.plant != null)
				{
					int num = (int)((1f - plant.Growth) * plant.def.plant.growDays);
					if (num >= 60000)
					{
						plant.Age += 60000;
					}
					else if (num > 0)
					{
						plant.Age += num;
					}
					plant.Growth += 1f / plant.def.plant.growDays;
					if ((double)plant.Growth > 1.0)
					{
						plant.Growth = 1f;
					}
					Find.VisibleMap.mapDrawer.SectionAt(intVec).RegenerateAllLayers();
				}
			});
			base.DebugToolMap("Tool: Grow to maturity", delegate
			{
				IntVec3 intVec = UI.MouseCell();
				Plant plant = intVec.GetPlant(Find.VisibleMap);
				if (plant != null && plant.def.plant != null)
				{
					int num = (int)((1f - plant.Growth) * plant.def.plant.growDays);
					plant.Age += num;
					plant.Growth = 1f;
					Find.VisibleMap.mapDrawer.SectionAt(intVec).RegenerateAllLayers();
				}
			});
			base.DebugToolMap("Tool: Reproduce present plant", delegate
			{
				IntVec3 c = UI.MouseCell();
				Plant plant = c.GetPlant(Find.VisibleMap);
				if (plant != null && plant.def.plant != null)
				{
					Plant plant2 = GenPlantReproduction.TryReproduceFrom(plant.Position, plant.def, SeedTargFindMode.Reproduce, plant.Map);
					if (plant2 != null)
					{
						Find.VisibleMap.debugDrawer.FlashCell(plant2.Position, 0f, null);
						Find.VisibleMap.debugDrawer.FlashLine(plant.Position, plant2.Position);
					}
					else
					{
						Find.VisibleMap.debugDrawer.FlashCell(plant.Position, 0f, null);
					}
				}
			});
			base.DebugToolMap("Tool: Reproduce plant...", delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (ThingDef current in from d in DefDatabase<ThingDef>.AllDefs
				where d.category == ThingCategory.Plant && d.plant.reproduces
				select d)
				{
					ThingDef localDef = current;
					list.Add(new FloatMenuOption(localDef.LabelCap, delegate
					{
						Plant plant = GenPlantReproduction.TryReproduceFrom(UI.MouseCell(), localDef, SeedTargFindMode.Reproduce, Find.VisibleMap);
						if (plant != null)
						{
							Find.VisibleMap.debugDrawer.FlashCell(plant.Position, 0f, null);
							Find.VisibleMap.debugDrawer.FlashLine(UI.MouseCell(), plant.Position);
						}
						else
						{
							Find.VisibleMap.debugDrawer.FlashCell(UI.MouseCell(), 0f, null);
						}
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			});
			this.DoGap();
			base.DebugToolMap("Tool: Regen section", delegate
			{
				Find.VisibleMap.mapDrawer.SectionAt(UI.MouseCell()).RegenerateAllLayers();
			});
			base.DebugToolMap("Tool: Randomize color", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					CompColorable compColorable = current.TryGetComp<CompColorable>();
					if (compColorable != null)
					{
						current.SetColor(GenColor.RandomColorOpaque(), true);
					}
				}
			});
			base.DebugToolMap("Tool: Rot 1 day", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					CompRottable compRottable = current.TryGetComp<CompRottable>();
					if (compRottable != null)
					{
						compRottable.RotProgress += 60000f;
					}
				}
			});
			base.DebugToolMap("Tool: Fuel -20%", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					CompRefuelable compRefuelable = current.TryGetComp<CompRefuelable>();
					if (compRefuelable != null)
					{
						compRefuelable.ConsumeFuel(compRefuelable.Props.fuelCapacity * 0.2f);
					}
				}
			});
			base.DebugToolMap("Tool: Break down...", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					CompBreakdownable compBreakdownable = current.TryGetComp<CompBreakdownable>();
					if (compBreakdownable != null && !compBreakdownable.BrokenDown)
					{
						compBreakdownable.DoBreakdown();
					}
				}
			});
			base.DebugAction("Tool: Use scatterer", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_MapGen.Options_Scatterers()));
			});
			base.DebugAction("Tool: BaseGen", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (string current in (from x in DefDatabase<RuleDef>.AllDefs
				select x.symbol).Distinct<string>())
				{
					string localSymbol = current;
					list.Add(new DebugMenuOption(current, DebugMenuOptionMode.Action, delegate
					{
						DebugTool tool = null;
						IntVec3 firstCorner;
						tool = new DebugTool("first corner...", delegate
						{
							firstCorner = UI.MouseCell();
							DebugTools.curTool = new DebugTool("second corner...", delegate
							{
								IntVec3 second = UI.MouseCell();
								CellRect rect = CellRect.FromLimits(firstCorner, second).ClipInsideMap(Find.VisibleMap);
								BaseGen.globalSettings.map = Find.VisibleMap;
								BaseGen.symbolStack.Push(localSymbol, rect);
								BaseGen.Generate();
								DebugTools.curTool = tool;
							}, firstCorner);
						}, null);
						DebugTools.curTool = tool;
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolMap("Tool: Make roof", delegate
			{
				CellRect.CellRectIterator iterator = CellRect.CenteredOn(UI.MouseCell(), 1).GetIterator();
				while (!iterator.Done())
				{
					Find.VisibleMap.roofGrid.SetRoof(iterator.Current, RoofDefOf.RoofConstructed);
					iterator.MoveNext();
				}
			});
			base.DebugToolMap("Tool: Delete roof", delegate
			{
				CellRect.CellRectIterator iterator = CellRect.CenteredOn(UI.MouseCell(), 1).GetIterator();
				while (!iterator.Done())
				{
					Find.VisibleMap.roofGrid.SetRoof(iterator.Current, null);
					iterator.MoveNext();
				}
			});
			base.DebugToolMap("Tool: Toggle trap status", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).ToList<Thing>())
				{
					Building_Trap building_Trap = current as Building_Trap;
					if (building_Trap != null)
					{
						if (building_Trap.Armed)
						{
							building_Trap.Spring(null);
						}
						else
						{
							Building_TrapRearmable building_TrapRearmable = building_Trap as Building_TrapRearmable;
							if (building_TrapRearmable != null)
							{
								building_TrapRearmable.Rearm();
							}
						}
					}
				}
			});
			base.DebugToolMap("Tool: Add trap memory", delegate
			{
				foreach (Faction current in Find.World.factionManager.AllFactions)
				{
					current.TacticalMemory.TrapRevealed(UI.MouseCell(), Find.VisibleMap);
				}
				Find.VisibleMap.debugDrawer.FlashCell(UI.MouseCell(), 0f, "added");
			});
			base.DebugToolMap("Tool: Test flood unfog", delegate
			{
				FloodFillerFog.DebugFloodUnfog(UI.MouseCell(), Find.VisibleMap);
			});
			base.DebugToolMap("Tool: Flash closewalk cell 30", delegate
			{
				IntVec3 c = CellFinder.RandomClosewalkCellNear(UI.MouseCell(), Find.VisibleMap, 30, null);
				Find.VisibleMap.debugDrawer.FlashCell(c, 0f, null);
			});
			base.DebugToolMap("Tool: Flash walk path", delegate
			{
				WalkPathFinder.DebugFlashWalkPath(UI.MouseCell(), 8);
			});
			base.DebugToolMap("Tool: Flash skygaze cell", delegate
			{
				Pawn pawn = Find.VisibleMap.mapPawns.FreeColonists.First<Pawn>();
				IntVec3 c;
				RCellFinder.TryFindSkygazeCell(UI.MouseCell(), pawn, out c);
				Find.VisibleMap.debugDrawer.FlashCell(c, 0f, null);
				MoteMaker.ThrowText(c.ToVector3Shifted(), Find.VisibleMap, "for " + pawn.Label, Color.white, -1f);
			});
			base.DebugToolMap("Tool: Flash direct flee dest", delegate
			{
				Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
				IntVec3 c;
				if (pawn == null)
				{
					Find.VisibleMap.debugDrawer.FlashCell(UI.MouseCell(), 0f, "select a pawn");
				}
				else if (RCellFinder.TryFindDirectFleeDestination(UI.MouseCell(), 9f, pawn, out c))
				{
					Find.VisibleMap.debugDrawer.FlashCell(c, 0.5f, null);
				}
				else
				{
					Find.VisibleMap.debugDrawer.FlashCell(UI.MouseCell(), 0.8f, "not found");
				}
			});
			base.DebugAction("Tool: Flash spectators cells", delegate
			{
				Action<bool> act = delegate(bool bestSideOnly)
				{
					DebugTool tool = null;
					IntVec3 firstCorner;
					tool = new DebugTool("first watch rect corner...", delegate
					{
						firstCorner = UI.MouseCell();
						DebugTools.curTool = new DebugTool("second watch rect corner...", delegate
						{
							IntVec3 second = UI.MouseCell();
							CellRect spectateRect = CellRect.FromLimits(firstCorner, second).ClipInsideMap(Find.VisibleMap);
							SpectateRectSide allowedSides = SpectateRectSide.All;
							if (bestSideOnly)
							{
								allowedSides = SpectatorCellFinder.FindSingleBestSide(spectateRect, Find.VisibleMap, SpectateRectSide.All, 1);
							}
							SpectatorCellFinder.DebugFlashPotentialSpectatorCells(spectateRect, Find.VisibleMap, allowedSides, 1);
							DebugTools.curTool = tool;
						}, firstCorner);
					}, null);
					DebugTools.curTool = tool;
				};
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				list.Add(new DebugMenuOption("All sides", DebugMenuOptionMode.Action, delegate
				{
					act(false);
				}));
				list.Add(new DebugMenuOption("Best side only", DebugMenuOptionMode.Action, delegate
				{
					act(true);
				}));
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Tool: Check reachability", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				TraverseMode[] array = (TraverseMode[])Enum.GetValues(typeof(TraverseMode));
				for (int i = 0; i < array.Length; i++)
				{
					TraverseMode traverseMode3 = array[i];
					TraverseMode traverseMode = traverseMode3;
					list.Add(new DebugMenuOption(traverseMode3.ToString(), DebugMenuOptionMode.Action, delegate
					{
						DebugTool tool = null;
						IntVec3 from;
						Pawn fromPawn;
						tool = new DebugTool("from...", delegate
						{
							from = UI.MouseCell();
							fromPawn = from.GetFirstPawn(Find.VisibleMap);
							string text = "to...";
							if (fromPawn != null)
							{
								text = text + " (pawn=" + fromPawn.LabelShort + ")";
							}
							DebugTools.curTool = new DebugTool(text, delegate
							{
								DebugTools.curTool = tool;
							}, delegate
							{
								IntVec3 c = UI.MouseCell();
								bool flag;
								IntVec3 intVec;
								if (fromPawn != null)
								{
									TraverseMode traverseMode2 = traverseMode;
									flag = fromPawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly, false, traverseMode2);
									intVec = fromPawn.Position;
								}
								else
								{
									flag = Find.VisibleMap.reachability.CanReach(from, c, PathEndMode.OnCell, traverseMode, Danger.Deadly);
									intVec = from;
								}
								Color color = (!flag) ? Color.red : Color.green;
								Widgets.DrawLine(intVec.ToUIPosition(), c.ToUIPosition(), color, 2f);
							});
						}, null);
						DebugTools.curTool = tool;
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolMapForPawns("Tool: Flash TryFindRandomPawnExitCell", delegate(Pawn p)
			{
				IntVec3 intVec;
				if (CellFinder.TryFindRandomPawnExitCell(p, out intVec))
				{
					p.Map.debugDrawer.FlashCell(intVec, 0.5f, null);
					p.Map.debugDrawer.FlashLine(p.Position, intVec);
				}
				else
				{
					p.Map.debugDrawer.FlashCell(p.Position, 0.2f, "no exit cell");
				}
			});
			base.DebugToolMapForPawns("Tool: RandomSpotJustOutsideColony", delegate(Pawn p)
			{
				IntVec3 intVec;
				if (RCellFinder.TryFindRandomSpotJustOutsideColony(p, out intVec))
				{
					p.Map.debugDrawer.FlashCell(intVec, 0.5f, null);
					p.Map.debugDrawer.FlashLine(p.Position, intVec);
				}
				else
				{
					p.Map.debugDrawer.FlashCell(p.Position, 0.2f, "no cell");
				}
			});
			this.DoGap();
			this.DoLabel("Tools - Pawns");
			base.DebugToolMapForPawns("Tool: Damage to down", delegate(Pawn p)
			{
				HealthUtility.DamageUntilDowned(p);
			});
			base.DebugToolMapForPawns("Tool: Damage to death", delegate(Pawn p)
			{
				HealthUtility.DamageUntilDead(p);
			});
			base.DebugToolMap("Tool: Damage held pawn to death", delegate
			{
				foreach (Thing current in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).ToList<Thing>())
				{
					Pawn pawn = current as Pawn;
					if (pawn != null && pawn.carryTracker.CarriedThing != null && pawn.carryTracker.CarriedThing is Pawn)
					{
						HealthUtility.DamageUntilDead((Pawn)pawn.carryTracker.CarriedThing);
					}
				}
			});
			base.DebugToolMapForPawns("Tool: Surgery fail minor", delegate(Pawn p)
			{
				BodyPartRecord bodyPartRecord = p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).RandomElement<BodyPartRecord>();
				Log.Message("part is " + bodyPartRecord);
				HealthUtility.GiveInjuriesOperationFailureMinor(p, bodyPartRecord);
			});
			base.DebugToolMapForPawns("Tool: Surgery fail catastrophic", delegate(Pawn p)
			{
				BodyPartRecord bodyPartRecord = p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).RandomElement<BodyPartRecord>();
				Log.Message("part is " + bodyPartRecord);
				HealthUtility.GiveInjuriesOperationFailureCatastrophic(p, bodyPartRecord);
			});
			base.DebugToolMapForPawns("Tool: Surgery fail ridiculous", delegate(Pawn p)
			{
				HealthUtility.GiveInjuriesOperationFailureRidiculous(p);
			});
			base.DebugToolMapForPawns("Tool: Restore body part...", delegate(Pawn p)
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_RestorePart(p)));
			});
			base.DebugAction("Tool: Apply damage...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_ApplyDamage()));
			});
			base.DebugAction("Tool: Add Hediff...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_AddHediff()));
			});
			base.DebugToolMapForPawns("Tool: Heal random injury (10)", delegate(Pawn p)
			{
				Hediff_Injury hediff_Injury;
				if ((from x in p.health.hediffSet.GetHediffs<Hediff_Injury>()
				where x.CanHealNaturally() || x.CanHealFromTending()
				select x).TryRandomElement(out hediff_Injury))
				{
					hediff_Injury.Heal(10f);
				}
			});
			base.DebugToolMapForPawns("Tool: Activate HediffGiver", delegate(Pawn p)
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				if (p.RaceProps.hediffGiverSets != null)
				{
					foreach (HediffGiver current in p.RaceProps.hediffGiverSets.SelectMany((HediffGiverSetDef set) => set.hediffGivers))
					{
						HediffGiver localHdg = current;
						list.Add(new FloatMenuOption(localHdg.hediff.defName, delegate
						{
							localHdg.TryApply(p, null);
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
				}
				if (list.Any<FloatMenuOption>())
				{
					Find.WindowStack.Add(new FloatMenu(list));
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolMapForPawns("Tool: Give birth", delegate(Pawn p)
			{
				Hediff_Pregnant.DoBirthSpawn(p, null);
				this.DustPuffFrom(p);
			});
			base.DebugToolMapForPawns("Tool: Add/remove pawn relation", delegate(Pawn p)
			{
				if (!p.RaceProps.IsFlesh)
				{
					return;
				}
				Action<bool> act = delegate(bool add)
				{
					if (add)
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (PawnRelationDef current in DefDatabase<PawnRelationDef>.AllDefs)
						{
							if (!current.implied)
							{
								PawnRelationDef defLocal = current;
								list2.Add(new DebugMenuOption(defLocal.defName, DebugMenuOptionMode.Action, delegate
								{
									List<DebugMenuOption> list4 = new List<DebugMenuOption>();
									IOrderedEnumerable<Pawn> orderedEnumerable = from x in PawnsFinder.AllMapsAndWorld_Alive
									where x.RaceProps.IsFlesh
									orderby x.def == p.def descending, x.IsWorldPawn()
									select x;
									foreach (Pawn current2 in orderedEnumerable)
									{
										if (p != current2)
										{
											if (!defLocal.familyByBloodRelation || current2.def == p.def)
											{
												if (!p.relations.DirectRelationExists(defLocal, current2))
												{
													Pawn otherLocal = current2;
													list4.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
													{
														p.relations.AddDirectRelation(defLocal, otherLocal);
													}));
												}
											}
										}
									}
									Find.WindowStack.Add(new Dialog_DebugOptionListLister(list4));
								}));
							}
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}
					else
					{
						List<DebugMenuOption> list3 = new List<DebugMenuOption>();
						List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
						for (int i = 0; i < directRelations.Count; i++)
						{
							DirectPawnRelation rel = directRelations[i];
							list3.Add(new DebugMenuOption(rel.def.defName + " - " + rel.otherPawn.LabelShort, DebugMenuOptionMode.Action, delegate
							{
								p.relations.RemoveDirectRelation(rel);
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
					}
				};
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				list.Add(new DebugMenuOption("Add", DebugMenuOptionMode.Action, delegate
				{
					act(true);
				}));
				list.Add(new DebugMenuOption("Remove", DebugMenuOptionMode.Action, delegate
				{
					act(false);
				}));
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolMapForPawns("Tool: Add opinion thoughts about", delegate(Pawn p)
			{
				if (!p.RaceProps.Humanlike)
				{
					return;
				}
				Action<bool> act = delegate(bool good)
				{
					foreach (Pawn current in from x in p.Map.mapPawns.AllPawnsSpawned
					where x.RaceProps.Humanlike
					select x)
					{
						if (p != current)
						{
							IEnumerable<ThoughtDef> source = DefDatabase<ThoughtDef>.AllDefs.Where((ThoughtDef x) => typeof(Thought_MemorySocial).IsAssignableFrom(x.thoughtClass) && ((good && x.stages[0].baseOpinionOffset > 0f) || (!good && x.stages[0].baseOpinionOffset < 0f)));
							if (source.Any<ThoughtDef>())
							{
								int num = Rand.Range(2, 5);
								for (int i = 0; i < num; i++)
								{
									ThoughtDef def = source.RandomElement<ThoughtDef>();
									current.needs.mood.thoughts.memories.TryGainMemory(def, p);
								}
							}
						}
					}
				};
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				list.Add(new DebugMenuOption("Good", DebugMenuOptionMode.Action, delegate
				{
					act(true);
				}));
				list.Add(new DebugMenuOption("Bad", DebugMenuOptionMode.Action, delegate
				{
					act(false);
				}));
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolMapForPawns("Tool: Force vomit...", delegate(Pawn p)
			{
				p.jobs.StartJob(new Job(JobDefOf.Vomit), JobCondition.InterruptForced, null, true, true, null, null);
			});
			base.DebugToolMap("Tool: Food -20%", delegate
			{
				this.OffsetNeed(NeedDefOf.Food, -0.2f);
			});
			base.DebugToolMap("Tool: Rest -20%", delegate
			{
				this.OffsetNeed(NeedDefOf.Rest, -0.2f);
			});
			base.DebugToolMap("Tool: Joy -20%", delegate
			{
				this.OffsetNeed(NeedDefOf.Joy, -0.2f);
			});
			base.DebugToolMap("Tool: Chemical -20%", delegate
			{
				List<NeedDef> allDefsListForReading = DefDatabase<NeedDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (typeof(Need_Chemical).IsAssignableFrom(allDefsListForReading[i].needClass))
					{
						this.OffsetNeed(allDefsListForReading[i], -0.2f);
					}
				}
			});
			base.DebugToolMapForPawns("Tool: Set skill", delegate(Pawn p)
			{
				if (p.skills == null)
				{
					return;
				}
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (SkillDef current in DefDatabase<SkillDef>.AllDefs)
				{
					SkillDef localDef = current;
					list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						for (int i = 0; i <= 20; i++)
						{
							int level = i;
							list2.Add(new DebugMenuOption(level.ToString(), DebugMenuOptionMode.Action, delegate
							{
								SkillRecord skill = p.skills.GetSkill(localDef);
								skill.Level = level;
								skill.xpSinceLastLevel = skill.XpRequiredForLevelUp / 2f;
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolMapForPawns("Tool: Max skills", delegate(Pawn p)
			{
				if (p.skills != null)
				{
					foreach (SkillDef current in DefDatabase<SkillDef>.AllDefs)
					{
						p.skills.Learn(current, 1E+08f, false);
					}
					this.DustPuffFrom(p);
				}
				if (p.training != null)
				{
					foreach (TrainableDef current2 in DefDatabase<TrainableDef>.AllDefs)
					{
						Pawn trainer = p.Map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
						bool flag;
						if (p.training.CanAssignToTrain(current2, out flag).Accepted)
						{
							p.training.Train(current2, trainer);
						}
					}
				}
			});
			base.DebugAction("Tool: Mental break...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				list.Add(new DebugMenuOption("(log possibles)", DebugMenuOptionMode.Tool, delegate
				{
					foreach (Pawn current2 in (from t in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell())
					where t is Pawn
					select t).Cast<Pawn>())
					{
						current2.mindState.mentalBreaker.LogPossibleMentalBreaks();
						this.DustPuffFrom(current2);
					}
				}));
				list.Add(new DebugMenuOption("(natural mood break)", DebugMenuOptionMode.Tool, delegate
				{
					foreach (Pawn current2 in (from t in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell())
					where t is Pawn
					select t).Cast<Pawn>())
					{
						current2.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak();
						this.DustPuffFrom(current2);
					}
				}));
				foreach (MentalStateDef current in DefDatabase<MentalStateDef>.AllDefs)
				{
					MentalStateDef locBrDef = current;
					string text = locBrDef.defName;
					if (!Find.VisibleMap.mapPawns.FreeColonists.Any((Pawn x) => locBrDef.Worker.StateCanOccur(x)))
					{
						text += " [NO]";
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Tool, delegate
					{
						foreach (Pawn current2 in (from t in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
						{
							Pawn locP = current2;
							if (locBrDef != MentalStateDefOf.SocialFighting)
							{
								locP.mindState.mentalStateHandler.TryStartMentalState(locBrDef, null, true, false, null);
								this.DustPuffFrom(locP);
							}
							else
							{
								DebugTools.curTool = new DebugTool("...with", delegate
								{
									Pawn pawn = (Pawn)(from t in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell())
									where t is Pawn
									select t).FirstOrDefault<Thing>();
									if (pawn != null)
									{
										if (!InteractionUtility.HasAnySocialFightProvokingThought(locP, pawn))
										{
											locP.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Insulted, pawn);
											Messages.Message("Dev: auto added negative thought.", locP, MessageSound.Standard);
										}
										locP.interactions.StartSocialFight(pawn);
										DebugTools.curTool = null;
									}
								}, null);
							}
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Tool: Give trait...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (TraitDef current in DefDatabase<TraitDef>.AllDefs)
				{
					TraitDef trDef = current;
					for (int j = 0; j < current.degreeDatas.Count; j++)
					{
						int i = j;
						list.Add(new DebugMenuOption(string.Concat(new object[]
						{
							trDef.degreeDatas[i].label,
							" (",
							trDef.degreeDatas[j].degree,
							")"
						}), DebugMenuOptionMode.Tool, delegate
						{
							foreach (Pawn current2 in (from t in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell())
							where t is Pawn
							select t).Cast<Pawn>())
							{
								if (current2.story != null)
								{
									Trait item = new Trait(trDef, trDef.degreeDatas[i].degree, false);
									current2.story.traits.allTraits.Add(item);
									this.DustPuffFrom(current2);
								}
							}
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolMapForPawns("Tool: Give good thought", delegate(Pawn p)
			{
				if (p.needs.mood != null)
				{
					p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DebugGood, null);
				}
			});
			base.DebugToolMapForPawns("Tool: Give bad thought", delegate(Pawn p)
			{
				if (p.needs.mood != null)
				{
					p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DebugBad, null);
				}
			});
			base.DebugToolMapForPawns("Tool: Make faction hostile", delegate(Pawn p)
			{
				if (p.Faction != null && !p.Faction.HostileTo(Faction.OfPlayer))
				{
					p.Faction.SetHostileTo(Faction.OfPlayer, true);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolMapForPawns("Tool: Make faction neutral", delegate(Pawn p)
			{
				if (p.Faction != null && p.Faction.HostileTo(Faction.OfPlayer))
				{
					p.Faction.SetHostileTo(Faction.OfPlayer, false);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolMap("Tool: Clear bound unfinished things", delegate
			{
				foreach (Building_WorkTable current in (from t in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell())
				where t is Building_WorkTable
				select t).Cast<Building_WorkTable>())
				{
					foreach (Bill current2 in current.BillStack)
					{
						Bill_ProductionWithUft bill_ProductionWithUft = current2 as Bill_ProductionWithUft;
						if (bill_ProductionWithUft != null)
						{
							bill_ProductionWithUft.ClearBoundUft();
						}
					}
				}
			});
			base.DebugToolMapForPawns("Tool: Force birthday", delegate(Pawn p)
			{
				p.ageTracker.AgeBiologicalTicks = (long)((p.ageTracker.AgeBiologicalYears + 1) * 3600000 + 1);
				p.ageTracker.DebugForceBirthdayBiological();
			});
			base.DebugToolMapForPawns("Tool: Recruit", delegate(Pawn p)
			{
				if (p.Faction != Faction.OfPlayer && p.RaceProps.Humanlike)
				{
					InteractionWorker_RecruitAttempt.DoRecruit(p.Map.mapPawns.FreeColonists.RandomElement<Pawn>(), p, 1f, true);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolMapForPawns("Tool: Damage apparel", delegate(Pawn p)
			{
				if (p.apparel != null && p.apparel.WornApparelCount > 0)
				{
					p.apparel.WornApparel.RandomElement<Apparel>().TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 30, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolMapForPawns("Tool: Tame animal", delegate(Pawn p)
			{
				if (p.RaceProps.Animal && p.Faction != Faction.OfPlayer)
				{
					InteractionWorker_RecruitAttempt.DoRecruit(p.Map.mapPawns.FreeColonists.FirstOrDefault<Pawn>(), p, 1f, true);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolMapForPawns("Tool: Train animal", delegate(Pawn p)
			{
				if (p.RaceProps.Animal && p.Faction == Faction.OfPlayer && p.training != null)
				{
					foreach (TrainableDef current in DefDatabase<TrainableDef>.AllDefs)
					{
						while (!p.training.IsCompleted(current))
						{
							p.training.Train(current, null);
						}
					}
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolMapForPawns("Tool: Name animal by nuzzling", delegate(Pawn p)
			{
				if ((p.Name == null || p.Name.Numerical) && p.RaceProps.Animal)
				{
					PawnUtility.GiveNameBecauseOfNuzzle(p.Map.mapPawns.FreeColonists.First<Pawn>(), p);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolMapForPawns("Tool: Try develop bond relation", delegate(Pawn p)
			{
				if (p.Faction == null)
				{
					return;
				}
				if (p.RaceProps.Humanlike)
				{
					IEnumerable<Pawn> source = from x in p.Map.mapPawns.AllPawnsSpawned
					where x.RaceProps.Animal && x.Faction == p.Faction
					select x;
					if (source.Any<Pawn>())
					{
						RelationsUtility.TryDevelopBondRelation(p, source.RandomElement<Pawn>(), 999999f);
					}
				}
				else if (p.RaceProps.Animal)
				{
					IEnumerable<Pawn> source2 = from x in p.Map.mapPawns.AllPawnsSpawned
					where x.RaceProps.Humanlike && x.Faction == p.Faction
					select x;
					if (source2.Any<Pawn>())
					{
						RelationsUtility.TryDevelopBondRelation(source2.RandomElement<Pawn>(), p, 999999f);
					}
				}
			});
			base.DebugToolMapForPawns("Tool: Start marriage ceremony", delegate(Pawn p)
			{
				if (!p.RaceProps.Humanlike)
				{
					return;
				}
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Pawn current in from x in p.Map.mapPawns.AllPawnsSpawned
				where x.RaceProps.Humanlike
				select x)
				{
					if (p != current)
					{
						Pawn otherLocal = current;
						list.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
						{
							if (!p.relations.DirectRelationExists(PawnRelationDefOf.Fiance, otherLocal))
							{
								p.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherLocal);
								p.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherLocal);
								p.relations.AddDirectRelation(PawnRelationDefOf.Fiance, otherLocal);
								Messages.Message("Dev: auto added fiance relation.", p, MessageSound.Standard);
							}
							if (!p.Map.lordsStarter.TryStartMarriageCeremony(p, otherLocal))
							{
								Messages.Message("Could not find any valid marriage site.", MessageSound.Negative);
							}
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolMapForPawns("Tool: Force interaction", delegate(Pawn p)
			{
				if (p.Faction == null)
				{
					return;
				}
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Pawn current in p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction))
				{
					if (current != p)
					{
						Pawn otherLocal = current;
						list.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
						{
							List<DebugMenuOption> list2 = new List<DebugMenuOption>();
							foreach (InteractionDef current2 in DefDatabase<InteractionDef>.AllDefsListForReading)
							{
								InteractionDef interactionLocal = current2;
								list2.Add(new DebugMenuOption(interactionLocal.label, DebugMenuOptionMode.Action, delegate
								{
									p.interactions.TryInteractWith(otherLocal, interactionLocal);
								}));
							}
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Tool: Start party", delegate
			{
				if (!Find.VisibleMap.lordsStarter.TryStartParty())
				{
					Messages.Message("Could not find any valid party spot or organizer.", MessageSound.Negative);
				}
			});
			base.DebugToolMapForPawns("Tool: Start prison break", delegate(Pawn p)
			{
				if (!p.IsPrisoner)
				{
					return;
				}
				PrisonBreakUtility.StartPrisonBreak(p);
			});
			base.DebugToolMapForPawns("Tool: Pass to world", delegate(Pawn p)
			{
				p.DeSpawn();
				Find.WorldPawns.PassToWorld(p, PawnDiscardDecideMode.KeepForever);
			});
			base.DebugToolMapForPawns("Tool: Make 1 year older", delegate(Pawn p)
			{
				p.ageTracker.DebugMake1YearOlder();
			});
			this.DoGap();
			base.DebugToolMapForPawns("Tool: Try job giver", delegate(Pawn p)
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Type current in typeof(ThinkNode_JobGiver).AllSubclasses())
				{
					Type localType = current;
					list.Add(new DebugMenuOption(localType.Name, DebugMenuOptionMode.Action, delegate
					{
						ThinkNode_JobGiver thinkNode_JobGiver = (ThinkNode_JobGiver)Activator.CreateInstance(localType);
						ThinkResult thinkResult = thinkNode_JobGiver.TryIssueJobPackage(p, default(JobIssueParams));
						if (thinkResult.Job != null)
						{
							p.jobs.StartJob(thinkResult.Job, JobCondition.None, null, false, true, null, null);
						}
						else
						{
							Messages.Message("Failed to give job", MessageSound.Silent);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolMapForPawns("Tool: EndCurrentJob(" + JobCondition.InterruptForced.ToString() + ")", delegate(Pawn p)
			{
				p.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
				this.DustPuffFrom(p);
			});
			base.DebugToolMapForPawns("Tool: CheckForJobOverride", delegate(Pawn p)
			{
				p.jobs.CheckForJobOverride();
				this.DustPuffFrom(p);
			});
			base.DebugToolMapForPawns("Tool: Toggle job logging", delegate(Pawn p)
			{
				p.jobs.debugLog = !p.jobs.debugLog;
				this.DustPuffFrom(p);
				MoteMaker.ThrowText(p.DrawPos, p.Map, p.LabelShort + "\n" + ((!p.jobs.debugLog) ? "OFF" : "ON"), -1f);
			});
			base.DebugToolMapForPawns("Tool: Toggle stance logging", delegate(Pawn p)
			{
				p.stances.debugLog = !p.stances.debugLog;
				this.DustPuffFrom(p);
			});
			this.DoGap();
			this.DoLabel("Tools - Spawning");
			base.DebugAction("Tool: Spawn pawn", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (PawnKindDef current in from kd in DefDatabase<PawnKindDef>.AllDefs
				orderby kd.defName
				select kd)
				{
					PawnKindDef localKindDef = current;
					list.Add(new DebugMenuOption(localKindDef.defName, DebugMenuOptionMode.Tool, delegate
					{
						Faction faction = FactionUtility.DefaultFactionFrom(localKindDef.defaultFactionType);
						Pawn newPawn = PawnGenerator.GeneratePawn(localKindDef, faction);
						GenSpawn.Spawn(newPawn, UI.MouseCell(), Find.VisibleMap);
						if (faction != null && faction != Faction.OfPlayer)
						{
							Lord lord = null;
							if (newPawn.Map.mapPawns.SpawnedPawnsInFaction(faction).Any((Pawn p) => p != newPawn))
							{
								Predicate<Thing> validator = (Thing p) => p != newPawn && ((Pawn)p).GetLord() != null;
								Pawn p2 = (Pawn)GenClosest.ClosestThing_Global(newPawn.Position, newPawn.Map.mapPawns.SpawnedPawnsInFaction(faction), 99999f, validator);
								lord = p2.GetLord();
							}
							if (lord == null)
							{
								LordJob_DefendPoint lordJob = new LordJob_DefendPoint(newPawn.Position);
								lord = LordMaker.MakeNewLord(faction, lordJob, Find.VisibleMap, null);
							}
							lord.AddPawn(newPawn);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Tool: Spawn weapon...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (ThingDef current in from def in DefDatabase<ThingDef>.AllDefs
				where def.equipmentType == EquipmentType.Primary
				select def)
				{
					ThingDef localDef = current;
					list.Add(new DebugMenuOption(localDef.LabelCap, DebugMenuOptionMode.Tool, delegate
					{
						DebugThingPlaceHelper.DebugSpawn(localDef, UI.MouseCell(), -1, false);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Tool: Try place near thing...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(1, false)));
			});
			base.DebugAction("Tool: Try place near stacks of 25...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(25, false)));
			});
			base.DebugAction("Tool: Try place near stacks of 75...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(75, false)));
			});
			base.DebugAction("Tool: Try place direct thing...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(1, true)));
			});
			base.DebugAction("Tool: Try place direct stacks of 25...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(25, true)));
			});
			base.DebugAction("Tool: Set terrain...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (TerrainDef current in DefDatabase<TerrainDef>.AllDefs)
				{
					TerrainDef localDef = current;
					list.Add(new DebugMenuOption(localDef.LabelCap, DebugMenuOptionMode.Tool, delegate
					{
						if (UI.MouseCell().InBounds(Find.VisibleMap))
						{
							Find.VisibleMap.terrainGrid.SetTerrain(UI.MouseCell(), localDef);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolMap("Tool: Make filth x100", delegate
			{
				for (int i = 0; i < 100; i++)
				{
					IntVec3 c = UI.MouseCell() + GenRadial.RadialPattern[i];
					if (c.InBounds(Find.VisibleMap) && c.Walkable(Find.VisibleMap))
					{
						FilthMaker.MakeFilth(c, Find.VisibleMap, ThingDefOf.FilthDirt, 2);
						MoteMaker.ThrowMetaPuff(c.ToVector3Shifted(), Find.VisibleMap);
					}
				}
			});
			base.DebugToolMap("Tool: Spawn faction leader", delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (Faction current in Find.FactionManager.AllFactions)
				{
					Faction localFac = current;
					if (localFac.leader != null)
					{
						list.Add(new FloatMenuOption(localFac.Name + " - " + localFac.leader.Name.ToStringFull, delegate
						{
							GenSpawn.Spawn(localFac.leader, UI.MouseCell(), Find.VisibleMap);
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			});
			base.DebugAction("Spawn world pawn...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				Action<Pawn> act = delegate(Pawn p)
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (PawnKindDef current2 in from x in DefDatabase<PawnKindDef>.AllDefs
					where x.race == p.def
					select x)
					{
						PawnKindDef kLocal = current2;
						list2.Add(new DebugMenuOption(kLocal.defName, DebugMenuOptionMode.Tool, delegate
						{
							PawnGenerationRequest request = new PawnGenerationRequest(kLocal, p.Faction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, false, null, null, null, null, null, null);
							PawnGenerator.RedressPawn(p, request);
							GenSpawn.Spawn(p, UI.MouseCell(), Find.VisibleMap);
							DebugTools.curTool = null;
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				};
				foreach (Pawn current in Find.WorldPawns.AllPawnsAlive)
				{
					Pawn pLocal = current;
					list.Add(new DebugMenuOption(current.LabelShort, DebugMenuOptionMode.Action, delegate
					{
						act(pLocal);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Spawn item collection...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				List<ItemCollectionGeneratorDef> allDefsListForReading = DefDatabase<ItemCollectionGeneratorDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					ItemCollectionGeneratorDef localGenerator = allDefsListForReading[i];
					list.Add(new DebugMenuOption(localGenerator.defName, DebugMenuOptionMode.Tool, delegate
					{
						if (!UI.MouseCell().InBounds(Find.VisibleMap))
						{
							return;
						}
						StringBuilder stringBuilder = new StringBuilder();
						List<Thing> list2 = localGenerator.Worker.GenerateRandomTestItems();
						stringBuilder.AppendLine(string.Concat(new object[]
						{
							localGenerator.Worker.GetType().Name,
							" generated ",
							list2.Count,
							" things:"
						}));
						float num = 0f;
						for (int j = 0; j < list2.Count; j++)
						{
							stringBuilder.AppendLine("   - " + list2[j].LabelCap);
							num += list2[j].MarketValue * (float)list2[j].stackCount;
							if (!GenPlace.TryPlaceThing(list2[j], UI.MouseCell(), Find.VisibleMap, ThingPlaceMode.Near, null))
							{
								list2[j].Destroy(DestroyMode.Vanish);
							}
						}
						stringBuilder.AppendLine("Total market value: " + num.ToString("0.##"));
						Log.Message(stringBuilder.ToString());
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			this.DoGap();
			this.DoLabel("Autotests");
			base.DebugAction("Make colony (full)", delegate
			{
				Autotests_ColonyMaker.MakeColony_Full();
			});
			base.DebugAction("Make colony (animals)", delegate
			{
				Autotests_ColonyMaker.MakeColony_Animals();
			});
			base.DebugAction("Test force downed x100", delegate
			{
				for (int i = 0; i < 100; i++)
				{
					PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
					Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
					GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.VisibleMap), Find.VisibleMap, 1000), Find.VisibleMap);
					HealthUtility.DamageUntilDowned(pawn);
					if (pawn.Dead)
					{
						Log.Error(string.Concat(new object[]
						{
							"Pawn died while force downing: ",
							pawn,
							" at ",
							pawn.Position
						}));
						return;
					}
				}
			});
			base.DebugAction("Test force kill x100", delegate
			{
				for (int i = 0; i < 100; i++)
				{
					PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
					Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
					GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.VisibleMap), Find.VisibleMap, 1000), Find.VisibleMap);
					HealthUtility.DamageUntilDead(pawn);
					if (!pawn.Dead)
					{
						Log.Error(string.Concat(new object[]
						{
							"Pawn died not die: ",
							pawn,
							" at ",
							pawn.Position
						}));
						return;
					}
				}
			});
			base.DebugAction("Test Surgery fail catastrophic x100", delegate
			{
				for (int i = 0; i < 100; i++)
				{
					PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
					Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
					GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.VisibleMap), Find.VisibleMap, 1000), Find.VisibleMap);
					pawn.health.forceIncap = true;
					BodyPartRecord part = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).RandomElement<BodyPartRecord>();
					HealthUtility.GiveInjuriesOperationFailureCatastrophic(pawn, part);
					pawn.health.forceIncap = false;
					if (pawn.Dead)
					{
						Log.Error(string.Concat(new object[]
						{
							"Pawn died: ",
							pawn,
							" at ",
							pawn.Position
						}));
					}
				}
			});
			base.DebugAction("Test Surgery fail ridiculous x100", delegate
			{
				for (int i = 0; i < 100; i++)
				{
					PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
					Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
					GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.VisibleMap), Find.VisibleMap, 1000), Find.VisibleMap);
					pawn.health.forceIncap = true;
					HealthUtility.GiveInjuriesOperationFailureRidiculous(pawn);
					pawn.health.forceIncap = false;
					if (pawn.Dead)
					{
						Log.Error(string.Concat(new object[]
						{
							"Pawn died: ",
							pawn,
							" at ",
							pawn.Position
						}));
					}
				}
			});
			base.DebugAction("Test generate pawn x1000", delegate
			{
				float[] array = new float[]
				{
					10f,
					20f,
					50f,
					100f,
					200f,
					500f,
					1000f,
					2000f,
					5000f,
					1E+20f
				};
				int[] array2 = new int[array.Length];
				for (int i = 0; i < 1000; i++)
				{
					PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
					PerfLogger.Reset();
					Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
					float ms = PerfLogger.Duration() * 1000f;
					array2[array.FirstIndexOf((float time) => ms <= time)]++;
					if (pawn.Dead)
					{
						Log.Error("Pawn is dead");
					}
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Pawn creation time histogram:");
				for (int j = 0; j < array2.Length; j++)
				{
					stringBuilder.AppendLine(string.Format("<{0}ms: {1}", array[j], array2[j]));
				}
				Log.Message(stringBuilder.ToString());
			});
			base.DebugAction("Check region listers", delegate
			{
				Autotests_RegionListers.CheckBugs(Find.VisibleMap);
			});
		}

		private void DoListingItems_World()
		{
			this.DoLabel("Tools - World");
			Text.Font = GameFont.Tiny;
			this.DoLabel("Incidents");
			IIncidentTarget altTarget = Find.WorldSelector.SingleSelectedObject as IIncidentTarget;
			this.DoExecuteIncidentDebugAction(Find.World, altTarget);
			this.DoExecuteIncidentWithDebugAction(Find.World, altTarget);
			this.DoLabel("Tools - Spawning");
			base.DebugToolWorld("Spawn random caravan", delegate
			{
				int num = GenWorld.MouseTile(false);
				Tile tile = Find.WorldGrid[num];
				if (tile.biome.impassable)
				{
					return;
				}
				Caravan caravan = (Caravan)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Caravan);
				caravan.Tile = num;
				caravan.SetFaction(Faction.OfPlayer);
				Find.WorldObjects.Add(caravan);
				int num2 = Rand.RangeInclusive(1, 10);
				for (int i = 0; i < num2; i++)
				{
					Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
					caravan.AddPawn(pawn, true);
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					if (Rand.Value < 0.5f)
					{
						ThingDef thingDef = (from def in DefDatabase<ThingDef>.AllDefs
						where def.IsWeapon && def.PlayerAcquirable
						select def).RandomElementWithFallback(null);
						pawn.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef)));
					}
				}
				List<Thing> list = ItemCollectionGeneratorDefOf.DebugCaravanInventory.Worker.Generate(default(ItemCollectionGeneratorParams));
				for (int j = 0; j < list.Count; j++)
				{
					CaravanInventoryUtility.GiveThing(caravan, list[j]);
				}
			});
			base.DebugToolWorld("Spawn random faction base", delegate
			{
				Faction faction;
				if ((from x in Find.FactionManager.AllFactions
				where !x.IsPlayer && !x.def.hidden
				select x).TryRandomElement(out faction))
				{
					int num = GenWorld.MouseTile(false);
					Tile tile = Find.WorldGrid[num];
					if (tile.biome.impassable)
					{
						return;
					}
					FactionBase factionBase = (FactionBase)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.FactionBase);
					factionBase.SetFaction(faction);
					factionBase.Tile = num;
					factionBase.Name = FactionBaseNameGenerator.GenerateFactionBaseName(factionBase);
					Find.WorldObjects.Add(factionBase);
				}
			});
			base.DebugToolWorld("Spawn site", delegate
			{
				int tile = GenWorld.MouseTile(false);
				if (tile < 0 || Find.World.Impassable(tile))
				{
					Messages.Message("Impassable", MessageSound.RejectInput);
				}
				else
				{
					List<DebugMenuOption> list = new List<DebugMenuOption>();
					List<SitePartDef> parts = new List<SitePartDef>();
					foreach (SiteCoreDef current in DefDatabase<SiteCoreDef>.AllDefs)
					{
						SiteCoreDef localDef = current;
						Action addPart = null;
						addPart = delegate
						{
							List<DebugMenuOption> list2 = new List<DebugMenuOption>();
							list2.Add(new DebugMenuOption("-Done (" + parts.Count + " parts)-", DebugMenuOptionMode.Action, delegate
							{
								Site site = SiteMaker.TryMakeSite(localDef, parts, true, null);
								if (site == null)
								{
									Messages.Message("Could not find any valid faction for this site.", MessageSound.RejectInput);
								}
								else
								{
									site.Tile = tile;
									Find.WorldObjects.Add(site);
								}
							}));
							foreach (SitePartDef current2 in DefDatabase<SitePartDef>.AllDefs)
							{
								SitePartDef localPart = current2;
								list2.Add(new DebugMenuOption(current2.defName, DebugMenuOptionMode.Action, delegate
								{
									parts.Add(localPart);
									addPart();
								}));
							}
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
						};
						list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, addPart));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
				}
			});
		}

		private void DoLabel(string label)
		{
			this.listing.Label(label, -1f);
			this.totalOptionsHeight += Text.CalcHeight(label, 300f) + 2f;
		}

		private void DoGap()
		{
			this.listing.Gap(7f);
			this.totalOptionsHeight += 7f;
		}

		private void DoRaid(IncidentParms parms)
		{
			IncidentDef incidentDef;
			if (parms.faction.HostileTo(Faction.OfPlayer))
			{
				incidentDef = IncidentDefOf.RaidEnemy;
			}
			else
			{
				incidentDef = IncidentDefOf.RaidFriendly;
			}
			incidentDef.Worker.TryExecute(parms);
		}

		private void DoExecuteIncidentDebugAction(IIncidentTarget target, IIncidentTarget altTarget)
		{
			base.DebugAction("Execute incident...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (IncidentDef current in from d in DefDatabase<IncidentDef>.AllDefs
				where d.TargetAllowed(target) || (altTarget != null && d.TargetAllowed(altTarget))
				orderby !d.TargetAllowed(target), d.defName
				select d)
				{
					Dialog_DebugActionsMenu.<DoExecuteIncidentDebugAction>c__AnonStorey5CA.<DoExecuteIncidentDebugAction>c__AnonStorey5CB <DoExecuteIncidentDebugAction>c__AnonStorey5CB = new Dialog_DebugActionsMenu.<DoExecuteIncidentDebugAction>c__AnonStorey5CA.<DoExecuteIncidentDebugAction>c__AnonStorey5CB();
					<DoExecuteIncidentDebugAction>c__AnonStorey5CB.<>f__ref$1482 = <DoExecuteIncidentDebugAction>c__AnonStorey5CA;
					Dialog_DebugActionsMenu.<DoExecuteIncidentDebugAction>c__AnonStorey5CA.<DoExecuteIncidentDebugAction>c__AnonStorey5CB arg_98_0 = <DoExecuteIncidentDebugAction>c__AnonStorey5CB;
					IIncidentTarget arg_98_1;
					if (current.TargetAllowed(target))
					{
						IIncidentTarget target2 = target;
						arg_98_1 = target2;
					}
					else
					{
						arg_98_1 = altTarget;
					}
					arg_98_0.thisIncidentTarget = arg_98_1;
					<DoExecuteIncidentDebugAction>c__AnonStorey5CB.localDef = current;
					string text = <DoExecuteIncidentDebugAction>c__AnonStorey5CB.localDef.defName;
					if (!<DoExecuteIncidentDebugAction>c__AnonStorey5CB.localDef.Worker.CanFireNow(<DoExecuteIncidentDebugAction>c__AnonStorey5CB.thisIncidentTarget))
					{
						text += " [NO]";
					}
					if (<DoExecuteIncidentDebugAction>c__AnonStorey5CB.thisIncidentTarget == altTarget)
					{
						text = text + " (" + altTarget.GetType().Name.Truncate(52f, null) + ")";
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, <DoExecuteIncidentDebugAction>c__AnonStorey5CB.localDef.category, <DoExecuteIncidentDebugAction>c__AnonStorey5CB.thisIncidentTarget);
						if (<DoExecuteIncidentDebugAction>c__AnonStorey5CB.localDef.pointsScaleable)
						{
							StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_ThreatCycle || x is StorytellerComp_RandomMain);
							incidentParms = storytellerComp.GenerateParms(<DoExecuteIncidentDebugAction>c__AnonStorey5CB.localDef.category, incidentParms.target);
						}
						<DoExecuteIncidentDebugAction>c__AnonStorey5CB.localDef.Worker.TryExecute(incidentParms);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
		}

		private void DoExecuteIncidentWithDebugAction(IIncidentTarget target, IIncidentTarget altTarget)
		{
			base.DebugAction("Execute incident with...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (IncidentDef current in from d in DefDatabase<IncidentDef>.AllDefs
				where (d.TargetAllowed(target) || (altTarget != null && d.TargetAllowed(altTarget))) && d.pointsScaleable
				orderby !d.TargetAllowed(target), d.defName
				select d)
				{
					Dialog_DebugActionsMenu.<DoExecuteIncidentWithDebugAction>c__AnonStorey5CC.<DoExecuteIncidentWithDebugAction>c__AnonStorey5CD <DoExecuteIncidentWithDebugAction>c__AnonStorey5CD = new Dialog_DebugActionsMenu.<DoExecuteIncidentWithDebugAction>c__AnonStorey5CC.<DoExecuteIncidentWithDebugAction>c__AnonStorey5CD();
					<DoExecuteIncidentWithDebugAction>c__AnonStorey5CD.<>f__ref$1484 = <DoExecuteIncidentWithDebugAction>c__AnonStorey5CC;
					Dialog_DebugActionsMenu.<DoExecuteIncidentWithDebugAction>c__AnonStorey5CC.<DoExecuteIncidentWithDebugAction>c__AnonStorey5CD arg_98_0 = <DoExecuteIncidentWithDebugAction>c__AnonStorey5CD;
					IIncidentTarget arg_98_1;
					if (current.TargetAllowed(target))
					{
						IIncidentTarget target2 = target;
						arg_98_1 = target2;
					}
					else
					{
						arg_98_1 = altTarget;
					}
					arg_98_0.thisIncidentTarget = arg_98_1;
					<DoExecuteIncidentWithDebugAction>c__AnonStorey5CD.localDef = current;
					string text = <DoExecuteIncidentWithDebugAction>c__AnonStorey5CD.localDef.defName;
					if (!<DoExecuteIncidentWithDebugAction>c__AnonStorey5CD.localDef.Worker.CanFireNow(<DoExecuteIncidentWithDebugAction>c__AnonStorey5CD.thisIncidentTarget))
					{
						text += " [NO]";
					}
					if (<DoExecuteIncidentWithDebugAction>c__AnonStorey5CD.thisIncidentTarget == altTarget)
					{
						text = text + " (" + altTarget.GetType().Name.Truncate(52f, null) + ")";
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						IncidentParms parms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, <DoExecuteIncidentWithDebugAction>c__AnonStorey5CD.localDef.category, <DoExecuteIncidentWithDebugAction>c__AnonStorey5CD.thisIncidentTarget);
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (float num in Dialog_DebugActionsMenu.PointsOptions())
						{
							float localPoints = num;
							list2.Add(new DebugMenuOption(num + " points", DebugMenuOptionMode.Action, delegate
							{
								parms.points = localPoints;
								<DoExecuteIncidentWithDebugAction>c__AnonStorey5CD.localDef.Worker.TryExecute(parms);
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
		}

		private void DebugGiveResource(ThingDef resDef, int count)
		{
			Pawn pawn = Find.VisibleMap.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
			int i = count;
			int num = 0;
			while (i > 0)
			{
				int num2 = Math.Min(resDef.stackLimit, i);
				i -= num2;
				Thing thing = ThingMaker.MakeThing(resDef, null);
				thing.stackCount = num2;
				if (!GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near, null))
				{
					break;
				}
				num += num2;
			}
			Messages.Message(string.Concat(new object[]
			{
				"Made ",
				num,
				" ",
				resDef,
				" near ",
				pawn,
				"."
			}), MessageSound.Benefit);
		}

		private void OffsetNeed(NeedDef nd, float offsetPct)
		{
			foreach (Pawn current in (from t in Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell())
			where t is Pawn
			select t).Cast<Pawn>())
			{
				Need need = current.needs.TryGetNeed(nd);
				if (need != null)
				{
					need.CurLevel += offsetPct * need.MaxLevel;
					this.DustPuffFrom(current);
				}
			}
		}

		private void DustPuffFrom(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				pawn.Drawer.Notify_DebugAffected();
			}
		}

		private void AddGuest(bool prisoner)
		{
			foreach (Building_Bed current in Find.VisibleMap.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>())
			{
				if (current.ForPrisoners == prisoner && (!current.owners.Any<Pawn>() || (prisoner && current.AnyUnownedSleepingSlot)))
				{
					PawnKindDef pawnKindDef;
					if (!prisoner)
					{
						pawnKindDef = PawnKindDefOf.SpaceRefugee;
					}
					else
					{
						pawnKindDef = (from pk in DefDatabase<PawnKindDef>.AllDefs
						where pk.defaultFactionType != null && !pk.defaultFactionType.isPlayer && pk.RaceProps.Humanlike
						select pk).RandomElement<PawnKindDef>();
					}
					Faction faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionType);
					Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
					GenSpawn.Spawn(pawn, current.Position, Find.VisibleMap);
					foreach (ThingWithComps current2 in pawn.equipment.AllEquipmentListForReading.ToList<ThingWithComps>())
					{
						ThingWithComps thingWithComps;
						if (pawn.equipment.TryDropEquipment(current2, out thingWithComps, pawn.Position, true))
						{
							thingWithComps.Destroy(DestroyMode.Vanish);
						}
					}
					pawn.inventory.innerContainer.Clear();
					pawn.ownership.ClaimBedIfNonMedical(current);
					pawn.guest.SetGuestStatus(Faction.OfPlayer, prisoner);
					break;
				}
			}
		}

		[DebuggerHidden]
		public static IEnumerable<float> PointsOptions()
		{
			yield return 35f;
			yield return 70f;
			yield return 135f;
			yield return 200f;
			yield return 300f;
			yield return 500f;
			yield return 800f;
			yield return 1200f;
			yield return 2000f;
			yield return 3000f;
			yield return 4000f;
		}
	}
}
