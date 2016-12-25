using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Verse.Steam;

namespace Verse
{
	public class EditWindow_DebugInspector : EditWindow
	{
		private StringBuilder debugStringBuilder = new StringBuilder();

		public bool fullMode;

		private float columnWidth = 360f;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(400f, 600f);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public EditWindow_DebugInspector()
		{
			this.optionalTitle = "Debug inspector";
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				GenUI.RenderMouseoverBracket();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			if (KeyBindingDefOf.ToggleDebugInspector.KeyDownEvent)
			{
				Event.current.Use();
				this.Close(true);
			}
			WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
			widgetRow.ToggleableIcon(ref this.fullMode, TexButton.InspectModeToggle, "Toggle deep inspection mode for things on the map.", null, null);
			widgetRow.ToggleableIcon(ref DebugViewSettings.writeCellContents, TexButton.InspectModeToggle, "Toggle shallow inspection for things on the map.", null, null);
			if (widgetRow.ButtonText("Visibility", "Toggle what information should be reported by the inspector.", true, false))
			{
				Find.WindowStack.Add(new Dialog_DebugSettingsMenu());
			}
			if (widgetRow.ButtonText("Column Width +", "Make the columns wider.", true, false))
			{
				this.columnWidth += 20f;
				this.columnWidth = Mathf.Clamp(this.columnWidth, 200f, 1600f);
			}
			if (widgetRow.ButtonText("Column Width -", "Make the columns narrower.", true, false))
			{
				this.columnWidth -= 20f;
				this.columnWidth = Mathf.Clamp(this.columnWidth, 200f, 1600f);
			}
			inRect.yMin += 30f;
			Listing_Standard listing_Standard = new Listing_Standard(inRect, GameFont.Tiny);
			listing_Standard.ColumnWidth = Mathf.Min(this.columnWidth, inRect.width);
			string[] array = this.debugStringBuilder.ToString().Split(new char[]
			{
				'\n'
			});
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string label = array2[i];
				listing_Standard.Label(label);
				listing_Standard.Gap(-10f);
			}
			listing_Standard.End();
			if (Event.current.type == EventType.Repaint)
			{
				this.debugStringBuilder = new StringBuilder();
				this.debugStringBuilder.Append(this.CurrentDebugString());
			}
		}

		public void AppendDebugString(string str)
		{
			this.debugStringBuilder.AppendLine(str);
		}

		private string CurrentDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (DebugViewSettings.writeMapConditions)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.MapConditionManager.DebugString());
			}
			if (DebugViewSettings.writePlayingSounds)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine("Sustainers:");
				foreach (Sustainer current in Find.SoundRoot.sustainerManager.AllSustainers)
				{
					stringBuilder.AppendLine(current.DebugString());
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("OneShots:");
				foreach (SampleOneShot current2 in Find.SoundRoot.oneShotManager.PlayingOneShots)
				{
					stringBuilder.AppendLine(current2.ToString());
				}
			}
			if (DebugViewSettings.writeSoundEventsRecord)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine("Recent sound events:\n       ...");
				stringBuilder.AppendLine(DebugSoundEventsLog.EventsListingDebugString);
			}
			if (DebugViewSettings.writeGame)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine((Current.Game != null) ? Current.Game.DebugString() : "Current.Game = null");
			}
			if (DebugViewSettings.writeSteamItems)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(WorkshopItems.DebugOutput());
			}
			if (DebugViewSettings.writeConcepts)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(LessonAutoActivator.DebugString());
			}
			if (DebugViewSettings.writeMemoryUsage)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine("Total allocated: " + Profiler.GetTotalAllocatedMemory().ToStringBytes("F2"));
				stringBuilder.AppendLine("Total reserved: " + Profiler.GetTotalReservedMemory().ToStringBytes("F2"));
				stringBuilder.AppendLine("Total reserved unused: " + Profiler.GetTotalUnusedReservedMemory().ToStringBytes("F2"));
				stringBuilder.AppendLine("Mono heap size: " + Profiler.GetMonoHeapSize().ToStringBytes("F2"));
				stringBuilder.AppendLine("Mono used size: " + Profiler.GetMonoUsedSize().ToStringBytes("F2"));
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				IntVec3 intVec = Gen.MouseCell();
				stringBuilder.AppendLine("Tick " + Find.TickManager.TicksGame);
				stringBuilder.AppendLine("Inspecting " + intVec.ToString());
				if (DebugViewSettings.writeCellContents || this.fullMode)
				{
					stringBuilder.AppendLine("---");
					if (intVec.InBounds())
					{
						foreach (Designation current3 in Find.DesignationManager.AllDesignationsAt(intVec))
						{
							stringBuilder.AppendLine(current3.ToString());
						}
						foreach (Thing current4 in Find.ThingGrid.ThingsAt(intVec))
						{
							if (!this.fullMode)
							{
								stringBuilder.AppendLine(current4.LabelCap + " - " + current4.ToString());
							}
							else
							{
								stringBuilder.AppendLine(Scribe.DebugOutputFor(current4));
								stringBuilder.AppendLine();
							}
						}
					}
				}
				if (DebugViewSettings.debugApparelOptimize)
				{
					stringBuilder.AppendLine("---");
					foreach (Thing current5 in Find.ThingGrid.ThingsAt(intVec))
					{
						Apparel apparel = current5 as Apparel;
						if (apparel != null)
						{
							stringBuilder.AppendLine(apparel.LabelCap);
							stringBuilder.AppendLine("   raw: " + JobGiver_OptimizeApparel.ApparelScoreRaw(apparel).ToString("F2"));
							Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
							if (pawn != null)
							{
								stringBuilder.AppendLine("  Pawn: " + pawn);
								stringBuilder.AppendLine("  gain: " + JobGiver_OptimizeApparel.ApparelScoreGain(pawn, apparel).ToString("F2"));
							}
						}
					}
				}
				if (DebugViewSettings.drawPawnDebug)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.Reservations.DebugString());
				}
				if (DebugViewSettings.writeMoteSaturation)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("Mote count: " + MoteCounter.MoteCount);
					stringBuilder.AppendLine("Mote saturation: " + MoteCounter.Saturation);
				}
				if (DebugViewSettings.writeSnowDepth)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("Snow depth: " + Find.SnowGrid.GetDepth(intVec));
				}
				if (DebugViewSettings.drawDeepResources)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("Deep resource def: " + Find.DeepResourceGrid.ThingDefAt(intVec));
					stringBuilder.AppendLine("Deep resource count: " + Find.DeepResourceGrid.CountAt(intVec));
				}
				if (DebugViewSettings.writeEcosystem)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(WildSpawner.DebugString());
				}
				if (DebugViewSettings.writeTotalSnowDepth)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("Total snow depth: " + Find.SnowGrid.TotalDepth);
				}
				if (DebugViewSettings.writeCanReachColony)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("CanReachColony: " + Gen.MouseCell().CanReachColony());
				}
				if (DebugViewSettings.writeMentalStateCalcs)
				{
					stringBuilder.AppendLine("---");
					foreach (Pawn current6 in (from t in Find.ThingGrid.ThingsAt(Gen.MouseCell())
					where t is Pawn
					select t).Cast<Pawn>())
					{
						stringBuilder.AppendLine(current6.mindState.mentalBreaker.DebugString());
					}
				}
				if (DebugViewSettings.writeWind)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(WindManager.DebugString());
				}
				if (DebugViewSettings.writeWorkSettings)
				{
					foreach (Pawn current7 in (from t in Find.ThingGrid.ThingsAt(Gen.MouseCell())
					where t is Pawn
					select t).Cast<Pawn>())
					{
						if (current7.workSettings != null)
						{
							stringBuilder.AppendLine("---");
							stringBuilder.AppendLine(current7.workSettings.DebugString());
						}
					}
				}
				if (DebugViewSettings.writeApparelScore)
				{
					stringBuilder.AppendLine("---");
					if (intVec.InBounds())
					{
						foreach (Thing current8 in intVec.GetThingList())
						{
							Apparel apparel2 = current8 as Apparel;
							if (apparel2 != null)
							{
								stringBuilder.AppendLine(apparel2.Label + ": " + JobGiver_OptimizeApparel.ApparelScoreRaw(apparel2).ToString("F2"));
							}
						}
					}
				}
				if (DebugViewSettings.writeRecentStrikes)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(MineStrikeManager.DebugStrikeRecords());
				}
				if (DebugViewSettings.writeStoryteller)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.Storyteller.DebugString());
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.StoryWatcher.DebugString());
				}
				if (DebugViewSettings.writeListRepairableBldgs)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(ListerBuildingsRepairable.DebugString());
				}
				if (DebugViewSettings.writeListFilthInHomeArea)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(ListerFilthInHomeArea.DebugString());
				}
				if (DebugViewSettings.writeTerrain)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.TerrainGrid.DebugStringAt(intVec));
				}
				if (DebugViewSettings.writeListHaulables)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(ListerHaulables.DebugString());
				}
				if (DebugViewSettings.drawLords)
				{
					foreach (Lord current9 in Find.LordManager.lords)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine(current9.DebugString());
					}
				}
				if (DebugViewSettings.writeMusicManagerMap)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.MusicManagerMap.DebugString());
				}
				if (DebugViewSettings.writeAttackTargets)
				{
					foreach (Pawn current10 in Find.ThingGrid.ThingsAt(Gen.MouseCell()).OfType<Pawn>())
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("Potential attack targets for " + current10.LabelShort + ":");
						List<IAttackTarget> potentialTargetsFor = Find.AttackTargetsCache.GetPotentialTargetsFor(current10);
						for (int i = 0; i < potentialTargetsFor.Count; i++)
						{
							Thing thing = (Thing)potentialTargetsFor[i];
							stringBuilder.AppendLine(string.Concat(new object[]
							{
								thing.LabelShort,
								", ",
								thing.Faction,
								(!potentialTargetsFor[i].ThreatDisabled()) ? string.Empty : " (threat disabled)"
							}));
						}
					}
				}
				if (intVec.InBounds())
				{
					if (DebugViewSettings.drawRegions)
					{
						stringBuilder.AppendLine("---");
						Region regionAt_InvalidAllowed = Find.RegionGrid.GetRegionAt_InvalidAllowed(intVec);
						stringBuilder.AppendLine("Region:\n" + ((regionAt_InvalidAllowed == null) ? "null" : regionAt_InvalidAllowed.DebugString));
					}
					if (DebugViewSettings.drawRooms)
					{
						stringBuilder.AppendLine("---");
						Room room = RoomQuery.RoomAt(intVec);
						if (room != null)
						{
							stringBuilder.AppendLine(room.DebugString());
						}
						else
						{
							stringBuilder.AppendLine("(no room)");
						}
					}
					if (DebugViewSettings.drawGlow)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("Psych glow: " + Find.GlowGrid.PsychGlowAt(intVec));
						stringBuilder.AppendLine("GridGlow: " + Find.GlowGrid.VisualGlowAt(intVec));
						stringBuilder.AppendLine("GlowReport:\n" + ((SectionLayer_LightingOverlay)Find.Map.mapDrawer.SectionAt(intVec).GetLayer(typeof(SectionLayer_LightingOverlay))).GlowReportAt(intVec));
						stringBuilder.AppendLine("SkyManager.CurSkyGlow: " + SkyManager.CurSkyGlow);
					}
					if (DebugViewSettings.writePathCosts)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("Perceived path cost: " + Find.PathGrid.PerceivedPathCostAt(intVec));
						stringBuilder.AppendLine("Real path cost: " + PathGrid.CalculatedCostAt(intVec, false, IntVec3.Invalid));
					}
					if (DebugViewSettings.writeFertility)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("\nFertility: " + Find.FertilityGrid.FertilityAt(intVec).ToString("##0.00"));
					}
					if (DebugViewSettings.writeLinkFlags)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("\nLinkFlags: ");
						foreach (object current11 in Enum.GetValues(typeof(LinkFlags)))
						{
							if ((LinkGrid.LinkFlagsAt(intVec) & (LinkFlags)((int)current11)) != LinkFlags.None)
							{
								stringBuilder.Append(" " + current11);
							}
						}
					}
					if (DebugViewSettings.writeSkyManager)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine(SkyManager.DebugString());
					}
					if (DebugViewSettings.writeCover)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.Append("Cover: ");
						Thing thing2 = Find.CoverGrid[intVec];
						if (thing2 == null)
						{
							stringBuilder.AppendLine("null");
						}
						else
						{
							stringBuilder.AppendLine(thing2.ToString());
						}
					}
					if (DebugViewSettings.drawPower)
					{
						stringBuilder.AppendLine("---");
						foreach (Thing current12 in Find.ThingGrid.ThingsAt(intVec))
						{
							ThingWithComps thingWithComps = current12 as ThingWithComps;
							if (thingWithComps != null && thingWithComps.GetComp<CompPowerTrader>() != null)
							{
								stringBuilder.AppendLine(" " + thingWithComps.GetComp<CompPowerTrader>().DebugString);
							}
						}
						PowerNet powerNet = PowerNetGrid.TransmittedPowerNetAt(intVec);
						if (powerNet != null)
						{
							stringBuilder.AppendLine(string.Empty + powerNet.DebugString());
						}
						else
						{
							stringBuilder.AppendLine("(no PowerNet here)");
						}
					}
					if (DebugViewSettings.drawPreyInfo)
					{
						Pawn pawn2 = Find.Selector.SingleSelectedThing as Pawn;
						if (pawn2 != null)
						{
							List<Thing> thingList = intVec.GetThingList();
							for (int j = 0; j < thingList.Count; j++)
							{
								Pawn pawn3 = thingList[j] as Pawn;
								if (pawn3 != null)
								{
									stringBuilder.AppendLine("---");
									if (FoodUtility.IsAcceptablePreyFor(pawn2, pawn3))
									{
										stringBuilder.AppendLine("Prey score: " + FoodUtility.GetPreyScoreFor(pawn2, pawn3));
									}
									else
									{
										stringBuilder.AppendLine("Prey score: None");
									}
									break;
								}
							}
						}
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
