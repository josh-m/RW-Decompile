using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

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
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				this.DoListingItems_GameModeMap();
			}
		}

		[DebuggerHidden]
		private IEnumerable<float> PointsOptions()
		{
			for (int p = 25; p < 10000; p = Mathf.RoundToInt((float)p * 1.25f))
			{
				yield return (float)p;
			}
		}

		private void DoListingItems_GameModeMap()
		{
			Text.Font = GameFont.Tiny;
			this.DoLabel("Incidents");
			base.DebugAction("Execute incident...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (IncidentDef current in from d in DefDatabase<IncidentDef>.AllDefs
				orderby d.defName
				select d)
				{
					IncidentDef localDef = current;
					string text = localDef.defName;
					if (!localDef.Worker.CanFireNow())
					{
						text += " [NO]";
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						IncidentParms parms = new IncidentParms();
						if (localDef.pointsScaleable)
						{
							StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_ThreatCycle || x is StorytellerComp_RandomMain);
							parms = storytellerComp.GenerateParms(localDef.category);
						}
						localDef.Worker.TryExecute(parms);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Execute incident with...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (IncidentDef current in from d in DefDatabase<IncidentDef>.AllDefs
				where d.pointsScaleable
				orderby d.defName
				select d)
				{
					IncidentDef localDef = current;
					string text = localDef.defName;
					if (!localDef.Worker.CanFireNow())
					{
						text += " [NO]";
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						IncidentParms parms = new IncidentParms();
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (float num in this.PointsOptions())
						{
							float localPoints = num;
							list2.Add(new DebugMenuOption(num + " points", DebugMenuOptionMode.Action, delegate
							{
								parms.points = localPoints;
								localDef.Worker.TryExecute(parms);
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Execute raid with...", delegate
			{
				StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_ThreatCycle || x is StorytellerComp_RandomMain);
				IncidentParms parms = storytellerComp.GenerateParms(IncidentCategory.ThreatBig);
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Faction current in Find.FactionManager.AllFactions)
				{
					Faction localFac = current;
					list.Add(new DebugMenuOption(localFac.Name + " (" + localFac.def.defName + ")", DebugMenuOptionMode.Action, delegate
					{
						parms.faction = localFac;
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (float num in this.PointsOptions())
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
						Find.WeatherManager.TransitionTo(localWeather);
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
						Find.MusicManagerMap.ForceStartSong(localSong, false);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			if (Find.MapConditionManager.ActiveConditions.Count > 0)
			{
				base.DebugAction("End map condition ...", delegate
				{
					List<DebugMenuOption> list = new List<DebugMenuOption>();
					foreach (MapCondition current in Find.MapConditionManager.ActiveConditions)
					{
						MapCondition localMc = current;
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
				foreach (Lord current in Find.LordManager.lords)
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
				foreach (Lord current in Find.LordManager.lords)
				{
					if (current.faction.HostileTo(Faction.OfPlayer) && current.faction.def.canFlee)
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
				foreach (Thing current in Find.ListerThings.AllThings.ToList<Thing>())
				{
					current.Destroy(DestroyMode.Vanish);
				}
			});
			base.DebugAction("Destroy all plants", delegate
			{
				foreach (Thing current in Find.ListerThings.AllThings.ToList<Thing>())
				{
					if (current is Plant)
					{
						current.Destroy(DestroyMode.Vanish);
					}
				}
			});
			base.DebugAction("Unload unused assets", delegate
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					Resources.UnloadUnusedAssets();
				}, "UnloadingUnusedAssets", false, null);
			});
			base.DebugAction("Name colony", delegate
			{
				Find.WindowStack.Add(new Dialog_NamePlayerFaction());
			});
			base.DebugAction("Next lesson", delegate
			{
				LessonAutoActivator.DebugForceInitiateBestLessonNow();
			});
			base.DebugAction("Regen all map mesh sections", delegate
			{
				Find.Map.mapDrawer.RegenerateEverythingNow();
			});
			base.DebugAction("Finish all research", delegate
			{
				Find.ResearchManager.DebugSetAllProjectsFinished();
				Messages.Message("All research finished.", MessageSound.Benefit);
			});
			base.DebugAction("Replace all trade ships", delegate
			{
				Find.PassingShipManager.DebugSendAllShipsAway();
				for (int i = 0; i < 5; i++)
				{
					IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(new IncidentParms());
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
			base.DebugAction("Force ship countdown", delegate
			{
				ShipCountdown.InitiateCountdown(null);
			});
			base.DebugAction("Flash trade drop spot", delegate
			{
				IntVec3 intVec = DropCellFinder.TradeDropSpot();
				Find.DebugDrawer.FlashCell(intVec, 0f, null);
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
					leader.TakeDamage(new DamageInfo(DamageDefOf.Bullet, 30, null, 0f, null, null));
				}
			});
			base.DebugAction("Spawn world pawn", delegate
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
							PawnGenerationRequest request = new PawnGenerationRequest(kLocal, p.Faction, PawnGenerationContext.NonPlayer, false, false, false, false, true, false, 1f, false, true, true, null, null, null, null, null, null);
							PawnGenerator.RedressPawn(p, request);
							GenSpawn.Spawn(p, Gen.MouseCell());
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
			base.DebugAction("Refog map", delegate
			{
				FloodFillerFog.TestRefogMap();
			});
			this.DoGap();
			this.DoLabel("Tools - General");
			base.DebugTool("Tool: Destroy", delegate
			{
				foreach (Thing current in Find.ThingGrid.ThingsAt(Gen.MouseCell()).ToList<Thing>())
				{
					current.Destroy(DestroyMode.Vanish);
				}
			});
			base.DebugTool("Tool: 10 damage", delegate
			{
				foreach (Thing current in Find.ThingGrid.ThingsAt(Gen.MouseCell()).ToList<Thing>())
				{
					current.TakeDamage(new DamageInfo(DamageDefOf.Crush, 10, null, null, null));
				}
			});
			base.DebugTool("Tool: 5000 damage", delegate
			{
				foreach (Thing current in Find.ThingGrid.ThingsAt(Gen.MouseCell()).ToList<Thing>())
				{
					current.TakeDamage(new DamageInfo(DamageDefOf.Crush, 5000, null, null, null));
				}
			});
			base.DebugTool("Tool: Clear area 21x21", delegate
			{
				CellRect r = CellRect.CenteredOn(Gen.MouseCell(), 10);
				GenDebug.ClearArea(r);
			});
			base.DebugTool("Tool: Rock 21x21", delegate
			{
				CellRect cellRect = CellRect.CenteredOn(Gen.MouseCell(), 10);
				cellRect.ClipInsideMap();
				ThingDef granite = ThingDefOf.Granite;
				foreach (IntVec3 current in cellRect)
				{
					GenSpawn.Spawn(granite, current);
				}
			});
			this.DoGap();
			base.DebugTool("Tool: Explosion (bomb)", delegate
			{
				GenExplosion.DoExplosion(Gen.MouseCell(), 3.9f, DamageDefOf.Bomb, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			});
			base.DebugTool("Tool: Explosion (flame)", delegate
			{
				GenExplosion.DoExplosion(Gen.MouseCell(), 3.9f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			});
			base.DebugTool("Tool: Explosion (stun)", delegate
			{
				GenExplosion.DoExplosion(Gen.MouseCell(), 3.9f, DamageDefOf.Stun, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			});
			base.DebugTool("Tool: Explosion (EMP)", delegate
			{
				GenExplosion.DoExplosion(Gen.MouseCell(), 3.9f, DamageDefOf.EMP, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			});
			base.DebugTool("Tool: Explosion (extinguisher)", delegate
			{
				ThingDef filthFireFoam = ThingDefOf.FilthFireFoam;
				GenExplosion.DoExplosion(Gen.MouseCell(), 10f, DamageDefOf.Extinguish, null, null, null, null, filthFireFoam, 1f, 3, true, null, 0f, 1);
			});
			base.DebugTool("Tool: Lightning strike", delegate
			{
				Find.WeatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Gen.MouseCell()));
			});
			this.DoGap();
			base.DebugTool("Tool: Add snow", delegate
			{
				SnowUtility.AddSnowRadial(Gen.MouseCell(), 5f, 1f);
			});
			base.DebugTool("Tool: Remove snow", delegate
			{
				SnowUtility.AddSnowRadial(Gen.MouseCell(), 5f, -1f);
			});
			base.DebugAction("Clear all snow", delegate
			{
				foreach (IntVec3 current in Find.Map.AllCells)
				{
					Find.SnowGrid.SetDepth(current, 0f);
				}
			});
			base.DebugTool("Tool: Push heat (10)", delegate
			{
				GenTemperature.PushHeat(Gen.MouseCell(), 10f);
			});
			base.DebugTool("Tool: Push heat (10000)", delegate
			{
				GenTemperature.PushHeat(Gen.MouseCell(), 10000f);
			});
			base.DebugTool("Tool: Push heat (-1000)", delegate
			{
				GenTemperature.PushHeat(Gen.MouseCell(), -1000f);
			});
			this.DoGap();
			base.DebugTool("Tool: Spawn grass seed", delegate
			{
				GenPlantReproduction.TrySpawnSeed(Gen.MouseCell(), ThingDefOf.PlantGrass, SeedTargFindMode.ReproduceSeed, null);
			});
			base.DebugTool("Tool: Finish plant growth", delegate
			{
				foreach (Thing current in Find.ThingGrid.ThingsAt(Gen.MouseCell()))
				{
					Plant plant = current as Plant;
					if (plant != null)
					{
						plant.Growth = 1f;
					}
				}
			});
			base.DebugTool("Tool: Grow 1 day", delegate
			{
				IntVec3 intVec = Gen.MouseCell();
				Plant plant = intVec.GetPlant();
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
					Find.MapDrawer.SectionAt(intVec).RegenerateAllLayers();
				}
			});
			base.DebugTool("Tool: Grow to maturity", delegate
			{
				IntVec3 intVec = Gen.MouseCell();
				Plant plant = intVec.GetPlant();
				if (plant != null && plant.def.plant != null)
				{
					int num = (int)((1f - plant.Growth) * plant.def.plant.growDays);
					plant.Age += num;
					plant.Growth = 1f;
					Find.MapDrawer.SectionAt(intVec).RegenerateAllLayers();
				}
			});
			this.DoGap();
			base.DebugTool("Tool: Regen section", delegate
			{
				Find.MapDrawer.SectionAt(Gen.MouseCell()).RegenerateAllLayers();
			});
			base.DebugTool("Tool: Randomize color", delegate
			{
				foreach (Thing current in Find.ThingGrid.ThingsAt(Gen.MouseCell()))
				{
					CompColorable compColorable = current.TryGetComp<CompColorable>();
					if (compColorable != null)
					{
						current.SetColor(GenColor.RandomColorOpaque(), true);
					}
				}
			});
			base.DebugTool("Tool: Rot 1 day", delegate
			{
				foreach (Thing current in Find.ThingGrid.ThingsAt(Gen.MouseCell()))
				{
					CompRottable compRottable = current.TryGetComp<CompRottable>();
					if (compRottable != null)
					{
						compRottable.RotProgress += 60000f;
					}
				}
			});
			base.DebugTool("Tool: Fuel -20%", delegate
			{
				foreach (Thing current in Find.ThingGrid.ThingsAt(Gen.MouseCell()))
				{
					CompRefuelable compRefuelable = current.TryGetComp<CompRefuelable>();
					if (compRefuelable != null)
					{
						compRefuelable.ConsumeFuel(compRefuelable.Props.fuelCapacity * 0.2f);
					}
				}
			});
			base.DebugTool("Tool: Break down...", delegate
			{
				foreach (Thing current in Find.ThingGrid.ThingsAt(Gen.MouseCell()))
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
			base.DebugTool("Tool: Make roof", delegate
			{
				CellRect.CellRectIterator iterator = CellRect.CenteredOn(Gen.MouseCell(), 1).GetIterator();
				while (!iterator.Done())
				{
					Find.RoofGrid.SetRoof(iterator.Current, RoofDefOf.RoofConstructed);
					iterator.MoveNext();
				}
			});
			base.DebugTool("Tool: Delete roof", delegate
			{
				CellRect.CellRectIterator iterator = CellRect.CenteredOn(Gen.MouseCell(), 1).GetIterator();
				while (!iterator.Done())
				{
					Find.RoofGrid.SetRoof(iterator.Current, null);
					iterator.MoveNext();
				}
			});
			base.DebugTool("Tool: Add trap memory", delegate
			{
				foreach (Faction current in Find.World.factionManager.AllFactions)
				{
					current.TacticalMemory.TrapRevealed(Gen.MouseCell());
				}
				Find.DebugDrawer.FlashCell(Gen.MouseCell(), 0f, "added");
			});
			base.DebugTool("Tool: Test flood unfog", delegate
			{
				FloodFillerFog.TestFloodUnfog(Gen.MouseCell());
			});
			base.DebugTool("Tool: Flash closewalk cell 30", delegate
			{
				IntVec3 c = CellFinder.RandomClosewalkCellNear(Gen.MouseCell(), 30);
				Find.DebugDrawer.FlashCell(c, 0f, null);
			});
			base.DebugTool("Tool: Flash walk path", delegate
			{
				WalkPathFinder.DebugFlashWalkPath(Gen.MouseCell(), 8);
			});
			base.DebugTool("Tool: Flash skygaze cell", delegate
			{
				Pawn pawn = Find.MapPawns.FreeColonists.First<Pawn>();
				IntVec3 c;
				RCellFinder.TryFindSkygazeCell(Gen.MouseCell(), pawn, out c);
				Find.DebugDrawer.FlashCell(c, 0f, null);
				MoteMaker.ThrowText(c.ToVector3Shifted(), "for " + pawn.Label, Color.white, -1f);
			});
			base.DebugTool("Tool: Flash direct flee dest", delegate
			{
				Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
				IntVec3 c;
				if (pawn == null)
				{
					Find.DebugDrawer.FlashCell(Gen.MouseCell(), 0f, "select a pawn");
				}
				else if (RCellFinder.TryFindDirectFleeDestination(Gen.MouseCell(), 9f, pawn, out c))
				{
					Find.DebugDrawer.FlashCell(c, 0.5f, null);
				}
				else
				{
					Find.DebugDrawer.FlashCell(Gen.MouseCell(), 0.8f, "not found");
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
						firstCorner = Gen.MouseCell();
						DebugTools.curTool = new DebugTool("second watch rect corner...", delegate
						{
							IntVec3 second = Gen.MouseCell();
							CellRect spectateRect = CellRect.FromLimits(firstCorner, second);
							SpectateRectSide allowedSides = SpectateRectSide.All;
							if (bestSideOnly)
							{
								allowedSides = SpectatorCellFinder.FindSingleBestSide(spectateRect, SpectateRectSide.All, 1);
							}
							SpectatorCellFinder.DebugFlashPotentialSpectatorCells(spectateRect, allowedSides, 1);
							DebugTools.curTool = tool;
						}, delegate
						{
							IntVec3 intVec = Gen.MouseCell();
							Vector3 position = firstCorner.ToVector3Shifted();
							Vector3 position2 = intVec.ToVector3Shifted();
							if (position.x < position2.x)
							{
								position.x -= 0.5f;
								position2.x += 0.5f;
							}
							else
							{
								position.x += 0.5f;
								position2.x -= 0.5f;
							}
							if (position.z < position2.z)
							{
								position.z -= 0.5f;
								position2.z += 0.5f;
							}
							else
							{
								position.z += 0.5f;
								position2.z -= 0.5f;
							}
							Vector3 vector = Find.Camera.WorldToScreenPoint(position);
							Vector3 vector2 = Find.Camera.WorldToScreenPoint(position2);
							Vector2 vector3 = new Vector2(vector.x, (float)Screen.height - vector.y);
							Vector2 vector4 = new Vector2(vector2.x, (float)Screen.height - vector2.y);
							Widgets.DrawBox(new Rect(vector3.x, vector3.y, vector4.x - vector3.x, vector4.y - vector3.y), 3);
						});
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
			base.DebugToolForPawns("Tool: Flash TryFindRandomPawnExitCell", delegate(Pawn p)
			{
				IntVec3 intVec;
				if (CellFinder.TryFindRandomPawnExitCell(p, out intVec))
				{
					Find.DebugDrawer.FlashCell(intVec, 0.5f, null);
					Find.DebugDrawer.FlashLine(p.Position, intVec);
				}
				else
				{
					Find.DebugDrawer.FlashCell(p.Position, 0.2f, "no exit cell");
				}
			});
			this.DoGap();
			this.DoLabel("Tools - Pawns");
			base.DebugToolForPawns("Tool: Down", delegate(Pawn p)
			{
				HealthUtility.GiveInjuriesToForceDowned(p);
			});
			base.DebugToolForPawns("Tool: Kill", delegate(Pawn p)
			{
				HealthUtility.GiveInjuriesToKill(p);
			});
			base.DebugToolForPawns("Tool: Surgery fail catastrophic", delegate(Pawn p)
			{
				HealthUtility.GiveInjuriesOperationFailureCatastrophic(p);
			});
			base.DebugToolForPawns("Tool: Surgery fail minor", delegate(Pawn p)
			{
				HealthUtility.GiveInjuriesOperationFailureMinor(p);
			});
			base.DebugAction("Tool: Apply damage...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_ApplyDamage()));
			});
			base.DebugAction("Tool: Add Hediff...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_AddHediff()));
			});
			base.DebugToolForPawns("Tool: Force vomit...", delegate(Pawn p)
			{
				p.jobs.StartJob(new Job(JobDefOf.Vomit), JobCondition.InterruptForced, null, true, true, null);
			});
			base.DebugTool("Tool: Food -20%", delegate
			{
				this.OffsetNeed(NeedDefOf.Food, -0.2f);
			});
			base.DebugTool("Tool: Rest -20%", delegate
			{
				this.OffsetNeed(NeedDefOf.Rest, -0.2f);
			});
			base.DebugTool("Tool: Joy -20%", delegate
			{
				this.OffsetNeed(NeedDefOf.Joy, -0.2f);
			});
			base.DebugTool("Tool: Chemical -20%", delegate
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
			base.DebugToolForPawns("Tool: Max skills", delegate(Pawn p)
			{
				if (p.skills != null)
				{
					foreach (SkillDef current in DefDatabase<SkillDef>.AllDefs)
					{
						p.skills.Learn(current, 1E+08f);
					}
					this.DustPuffFrom(p);
				}
				if (p.training != null)
				{
					foreach (TrainableDef current2 in DefDatabase<TrainableDef>.AllDefs)
					{
						Pawn trainer = Find.MapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
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
					foreach (Pawn current2 in (from t in Find.ThingGrid.ThingsAt(Gen.MouseCell())
					where t is Pawn
					select t).Cast<Pawn>())
					{
						current2.mindState.mentalBreaker.LogPossibleMentalBreaks();
						this.DustPuffFrom(current2);
					}
				}));
				list.Add(new DebugMenuOption("(natural mood break)", DebugMenuOptionMode.Tool, delegate
				{
					foreach (Pawn current2 in (from t in Find.ThingGrid.ThingsAt(Gen.MouseCell())
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
					if (!current.Worker.StateCanOccur(Find.MapPawns.FreeColonists.First<Pawn>()))
					{
						text += " [NO]";
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Tool, delegate
					{
						foreach (Pawn current2 in (from t in Find.ThingGrid.ThingsAt(Gen.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
						{
							Pawn locP = current2;
							if (locBrDef != MentalStateDefOf.SocialFighting)
							{
								locP.mindState.mentalStateHandler.TryStartMentalState(locBrDef, null, true);
								this.DustPuffFrom(locP);
							}
							else
							{
								DebugTools.curTool = new DebugTool("...with", delegate
								{
									Pawn pawn = (Pawn)(from t in Find.ThingGrid.ThingsAt(Gen.MouseCell())
									where t is Pawn
									select t).FirstOrDefault<Thing>();
									if (pawn != null)
									{
										if (!InteractionUtility.HasAnySocialFightProvokingThought(locP, pawn))
										{
											locP.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.Insulted, pawn);
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
							foreach (Pawn current2 in (from t in Find.ThingGrid.ThingsAt(Gen.MouseCell())
							where t is Pawn
							select t).Cast<Pawn>())
							{
								if (current2.story != null)
								{
									Trait item = new Trait(trDef, trDef.degreeDatas[i].degree);
									current2.story.traits.allTraits.Add(item);
									this.DustPuffFrom(current2);
								}
							}
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugToolForPawns("Tool: Give good thought", delegate(Pawn p)
			{
				if (p.needs.mood != null)
				{
					p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.DebugGood, null);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolForPawns("Tool: Give bad thought", delegate(Pawn p)
			{
				if (p.needs.mood != null)
				{
					p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.DebugBad, null);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolForPawns("Tool: Make faction hostile", delegate(Pawn p)
			{
				if (p.Faction != null && !p.Faction.HostileTo(Faction.OfPlayer))
				{
					p.Faction.SetHostileTo(Faction.OfPlayer, true);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolForPawns("Tool: Make faction neutral", delegate(Pawn p)
			{
				if (p.Faction != null && p.Faction.HostileTo(Faction.OfPlayer))
				{
					p.Faction.SetHostileTo(Faction.OfPlayer, false);
					this.DustPuffFrom(p);
				}
			});
			base.DebugTool("Tool: Clear bound unfinished things", delegate
			{
				foreach (Building_WorkTable current in (from t in Find.ThingGrid.ThingsAt(Gen.MouseCell())
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
			base.DebugToolForPawns("Tool: Force birthday", delegate(Pawn p)
			{
				p.ageTracker.AgeBiologicalTicks = (long)((p.ageTracker.AgeBiologicalYears + 1) * 3600000 + 1);
				p.ageTracker.DebugForceBirthdayBiological();
			});
			base.DebugToolForPawns("Tool: Recruit", delegate(Pawn p)
			{
				if (p.Faction != Faction.OfPlayer && p.RaceProps.Humanlike)
				{
					InteractionWorker_RecruitAttempt.DoRecruit(Find.MapPawns.FreeColonists.RandomElement<Pawn>(), p, 1f, true);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolForPawns("Tool: Damage apparel", delegate(Pawn p)
			{
				if (p.apparel != null && p.apparel.WornApparelCount > 0)
				{
					p.apparel.WornApparel.RandomElement<Apparel>().TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 30, null, null, null));
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolForPawns("Tool: Tame animal", delegate(Pawn p)
			{
				if (p.RaceProps.Animal && p.Faction != Faction.OfPlayer)
				{
					InteractionWorker_RecruitAttempt.DoRecruit(Find.MapPawns.FreeColonists.FirstOrDefault<Pawn>(), p, 1f, true);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolForPawns("Tool: Give birth", delegate(Pawn p)
			{
				Hediff_Pregnant.DoBirthSpawn(p, null);
				this.DustPuffFrom(p);
			});
			base.DebugToolForPawns("Tool: Name animal by nuzzling", delegate(Pawn p)
			{
				if ((p.Name == null || p.Name.Numerical) && p.RaceProps.Animal)
				{
					PawnUtility.GiveNameBecauseOfNuzzle(Find.MapPawns.FreeColonists.First<Pawn>(), p);
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolForPawns("Tool: Activate HediffGiver", delegate(Pawn p)
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
						}, MenuOptionPriority.Medium, null, null, 0f, null));
					}
				}
				if (list.Any<FloatMenuOption>())
				{
					Find.WindowStack.Add(new FloatMenu(list));
					this.DustPuffFrom(p);
				}
			});
			base.DebugToolForPawns("Tool: Add/remove pawn relation", delegate(Pawn p)
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
									IOrderedEnumerable<Pawn> orderedEnumerable = from x in PawnUtility.AllPawnsMapOrWorldAlive
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
			base.DebugToolForPawns("Tool: Add opinion thoughts about", delegate(Pawn p)
			{
				if (!p.RaceProps.Humanlike)
				{
					return;
				}
				Action<bool> act = delegate(bool good)
				{
					foreach (Pawn current in from x in Find.MapPawns.AllPawnsSpawned
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
									current.needs.mood.thoughts.memories.TryGainMemoryThought(def, p);
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
			base.DebugToolForPawns("Tool: Try develop bond relation", delegate(Pawn p)
			{
				if (p.Faction == null)
				{
					return;
				}
				if (p.RaceProps.Humanlike)
				{
					IEnumerable<Pawn> source = from x in Find.MapPawns.AllPawnsSpawned
					where x.RaceProps.Animal && x.Faction == p.Faction
					select x;
					if (source.Any<Pawn>())
					{
						RelationsUtility.TryDevelopBondRelation(p, source.RandomElement<Pawn>(), 999999f);
					}
				}
				else if (p.RaceProps.Animal)
				{
					IEnumerable<Pawn> source2 = from x in Find.MapPawns.AllPawnsSpawned
					where x.RaceProps.Humanlike && x.Faction == p.Faction
					select x;
					if (source2.Any<Pawn>())
					{
						RelationsUtility.TryDevelopBondRelation(source2.RandomElement<Pawn>(), p, 999999f);
					}
				}
			});
			base.DebugToolForPawns("Tool: Start marriage ceremony", delegate(Pawn p)
			{
				if (!p.RaceProps.Humanlike)
				{
					return;
				}
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Pawn current in from x in Find.MapPawns.AllPawnsSpawned
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
							if (!Find.VoluntarilyJoinableLordsStarter.TryStartMarriageCeremony(p, otherLocal))
							{
								Messages.Message("Could not find any valid marriage site.", MessageSound.Negative);
							}
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugAction("Tool: Start party", delegate
			{
				if (!Find.VoluntarilyJoinableLordsStarter.TryStartParty())
				{
					Messages.Message("Could not find any valid party spot or organizer.", MessageSound.Negative);
				}
			});
			base.DebugToolForPawns("Tool: Start prison break", delegate(Pawn p)
			{
				if (!p.IsPrisoner)
				{
					return;
				}
				PrisonBreakUtility.StartPrisonBreak(p);
			});
			base.DebugToolForPawns("Tool: Pass to world", delegate(Pawn p)
			{
				p.DeSpawn();
				Find.WorldPawns.PassToWorld(p, PawnDiscardDecideMode.Keep);
			});
			this.DoGap();
			base.DebugToolForPawns("Tool: EndCurrentJob(" + JobCondition.InterruptForced.ToString() + ")", delegate(Pawn p)
			{
				p.jobs.EndCurrentJob(JobCondition.InterruptForced);
				this.DustPuffFrom(p);
			});
			base.DebugToolForPawns("Tool: CheckForJobOverride", delegate(Pawn p)
			{
				p.jobs.CheckForJobOverride();
				this.DustPuffFrom(p);
			});
			base.DebugToolForPawns("Tool: Toggle job logging", delegate(Pawn p)
			{
				p.jobs.debugLog = !p.jobs.debugLog;
				this.DustPuffFrom(p);
				MoteMaker.ThrowText(p.DrawPos, p.LabelShort + "\n" + ((!p.jobs.debugLog) ? "OFF" : "ON"), -1f);
			});
			base.DebugToolForPawns("Tool: Toggle stance logging", delegate(Pawn p)
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
						GenSpawn.Spawn(newPawn, Gen.MouseCell());
						if (faction != null && faction != Faction.OfPlayer)
						{
							Lord lord = null;
							if (Find.MapPawns.SpawnedPawnsInFaction(faction).Any((Pawn p) => p != newPawn))
							{
								Predicate<Thing> validator = (Thing p) => p != newPawn && ((Pawn)p).GetLord() != null;
								Pawn p2 = (Pawn)GenClosest.ClosestThing_Global(newPawn.Position, Find.MapPawns.SpawnedPawnsInFaction(faction), 99999f, validator);
								lord = p2.GetLord();
							}
							if (lord == null)
							{
								LordJob_DefendPoint lordJob = new LordJob_DefendPoint(newPawn.Position);
								lord = LordMaker.MakeNewLord(faction, lordJob, null);
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
						DebugThingPlaceHelper.DebugSpawn(localDef, Gen.MouseCell(), -1, false);
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
						if (Gen.MouseCell().InBounds())
						{
							Find.TerrainGrid.SetTerrain(Gen.MouseCell(), localDef);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			base.DebugTool("Tool: Make filth x100", delegate
			{
				for (int i = 0; i < 100; i++)
				{
					IntVec3 c = Gen.MouseCell() + GenRadial.RadialPattern[i];
					if (c.InBounds() && c.Walkable())
					{
						FilthMaker.MakeFilth(c, ThingDefOf.FilthDirt, 2);
						MoteMaker.ThrowMetaPuff(c.ToVector3Shifted());
					}
				}
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
					GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(), 1000));
					HealthUtility.GiveInjuriesToForceDowned(pawn);
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
					GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(), 1000));
					HealthUtility.GiveInjuriesToKill(pawn);
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
			base.DebugAction("Test generate pawn x1000", delegate
			{
				for (int i = 0; i < 1000; i++)
				{
					PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
					Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
					if (pawn.Dead)
					{
						Log.Error("Pawn is dead");
					}
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
			});
		}

		private void DoLabel(string label)
		{
			this.listing.Label(label);
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

		private void DebugGiveResource(ThingDef resDef, int count)
		{
			Pawn pawn = Find.MapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
			int i = count;
			int num = 0;
			while (i > 0)
			{
				int num2 = Math.Min(resDef.stackLimit, i);
				i -= num2;
				Thing thing = ThingMaker.MakeThing(resDef, null);
				thing.stackCount = num2;
				if (!GenPlace.TryPlaceThing(thing, pawn.Position, ThingPlaceMode.Near, null))
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
			foreach (Pawn current in (from t in Find.ThingGrid.ThingsAt(Gen.MouseCell())
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
			foreach (Building_Bed current in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_Bed>())
			{
				if (current.ForPrisoners == prisoner && !current.owners.Any<Pawn>())
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
					GenSpawn.Spawn(pawn, current.Position);
					foreach (ThingWithComps current2 in pawn.equipment.AllEquipment.ToList<ThingWithComps>())
					{
						ThingWithComps thingWithComps;
						if (pawn.equipment.TryDropEquipment(current2, out thingWithComps, pawn.Position, true))
						{
							thingWithComps.Destroy(DestroyMode.Vanish);
						}
					}
					pawn.inventory.container.Clear();
					pawn.ownership.ClaimBedIfNonMedical(current);
					pawn.guest.SetGuestStatus(Faction.OfPlayer, prisoner);
					break;
				}
			}
		}
	}
}
