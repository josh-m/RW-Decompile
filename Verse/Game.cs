using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public class Game : IExposable
	{
		private GameInitData initData;

		private GameInfo info = new GameInfo();

		private GameRules rules = new GameRules();

		private Scenario scenarioInt;

		private World worldInt;

		private Map mapInt;

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
				WorldRenderModeDatabase.Reset();
				this.worldInt = value;
			}
		}

		public Map Map
		{
			get
			{
				return this.mapInt;
			}
			set
			{
				this.mapInt = value;
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

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Log.Error("You must use special LoadData method to load Game.");
				return;
			}
			this.ExposeSmallComponents();
			Scribe_Deep.LookDeep<World>(ref this.worldInt, "world", new object[0]);
			Scribe_Deep.LookDeep<Map>(ref this.mapInt, "map", new object[0]);
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

		public void LoadData()
		{
			this.ExposeSmallComponents();
			LongEventHandler.SetCurrentEventText("LoadingWorld".Translate());
			Scribe.EnterNode("world");
			Current.Game.World = new World();
			Find.World.ExposeData();
			Scribe.ExitNode();
			LongEventHandler.SetCurrentEventText("LoadingMap".Translate());
			this.mapInt = new Map();
			List<Thing> list = null;
			Scribe.EnterNode("map");
			Scribe_Deep.LookDeep<MapInfo>(ref Find.Map.info, "mapInfo", new object[0]);
			MapIniterUtility.ReinitStaticMapComponents_PreConstruct();
			Find.Map.ConstructComponents();
			MapIniterUtility.ReinitStaticMapComponents_PostConstruct();
			Find.Map.ExposeComponents();
			DeepProfiler.Start("Load compressed things");
			MapFileCompressor mapFileCompressor = new MapFileCompressor();
			mapFileCompressor.ExposeData();
			DeepProfiler.End();
			DeepProfiler.Start("Load non-compressed things");
			Scribe_Collections.LookList<Thing>(ref list, "things", LookMode.Deep, new object[0]);
			DeepProfiler.End();
			Scribe.ExitNode();
			Scribe.FinalizeLoading();
			LongEventHandler.SetCurrentEventText("InitializingMap".Translate());
			DeepProfiler.Start("ResolveAllCrossReferences");
			CrossRefResolver.ResolveAllCrossReferences();
			DeepProfiler.End();
			DeepProfiler.Start("DoAllPostLoadInits");
			PostLoadInitter.DoAllPostLoadInits();
			DeepProfiler.End();
			LongEventHandler.SetCurrentEventText("SpawningAllThings".Translate());
			List<Thing> list2 = mapFileCompressor.ThingsToSpawnAfterLoad().ToList<Thing>();
			DeepProfiler.Start("Merge compressed and non-compressed thing lists");
			List<Thing> list3 = new List<Thing>(list.Count + list2.Count);
			foreach (Thing current in list.Concat(list2))
			{
				list3.Add(current);
			}
			DeepProfiler.End();
			DeepProfiler.Start("Spawn everything into the map");
			foreach (Thing current2 in list3)
			{
				try
				{
					GenSpawn.Spawn(current2, current2.Position, current2.Rotation);
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Exception spawning loaded thing ",
						current2,
						": ",
						ex
					}));
				}
			}
			DeepProfiler.End();
			MapIniterUtility.FinalizeMapInit();
		}

		public void Update()
		{
			this.tickManager.TickManagerUpdate();
			this.letterStack.LettersUpdate();
			this.Map.MapUpdate();
			this.Info.GameInfoUpdate();
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
			stringBuilder.AppendLine("Map:");
			if (this.mapInt == null)
			{
				stringBuilder.AppendLine("   null");
			}
			else
			{
				stringBuilder.AppendLine("   worldSquare: " + this.mapInt.WorldSquare);
			}
			return stringBuilder.ToString();
		}
	}
}
