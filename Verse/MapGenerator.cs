using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class MapGenerator
	{
		public static Map mapBeingGenerated;

		private static Dictionary<string, object> data = new Dictionary<string, object>();

		private static IntVec3 playerStartSpotInt = IntVec3.Invalid;

		public static List<IntVec3> rootsToUnfog = new List<IntVec3>();

		public const string ElevationName = "Elevation";

		public const string FertilityName = "Fertility";

		public const string CavesName = "Caves";

		public const string RectOfInterestName = "RectOfInterest";

		public static MapGenFloatGrid Elevation
		{
			get
			{
				return MapGenerator.FloatGridNamed("Elevation");
			}
		}

		public static MapGenFloatGrid Fertility
		{
			get
			{
				return MapGenerator.FloatGridNamed("Fertility");
			}
		}

		public static MapGenFloatGrid Caves
		{
			get
			{
				return MapGenerator.FloatGridNamed("Caves");
			}
		}

		public static IntVec3 PlayerStartSpot
		{
			get
			{
				if (!MapGenerator.playerStartSpotInt.IsValid)
				{
					Log.Error("Accessing player start spot before setting it.");
					return IntVec3.Zero;
				}
				return MapGenerator.playerStartSpotInt;
			}
			set
			{
				MapGenerator.playerStartSpotInt = value;
			}
		}

		public static Map GenerateMap(IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepDef> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
		{
			ProgramState programState = Current.ProgramState;
			Current.ProgramState = ProgramState.MapInitializing;
			MapGenerator.playerStartSpotInt = IntVec3.Invalid;
			MapGenerator.rootsToUnfog.Clear();
			MapGenerator.data.Clear();
			MapGenerator.mapBeingGenerated = null;
			Map result;
			try
			{
				DeepProfiler.Start("InitNewGeneratedMap");
				if (parent != null && parent.HasMap)
				{
					Log.Error("Tried to generate a new map and set " + parent + " as its parent, but this world object already has a map. One world object can't have more than 1 map.");
					parent = null;
				}
				DeepProfiler.Start("Set up map");
				Map map = new Map();
				map.uniqueID = Find.UniqueIDsManager.GetNextMapID();
				MapGenerator.mapBeingGenerated = map;
				map.info.Size = mapSize;
				map.info.parent = parent;
				map.ConstructComponents();
				DeepProfiler.End();
				Current.Game.AddMap(map);
				if (extraInitBeforeContentGen != null)
				{
					extraInitBeforeContentGen(map);
				}
				if (mapGenerator == null)
				{
					mapGenerator = DefDatabase<MapGeneratorDef>.AllDefsListForReading.RandomElementByWeight((MapGeneratorDef x) => x.selectionWeight);
				}
				IEnumerable<GenStepDef> enumerable = mapGenerator.GenSteps;
				if (extraGenStepDefs != null)
				{
					enumerable = enumerable.Concat(extraGenStepDefs);
				}
				map.areaManager.AddStartingAreas();
				map.weatherDecider.StartInitialWeather();
				DeepProfiler.Start("Generate contents into map");
				MapGenerator.GenerateContentsIntoMap(enumerable, map);
				DeepProfiler.End();
				Find.Scenario.PostMapGenerate(map);
				DeepProfiler.Start("Finalize map init");
				map.FinalizeInit();
				DeepProfiler.End();
				DeepProfiler.Start("MapComponent.MapGenerated()");
				MapComponentUtility.MapGenerated(map);
				DeepProfiler.End();
				if (parent != null)
				{
					parent.PostMapGenerate();
				}
				result = map;
			}
			finally
			{
				DeepProfiler.End();
				MapGenerator.mapBeingGenerated = null;
				Current.ProgramState = programState;
			}
			return result;
		}

		public static void GenerateContentsIntoMap(IEnumerable<GenStepDef> genStepDefs, Map map)
		{
			Rand.Seed = Gen.HashCombineInt(Find.World.info.Seed, map.Tile);
			MapGenerator.data.Clear();
			RockNoises.Init(map);
			foreach (GenStepDef current in from x in genStepDefs
			orderby x.order, x.index
			select x)
			{
				DeepProfiler.Start("GenStep - " + current);
				try
				{
					current.genStep.Generate(map);
				}
				catch (Exception arg)
				{
					Log.Error("Error in GenStep: " + arg);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
			Rand.RandomizeStateFromTime();
			RockNoises.Reset();
			MapGenerator.data.Clear();
		}

		public static T GetVar<T>(string name)
		{
			object obj;
			if (MapGenerator.data.TryGetValue(name, out obj))
			{
				return (T)((object)obj);
			}
			return default(T);
		}

		public static bool TryGetVar<T>(string name, out T var)
		{
			object obj;
			if (MapGenerator.data.TryGetValue(name, out obj))
			{
				var = (T)((object)obj);
				return true;
			}
			var = default(T);
			return false;
		}

		public static void SetVar<T>(string name, T var)
		{
			MapGenerator.data[name] = var;
		}

		public static MapGenFloatGrid FloatGridNamed(string name)
		{
			MapGenFloatGrid var = MapGenerator.GetVar<MapGenFloatGrid>(name);
			if (var != null)
			{
				return var;
			}
			MapGenFloatGrid mapGenFloatGrid = new MapGenFloatGrid(MapGenerator.mapBeingGenerated);
			MapGenerator.SetVar<MapGenFloatGrid>(name, mapGenFloatGrid);
			return mapGenFloatGrid;
		}
	}
}
