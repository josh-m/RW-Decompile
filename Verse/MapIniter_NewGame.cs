using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Verse
{
	public static class MapIniter_NewGame
	{
		public static void InitNewGeneratedMap()
		{
			string str = GenText.ToCommaList(from mod in LoadedModManager.RunningMods
			select mod.ToString(), true);
			Log.Message("Initializing new game with mods " + str);
			DeepProfiler.Start("InitNewGeneratedMap");
			if (Find.GameInitData == null)
			{
				MapIniter_NewGame.SetupForQuickTestPlay();
			}
			DeepProfiler.Start("Set up map");
			Current.ProgramState = ProgramState.MapInitializing;
			Current.Game.Map = new Map();
			Find.Map.info.Size = new IntVec3(Find.GameInitData.mapSize, 1, Find.GameInitData.mapSize);
			Find.Map.info.worldCoords = Find.GameInitData.startingCoords;
			if (Find.GameInitData.permadeath)
			{
				Current.Game.Info.permadeathMode = true;
				Current.Game.Info.permadeathModeUniqueName = PermadeathModeUtility.GeneratePermadeathSaveName();
			}
			MapIniterUtility.ReinitStaticMapComponents_PreConstruct();
			Find.Map.ConstructComponents();
			MapIniterUtility.ReinitStaticMapComponents_PostConstruct();
			if (Find.GameInitData.startingMonth == Month.Undefined)
			{
				Find.GameInitData.startingMonth = GenTemperature.EarliestMonthInTemperatureRange(16f, 9999f);
				if (Find.GameInitData.startingMonth == Month.Undefined)
				{
					Find.GameInitData.startingMonth = Month.Jun;
				}
			}
			Find.TickManager.gameStartAbsTick = GenTicks.ConfiguredTicksAbsAtGameStart;
			DeepProfiler.End();
			DeepProfiler.Start("Generate contents into map");
			MapGenerator.GenerateContentsIntoCurrentMap(DefDatabase<MapGeneratorDef>.GetRandom());
			DeepProfiler.End();
			Find.Scenario.PostMapGenerate();
			Find.AreaManager.InitForNewGame();
			DeepProfiler.Start("Finalize map init");
			MapIniterUtility.FinalizeMapInit();
			DeepProfiler.End();
			DeepProfiler.End();
			Find.CameraDriver.JumpTo(MapGenerator.PlayerStartSpot);
			if (Prefs.PauseOnLoad && Find.GameInitData.startedFromEntry)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					Find.TickManager.DoSingleTick();
					Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
				});
			}
			Find.Scenario.PostGameStart();
			if (Faction.OfPlayer.def.startingResearchTags != null)
			{
				foreach (string current in Faction.OfPlayer.def.startingResearchTags)
				{
					foreach (ResearchProjectDef current2 in DefDatabase<ResearchProjectDef>.AllDefs)
					{
						if (current2.HasTag(current))
						{
							Find.ResearchManager.InstantFinish(current2, false);
						}
					}
				}
			}
			MapIniter_NewGame.GiveAllPlayerPawnsThought(ThoughtDefOf.NewColonyOptimism);
			Current.Game.InitData = null;
		}

		private static bool TryLoadNewestWorld()
		{
			FileInfo fileInfo = (from wf in GenFilePaths.AllWorldFiles
			orderby wf.LastWriteTime descending
			select wf).FirstOrDefault<FileInfo>();
			if (fileInfo == null)
			{
				return false;
			}
			string fullName = fileInfo.FullName;
			GameDataSaveLoader.LoadWorldFromFileIntoGame(fullName);
			return true;
		}

		private static void SetupForQuickTestPlay()
		{
			Current.ProgramState = ProgramState.Entry;
			Current.Game = new Game();
			Current.Game.InitData = new GameInitData();
			Current.Game.Scenario = ScenarioDefOf.Crashlanded.scenario;
			Find.Scenario.PreConfigure();
			Current.Game.storyteller = new Storyteller(StorytellerDefOf.Cassandra, DifficultyDefOf.Hard);
			if (!MapIniter_NewGame.TryLoadNewestWorld())
			{
				Current.Game.World = WorldGenerator.GenerateWorld(WorldGenerator.DefaultSize, GenText.RandomSeedString());
			}
			Rand.RandomizeSeedFromTime();
			Find.Scenario.PostWorldLoad();
			Find.GameInitData.ChooseRandomStartingWorldSquare();
			Find.GameInitData.mapSize = 150;
			MapIniter_NewGame.PrepForMapGen();
			Find.Scenario.PreMapGenerate();
		}

		public static void PrepForMapGen()
		{
			foreach (Pawn current in Find.GameInitData.startingPawns)
			{
				current.SetFactionDirect(Faction.OfPlayer);
				PawnComponentsUtility.AddAndRemoveDynamicComponents(current, false);
			}
			foreach (Pawn current2 in Find.GameInitData.startingPawns)
			{
				current2.workSettings.DisableAll();
			}
			foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
			{
				if (w.alwaysStartActive)
				{
					foreach (Pawn current3 in from col in Find.GameInitData.startingPawns
					where !col.story.WorkTypeIsDisabled(w)
					select col)
					{
						current3.workSettings.SetPriority(w, 3);
					}
				}
				else
				{
					bool flag = false;
					foreach (Pawn current4 in Find.GameInitData.startingPawns)
					{
						if (!current4.story.WorkTypeIsDisabled(w) && current4.skills.AverageOfRelevantSkillsFor(w) >= 6f)
						{
							current4.workSettings.SetPriority(w, 3);
							flag = true;
						}
					}
					if (!flag)
					{
						IEnumerable<Pawn> source = from col in Find.GameInitData.startingPawns
						where !col.story.WorkTypeIsDisabled(w)
						select col;
						if (source.Any<Pawn>())
						{
							Pawn pawn = source.InRandomOrder(null).MaxBy((Pawn c) => c.skills.AverageOfRelevantSkillsFor(w));
							pawn.workSettings.SetPriority(w, 3);
						}
						else if (w.requireCapableColonist)
						{
							Log.Error("No colonist could do requireCapableColonist work type " + w);
						}
					}
				}
			}
		}

		public static void GiveAllPlayerPawnsThought(ThoughtDef thought)
		{
			foreach (Pawn current in Find.GameInitData.startingPawns)
			{
				if (thought.IsSocial)
				{
					foreach (Pawn current2 in Find.GameInitData.startingPawns)
					{
						if (current2 != current)
						{
							current.needs.mood.thoughts.memories.TryGainMemoryThought(thought, current2);
						}
					}
				}
				else
				{
					current.needs.mood.thoughts.memories.TryGainMemoryThought(thought, null);
				}
			}
		}
	}
}
