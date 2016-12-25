using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using Verse.AI;
using Verse.AI.Group;
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

		public static MainTabsRoot MainTabsRoot
		{
			get
			{
				return ((UIRootMap)Find.UIRoot).mainTabsRoot;
			}
		}

		public static Selector Selector
		{
			get
			{
				return ((UIRootMap)Find.UIRoot).selector;
			}
		}

		public static Targeter Targeter
		{
			get
			{
				return ((UIRootMap)Find.UIRoot).targeter;
			}
		}

		public static ColonistBar ColonistBar
		{
			get
			{
				return ((UIRootMap)Find.UIRoot).colonistBar;
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

		public static Map Map
		{
			get
			{
				return Current.Game.Map;
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
				return Current.Game.tutor.activeLesson;
			}
		}

		public static WorldSquare MapWorldSquare
		{
			get
			{
				return Find.World.grid.Get(Find.GameInitData.startingCoords);
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

		public static ResourceCounter ResourceCounter
		{
			get
			{
				return Find.Map.resourceCounter;
			}
		}

		public static ListerThings ListerThings
		{
			get
			{
				return Find.Map.listerThings;
			}
		}

		public static ListerBuildings ListerBuildings
		{
			get
			{
				return Find.Map.listerBuildings;
			}
		}

		public static DynamicDrawManager DynamicDrawManager
		{
			get
			{
				return Find.Map.dynamicDrawManager;
			}
		}

		public static PawnDestinationManager PawnDestinationManager
		{
			get
			{
				return Find.Map.pawnDestinationManager;
			}
		}

		public static TooltipGiverList TooltipGiverList
		{
			get
			{
				return Find.Map.tooltipGiverList;
			}
		}

		public static ReservationManager Reservations
		{
			get
			{
				return Find.Map.reservationManager;
			}
		}

		public static PhysicalInteractionReservationManager PhysicalInteractionReservations
		{
			get
			{
				return Find.Map.physicalInteractionReservationManager;
			}
		}

		public static DesignationManager DesignationManager
		{
			get
			{
				return Find.Map.designationManager;
			}
		}

		public static LordManager LordManager
		{
			get
			{
				return Find.Map.lordManager;
			}
		}

		public static DebugCellDrawer DebugDrawer
		{
			get
			{
				return Find.Map.debugDrawer;
			}
		}

		public static PassingShipManager PassingShipManager
		{
			get
			{
				return Find.Map.passingShipManager;
			}
		}

		public static SlotGroupManager SlotGroupManager
		{
			get
			{
				return Find.Map.slotGroupManager;
			}
		}

		public static MapDrawer MapDrawer
		{
			get
			{
				return Find.Map.mapDrawer;
			}
		}

		public static MapConditionManager MapConditionManager
		{
			get
			{
				return Find.Map.mapConditionManager;
			}
		}

		public static WeatherManager WeatherManager
		{
			get
			{
				return Find.Map.weatherManager;
			}
		}

		public static ZoneManager ZoneManager
		{
			get
			{
				return Find.Map.zoneManager;
			}
		}

		public static MusicManagerMap MusicManagerMap
		{
			get
			{
				return Find.Map.musicManagerMap;
			}
		}

		public static MapPawns MapPawns
		{
			get
			{
				return Find.Map.mapPawns;
			}
		}

		public static AttackTargetsCache AttackTargetsCache
		{
			get
			{
				return Find.Map.attackTargetsCache;
			}
		}

		public static AttackTargetReservationManager AttackTargetReservations
		{
			get
			{
				return Find.Map.attackTargetReservationManager;
			}
		}

		public static VoluntarilyJoinableLordsStarter VoluntarilyJoinableLordsStarter
		{
			get
			{
				return Find.Map.lordsStarter;
			}
		}

		public static ThingGrid ThingGrid
		{
			get
			{
				return Find.Map.thingGrid;
			}
		}

		public static EdificeGrid EdificeGrid
		{
			get
			{
				return Find.Map.buildingGrid;
			}
		}

		public static CoverGrid CoverGrid
		{
			get
			{
				return Find.Map.coverGrid;
			}
		}

		public static FogGrid FogGrid
		{
			get
			{
				return Find.Map.fogGrid;
			}
		}

		public static GlowGrid GlowGrid
		{
			get
			{
				return Find.Map.glowGrid;
			}
		}

		public static SnowGrid SnowGrid
		{
			get
			{
				return Find.Map.snowGrid;
			}
		}

		public static RegionGrid RegionGrid
		{
			get
			{
				return Find.Map.regionGrid;
			}
		}

		public static TerrainGrid TerrainGrid
		{
			get
			{
				return Find.Map.terrainGrid;
			}
		}

		public static PathGrid PathGrid
		{
			get
			{
				return Find.Map.pathGrid;
			}
		}

		public static RoofGrid RoofGrid
		{
			get
			{
				return Find.Map.roofGrid;
			}
		}

		public static FertilityGrid FertilityGrid
		{
			get
			{
				return Find.Map.fertilityGrid;
			}
		}

		public static DeepResourceGrid DeepResourceGrid
		{
			get
			{
				return Find.Map.deepResourceGrid;
			}
		}

		public static AreaManager AreaManager
		{
			get
			{
				return Find.Map.areaManager;
			}
		}

		public static Area_Home AreaHome
		{
			get
			{
				return Find.Map.areaManager.Get<Area_Home>();
			}
		}

		public static Area_BuildRoof AreaBuildRoof
		{
			get
			{
				return Find.Map.areaManager.Get<Area_BuildRoof>();
			}
		}

		public static Area_NoRoof AreaNoRoof
		{
			get
			{
				return Find.Map.areaManager.Get<Area_NoRoof>();
			}
		}

		public static Area_SnowClear AreaSnowClear
		{
			get
			{
				return Find.Map.areaManager.Get<Area_SnowClear>();
			}
		}
	}
}
