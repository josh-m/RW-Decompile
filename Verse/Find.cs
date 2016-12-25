using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using Verse.Sound;

namespace Verse
{
	public static class Find
	{
		public static Root Root
		{
			get
			{
				return Current.Root;
			}
		}

		public static SoundRoot SoundRoot
		{
			get
			{
				return Current.Root.soundRoot;
			}
		}

		public static UIRoot UIRoot
		{
			get
			{
				return Current.Root.uiRoot;
			}
		}

		public static MusicManagerEntry MusicManagerEntry
		{
			get
			{
				return ((Root_Entry)Current.Root).musicManagerEntry;
			}
		}

		public static MusicManagerPlay MusicManagerPlay
		{
			get
			{
				return ((Root_Play)Current.Root).musicManagerPlay;
			}
		}

		public static WindowStack WindowStack
		{
			get
			{
				return Find.UIRoot.windows;
			}
		}

		public static LanguageWorker ActiveLanguageWorker
		{
			get
			{
				return LanguageDatabase.activeLanguage.Worker;
			}
		}

		public static Camera Camera
		{
			get
			{
				return Current.Camera;
			}
		}

		public static CameraDriver CameraDriver
		{
			get
			{
				return Current.CameraDriver;
			}
		}

		public static ColorCorrectionCurves CameraColor
		{
			get
			{
				return Current.ColorCorrectionCurves;
			}
		}

		public static Camera PortraitCamera
		{
			get
			{
				return PortraitCameraManager.PortraitCamera;
			}
		}

		public static PortraitRenderer PortraitRenderer
		{
			get
			{
				return PortraitCameraManager.PortraitRenderer;
			}
		}

		public static Camera WorldCamera
		{
			get
			{
				return WorldCameraManager.WorldCamera;
			}
		}

		public static WorldCameraDriver WorldCameraDriver
		{
			get
			{
				return WorldCameraManager.WorldCameraDriver;
			}
		}

		public static MainTabsRoot MainTabsRoot
		{
			get
			{
				return ((UIRoot_Play)Find.UIRoot).mainTabsRoot;
			}
		}

		public static MapInterface MapUI
		{
			get
			{
				return ((UIRoot_Play)Find.UIRoot).mapUI;
			}
		}

		public static Selector Selector
		{
			get
			{
				return Find.MapUI.selector;
			}
		}

		public static Targeter Targeter
		{
			get
			{
				return Find.MapUI.targeter;
			}
		}

		public static ColonistBar ColonistBar
		{
			get
			{
				return Find.MapUI.colonistBar;
			}
		}

		public static DesignatorManager DesignatorManager
		{
			get
			{
				return Find.MapUI.designatorManager;
			}
		}

		public static ReverseDesignatorDatabase ReverseDesignatorDatabase
		{
			get
			{
				return Find.MapUI.reverseDesignatorDatabase;
			}
		}

		public static GameInitData GameInitData
		{
			get
			{
				return (Current.Game == null) ? null : Current.Game.InitData;
			}
		}

		public static GameInfo GameInfo
		{
			get
			{
				return Current.Game.Info;
			}
		}

		public static Scenario Scenario
		{
			get
			{
				if (Current.Game != null && Current.Game.Scenario != null)
				{
					return Current.Game.Scenario;
				}
				if (ScenarioMaker.GeneratingScenario != null)
				{
					return ScenarioMaker.GeneratingScenario;
				}
				if (Find.UIRoot != null)
				{
					Page_ScenarioEditor page_ScenarioEditor = Find.WindowStack.WindowOfType<Page_ScenarioEditor>();
					if (page_ScenarioEditor != null)
					{
						return page_ScenarioEditor.EditingScenario;
					}
				}
				return null;
			}
		}

		public static World World
		{
			get
			{
				return (Current.Game == null || Current.Game.World == null) ? Current.CreatingWorld : Current.Game.World;
			}
		}

		public static List<Map> Maps
		{
			get
			{
				return Current.Game.Maps;
			}
		}

		public static Map VisibleMap
		{
			get
			{
				return Current.Game.VisibleMap;
			}
		}

		public static Map AnyPlayerHomeMap
		{
			get
			{
				return Current.Game.AnyPlayerHomeMap;
			}
		}

		public static StoryWatcher StoryWatcher
		{
			get
			{
				return Current.Game.storyWatcher;
			}
		}

		public static ResearchManager ResearchManager
		{
			get
			{
				return Current.Game.researchManager;
			}
		}

		public static Storyteller Storyteller
		{
			get
			{
				if (Current.Game == null)
				{
					return null;
				}
				return Current.Game.storyteller;
			}
		}

		public static GameEnder GameEnder
		{
			get
			{
				return Current.Game.gameEnder;
			}
		}

		public static LetterStack LetterStack
		{
			get
			{
				return Current.Game.letterStack;
			}
		}

		public static PlaySettings PlaySettings
		{
			get
			{
				return Current.Game.playSettings;
			}
		}

		public static History History
		{
			get
			{
				return Current.Game.history;
			}
		}

		public static TaleManager TaleManager
		{
			get
			{
				return Current.Game.taleManager;
			}
		}

		public static PlayLog PlayLog
		{
			get
			{
				return Current.Game.playLog;
			}
		}

		public static TickManager TickManager
		{
			get
			{
				return Current.Game.tickManager;
			}
		}

		public static Tutor Tutor
		{
			get
			{
				if (Current.Game == null)
				{
					return null;
				}
				return Current.Game.tutor;
			}
		}

		public static TutorialState TutorialState
		{
			get
			{
				return Current.Game.tutor.tutorialState;
			}
		}

		public static ActiveLessonHandler ActiveLesson
		{
			get
			{
				if (Current.Game == null)
				{
					return null;
				}
				return Current.Game.tutor.activeLesson;
			}
		}

		public static Autosaver Autosaver
		{
			get
			{
				return Current.Game.autosaver;
			}
		}

		public static FactionManager FactionManager
		{
			get
			{
				return Find.World.factionManager;
			}
		}

		public static WorldPawns WorldPawns
		{
			get
			{
				return Find.World.worldPawns;
			}
		}

		public static UniqueIDsManager UniqueIDsManager
		{
			get
			{
				return Find.World.uniqueIDsManager;
			}
		}

		public static WorldObjectsHolder WorldObjects
		{
			get
			{
				return Find.World.worldObjects;
			}
		}

		public static WorldGrid WorldGrid
		{
			get
			{
				return Find.World.grid;
			}
		}

		public static WorldDebugDrawer WorldDebugDrawer
		{
			get
			{
				return Find.World.debugDrawer;
			}
		}

		public static WorldPathGrid WorldPathGrid
		{
			get
			{
				return Find.World.pathGrid;
			}
		}

		public static WorldDynamicDrawManager WorldDynamicDrawManager
		{
			get
			{
				return Find.World.dynamicDrawManager;
			}
		}

		public static WorldPathFinder WorldPathFinder
		{
			get
			{
				return Find.World.pathFinder;
			}
		}

		public static WorldPathPool WorldPathPool
		{
			get
			{
				return Find.World.pathPool;
			}
		}

		public static WorldReachability WorldReachability
		{
			get
			{
				return Find.World.reachability;
			}
		}

		public static WorldInterface WorldInterface
		{
			get
			{
				return Find.World.UI;
			}
		}

		public static WorldSelector WorldSelector
		{
			get
			{
				return Find.WorldInterface.selector;
			}
		}

		public static WorldTargeter WorldTargeter
		{
			get
			{
				return Find.WorldInterface.targeter;
			}
		}
	}
}
