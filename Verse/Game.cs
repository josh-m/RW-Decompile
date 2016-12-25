using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.Profile;

namespace Verse
{
	public class Game : IExposable
	{
		private GameInitData initData;

		public sbyte visibleMapIndex = -1;

		private GameInfo info = new GameInfo();

		private GameRules rules = new GameRules();

		private Scenario scenarioInt;

		private World worldInt;

		private List<Map> maps = new List<Map>();

		public PlaySettings playSettings = new PlaySettings();

		public StoryWatcher storyWatcher = new StoryWatcher();

		public LetterStack letterStack = new LetterStack();

		public ResearchManager researchManager = new ResearchManager();

		public GameEnder gameEnder = new GameEnder();

		public Storyteller storyteller = new Storyteller();

		public History history = new History();

		public TaleManager taleManager = new TaleManager();

		public PlayLog playLog = new PlayLog();

		public OutfitDatabase outfitDatabase = new OutfitDatabase();

		public DrugPolicyDatabase drugPolicyDatabase = new DrugPolicyDatabase();

		public TickManager tickManager = new TickManager();

		public Tutor tutor = new Tutor();

		public Autosaver autosaver = new Autosaver();

		public Scenario Scenario
		{
			get
			{
				return this.scenarioInt;
			}
			set
			{
				this.scenarioInt = value;
			}
		}

		public World World
		{
			get
			{
				return this.worldInt;
			}
			set
			{
				if (this.worldInt == value)
				{
					return;
				}
				this.worldInt = value;
			}
		}

		public Map VisibleMap
		{
			get
			{
				if ((int)this.visibleMapIndex < 0)
				{
					return null;
				}
				return this.maps[(int)this.visibleMapIndex];
			}
			set
			{
				int num;
				if (value == null)
				{
					num = -1;
				}
				else
				{
					num = this.maps.IndexOf(value);
					if (num < 0)
					{
						Log.Error("Could not set visible map because it does not exist.");
						return;
					}
				}
				if ((int)this.visibleMapIndex != num)
				{
					this.visibleMapIndex = (sbyte)num;
					Find.MapUI.Notify_SwitchedMap();
					AmbientSoundManager.Notify_SwitchedMap();
				}
			}
		}

		public Map AnyPlayerHomeMap
		{
			get
			{
				if (Faction.OfPlayerSilentFail == null)
				{
					return null;
				}
				for (int i = 0; i < this.maps.Count; i++)
				{
					Map map = this.maps[i];
					if (map.IsPlayerHome)
					{
						return map;
					}
				}
				return null;
			}
		}

		public List<Map> Maps
		{
			get
			{
				return this.maps;
			}
		}

		public GameInitData InitData
		{
			get
			{
				return this.initData;
			}
			set
			{
				this.initData = value;
			}
		}

		public GameInfo Info
		{
			get
			{
				return this.info;
			}
		}

		public GameRules Rules
		{
			get
			{
				return this.rules;
			}
		}

		public void AddMap(Map map)
		{
			if (map == null)
			{
				Log.Error("Tried to add null map.");
				return;
			}
			if (this.maps.Contains(map))
			{
				Log.Error("Tried to add map but it's already here.");
				return;
			}
			if (this.maps.Count > 127)
			{
				Log.Error("Can't add map. Reached maps count limit (" + 127 + ").");
				return;
			}
			this.maps.Add(map);
			Find.ColonistBar.MarkColonistsDirty();
		}

		public Map FindMap(MapParent mapParent)
		{
			for (int i = 0; i < this.maps.Count; i++)
			{
				if (this.maps[i].info.parent == mapParent)
				{
					return this.maps[i];
				}
			}
			return null;
		}

		public Map FindMap(int tile)
		{
			for (int i = 0; i < this.maps.Count; i++)
			{
				if (this.maps[i].info.tile == tile)
				{
					return this.maps[i];
				}
			}
			return null;
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Log.Error("You must use special LoadData method to load Game.");
				return;
			}
			Scribe_Values.LookValue<sbyte>(ref this.visibleMapIndex, "visibleMapIndex", -1, false);
			this.ExposeSmallComponents();
			Scribe_Deep.LookDeep<World>(ref this.worldInt, "world", new object[0]);
			Scribe_Collections.LookList<Map>(ref this.maps, "maps", LookMode.Deep, new object[0]);
			Find.CameraDriver.Expose();
		}

		private void ExposeSmallComponents()
		{
			Scribe_Deep.LookDeep<GameInfo>(ref this.info, "info", new object[0]);
			Scribe_Deep.LookDeep<GameRules>(ref this.rules, "rules", new object[0]);
			Scribe_Deep.LookDeep<Scenario>(ref this.scenarioInt, "scenario", new object[0]);
			Scribe_Deep.LookDeep<TickManager>(ref this.tickManager, "tickManager", new object[0]);
			Scribe_Deep.LookDeep<PlaySettings>(ref this.playSettings, "playSettings", new object[0]);
			Scribe_Deep.LookDeep<StoryWatcher>(ref this.storyWatcher, "storyWatcher", new object[0]);
			Scribe_Deep.LookDeep<GameEnder>(ref this.gameEnder, "gameEnder", new object[0]);
			Scribe_Deep.LookDeep<LetterStack>(ref this.letterStack, "letterStack", new object[0]);
			Scribe_Deep.LookDeep<ResearchManager>(ref this.researchManager, "researchManager", new object[0]);
			Scribe_Deep.LookDeep<Storyteller>(ref this.storyteller, "storyteller", new object[0]);
			Scribe_Deep.LookDeep<History>(ref this.history, "history", new object[0]);
			Scribe_Deep.LookDeep<TaleManager>(ref this.taleManager, "taleManager", new object[0]);
			Scribe_Deep.LookDeep<PlayLog>(ref this.playLog, "playLog", new object[0]);
			Scribe_Deep.LookDeep<OutfitDatabase>(ref this.outfitDatabase, "outfitDatabase", new object[0]);
			Scribe_Deep.LookDeep<DrugPolicyDatabase>(ref this.drugPolicyDatabase, "drugPolicyDatabase", new object[0]);
			Scribe_Deep.LookDeep<Tutor>(ref this.tutor, "tutor", new object[0]);
		}

		public void InitNewGame()
		{
			string str = GenText.ToCommaList(from mod in LoadedModManager.RunningMods
			select mod.ToString(), true);
			Log.Message("Initializing new game with mods " + str);
			if (this.maps.Any<Map>())
			{
				Log.Error("Called InitNewGame() but there already is a map. There should be 0 maps...");
				return;
			}
			if (this.initData == null)
			{
				Log.Error("Called InitNewGame() but init data is null. Create it first.");
				return;
			}
			MemoryUtility.UnloadUnusedUnityAssets();
			DeepProfiler.Start("InitNewGame");
			Current.ProgramState = ProgramState.MapInitializing;
			IntVec3 intVec = new IntVec3(this.initData.mapSize, 1, this.initData.mapSize);
			FactionBase factionBase = null;
			List<FactionBase> factionBases = Find.WorldObjects.FactionBases;
			for (int i = 0; i < factionBases.Count; i++)
			{
				if (factionBases[i].Faction == Faction.OfPlayer)
				{
					factionBase = factionBases[i];
					break;
				}
			}
			if (factionBase == null)
			{
				Log.Error("Could not generate starting map because there is no any player faction base.");
			}
			Map visibleMap = MapGenerator.GenerateMap(intVec, this.initData.startingTile, factionBase, delegate(Map generatedMap)
			{
				if (this.initData.startingMonth == Month.Undefined)
				{
					this.initData.startingMonth = generatedMap.mapTemperature.EarliestMonthInTemperatureRange(16f, 9999f);
					if (this.initData.startingMonth == Month.Undefined)
					{
						this.initData.startingMonth = Month.Jun;
					}
				}
				this.tickManager.gameStartAbsTick = GenTicks.ConfiguredTicksAbsAtGameStart;
			}, null);
			this.worldInt.info.initialMapSize = intVec;
			if (this.initData.permadeath)
			{
				this.info.permadeathMode = true;
				this.info.permadeathModeUniqueName = PermadeathModeUtility.GeneratePermadeathSaveName();
			}
			PawnUtility.GiveAllStartingPlayerPawnsThought(ThoughtDefOf.NewColonyOptimism);
			this.FinalizeInit();
			Current.Game.VisibleMap = visibleMap;
			Find.CameraDriver.JumpTo(MapGenerator.PlayerStartSpot);
			Find.CameraDriver.ResetSize();
			if (Prefs.PauseOnLoad && this.initData.startedFromEntry)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					this.tickManager.DoSingleTick();
					this.tickManager.CurTimeSpeed = TimeSpeed.Paused;
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
							this.researchManager.InstantFinish(current2, false);
						}
					}
				}
			}
			this.initData = null;
		}

		public void LoadGame()
		{
			if (this.maps.Any<Map>())
			{
				Log.Error("Called LoadGame() but there already is a map. There should be 0 maps...");
				return;
			}
			MemoryUtility.UnloadUnusedUnityAssets();
			Current.ProgramState = ProgramState.MapInitializing;
			this.ExposeSmallComponents();
			LongEventHandler.SetCurrentEventText("LoadingWorld".Translate());
			if (Scribe.EnterNode("world"))
			{
				this.World = new World();
				this.World.ExposeData();
				Scribe.ExitNode();
				this.World.FinalizeInit();
				LongEventHandler.SetCurrentEventText("LoadingMap".Translate());
				Scribe_Collections.LookList<Map>(ref this.maps, "maps", LookMode.Deep, new object[0]);
				int num = -1;
				Scribe_Values.LookValue<int>(ref num, "visibleMapIndex", -1, false);
				if (num < 0 && this.maps.Any<Map>())
				{
					Log.Error("Visible map is null after loading but there are maps available. Setting visible map to [0].");
					num = 0;
				}
				if (num >= this.maps.Count)
				{
					Log.Error("Visible map index out of bounds after loading.");
					if (this.maps.Any<Map>())
					{
						num = 0;
					}
					else
					{
						num = -1;
					}
				}
				this.visibleMapIndex = -128;
				this.VisibleMap = ((num < 0) ? null : this.maps[num]);
				LongEventHandler.SetCurrentEventText("InitializingGame".Translate());
				Find.CameraDriver.Expose();
				Scribe.FinalizeLoading();
				DeepProfiler.Start("ResolveAllCrossReferences");
				CrossRefResolver.ResolveAllCrossReferences();
				DeepProfiler.End();
				DeepProfiler.Start("DoAllPostLoadInits");
				PostLoadInitter.DoAllPostLoadInits();
				DeepProfiler.End();
				LongEventHandler.SetCurrentEventText("SpawningAllThings".Translate());
				for (int i = 0; i < this.maps.Count; i++)
				{
					this.maps[i].FinalizeLoading();
				}
				this.FinalizeInit();
				if (Prefs.PauseOnLoad)
				{
					LongEventHandler.ExecuteWhenFinished(delegate
					{
						Find.TickManager.DoSingleTick();
						Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
					});
				}
				return;
			}
			Log.Error("Could not find world XML node.");
		}

		public void Update()
		{
			this.tickManager.TickManagerUpdate();
			this.letterStack.LettersUpdate();
			this.World.WorldUpdate();
			for (int i = 0; i < this.maps.Count; i++)
			{
				this.maps[i].MapUpdate();
			}
			this.Info.GameInfoUpdate();
		}

		public void FinalizeInit()
		{
			LogSimple.FlushToFileAndOpen();
			this.researchManager.ReapplyAllMods();
			MessagesRepeatAvoider.Reset();
			Current.ProgramState = ProgramState.Playing;
		}

		public void DeinitAndRemoveMap(Map map)
		{
			if (map == null)
			{
				Log.Error("Tried to remove null map.");
				return;
			}
			if (!this.maps.Contains(map))
			{
				Log.Error("Tried to remove map " + map + " but it's not here.");
				return;
			}
			bool flag = map.ParentFaction != null && map.ParentFaction.HostileTo(Faction.OfPlayer);
			List<Pawn> list = map.mapPawns.AllPawns.ToList<Pawn>();
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					Pawn pawn = list[i];
					if (pawn.Spawned)
					{
						pawn.DeSpawn();
					}
					if (pawn.IsColonist && flag)
					{
						map.ParentFaction.kidnapped.KidnapPawn(pawn, null);
					}
					else
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					}
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not despawn and pass to world ",
						list[i],
						": ",
						ex
					}));
				}
			}
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int j = 0; j < allFactionsListForReading.Count; j++)
			{
				allFactionsListForReading[j].Notify_MapRemoved(map);
			}
			this.tickManager.RemoveAllFromMap(map);
			Map visibleMap = this.VisibleMap;
			int num = this.maps.IndexOf(map);
			for (int k = num; k < this.maps.Count; k++)
			{
				if (k == num)
				{
					RealTime.moteList.RemoveAllFromMap(this.maps[k]);
				}
				else
				{
					List<Mote> allMotes = RealTime.moteList.allMotes;
					for (int l = 0; l < allMotes.Count; l++)
					{
						if (allMotes[l].Map == this.maps[k])
						{
							allMotes[l].DecrementMapIndex();
						}
					}
				}
				List<Thing> allThings = this.maps[k].listerThings.AllThings;
				for (int m = 0; m < allThings.Count; m++)
				{
					if (k == num)
					{
						allThings[m].Notify_MyMapRemoved();
					}
					else
					{
						allThings[m].DecrementMapIndex();
					}
				}
				List<Room> allRooms = this.maps[k].regionGrid.allRooms;
				for (int n = 0; n < allRooms.Count; n++)
				{
					if (k == num)
					{
						allRooms[n].Notify_MyMapRemoved();
					}
					else
					{
						allRooms[n].DecrementMapIndex();
					}
				}
				foreach (Region current in this.maps[k].regionGrid.AllRegions)
				{
					if (k == num)
					{
						current.Notify_MyMapRemoved();
					}
					else
					{
						current.DecrementMapIndex();
					}
				}
			}
			this.maps.Remove(map);
			if (visibleMap != null)
			{
				sbyte b = (sbyte)this.maps.IndexOf(visibleMap);
				if ((int)b < 0)
				{
					if (this.maps.Any<Map>())
					{
						this.VisibleMap = this.maps[0];
					}
					else
					{
						this.VisibleMap = null;
					}
				}
				else
				{
					this.visibleMapIndex = b;
				}
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Game debug data:");
			stringBuilder.AppendLine("initData:");
			if (this.initData == null)
			{
				stringBuilder.AppendLine("   null");
			}
			else
			{
				stringBuilder.AppendLine(this.initData.ToString());
			}
			stringBuilder.AppendLine("Scenario:");
			if (this.scenarioInt == null)
			{
				stringBuilder.AppendLine("   null");
			}
			else
			{
				stringBuilder.AppendLine("   " + this.scenarioInt.ToString());
			}
			stringBuilder.AppendLine("World:");
			if (this.worldInt == null)
			{
				stringBuilder.AppendLine("   null");
			}
			else
			{
				stringBuilder.AppendLine("   name: " + this.worldInt.info.name);
			}
			stringBuilder.AppendLine("Maps count: " + this.maps.Count);
			for (int i = 0; i < this.maps.Count; i++)
			{
				stringBuilder.AppendLine("   Map " + this.maps[i].Index + ":");
				stringBuilder.AppendLine("      tile: " + this.maps[i].TileInfo);
			}
			return stringBuilder.ToString();
		}
	}
}
