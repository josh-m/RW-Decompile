using RimWorld.Planet;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class MapGenerator
	{
		[Unsaved]
		private static Dictionary<string, MapGenFloatGrid> genGrids;

		private static IntVec3 playerStartSpotInt = IntVec3.Invalid;

		public static List<IntVec3> rootsToUnfog = new List<IntVec3>();

		public static MapGenFloatGrid Elevation
		{
			get
			{
				return MapGenerator.genGrids["Elevation"];
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

		public static Map GenerateMap(IntVec3 mapSize, int tile, MapParent parent = null, Action<Map> extraInitBeforeContentGen = null, MapGeneratorDef mapGenerator = null)
		{
			ProgramState programState = Current.ProgramState;
			Current.ProgramState = ProgramState.MapInitializing;
			MapGenerator.playerStartSpotInt = IntVec3.Invalid;
			MapGenerator.rootsToUnfog.Clear();
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
				map.info.Size = mapSize;
				map.info.tile = tile;
				map.info.parent = parent;
				map.ConstructComponents();
				DeepProfiler.End();
				Current.Game.AddMap(map);
				if (extraInitBeforeContentGen != null)
				{
					extraInitBeforeContentGen(map);
				}
				DeepProfiler.Start("Generate contents into map");
				MapGeneratorDef arg_F5_0 = mapGenerator;
				if (mapGenerator == null)
				{
					arg_F5_0 = DefDatabase<MapGeneratorDef>.AllDefsListForReading.RandomElementByWeight((MapGeneratorDef x) => x.selectionWeight);
				}
				MapGenerator.GenerateContentsIntoMap(arg_F5_0, map);
				DeepProfiler.End();
				Find.Scenario.PostMapGenerate(map);
				map.areaManager.AddStartingAreas();
				DeepProfiler.Start("Finalize map init");
				map.FinalizeInit();
				DeepProfiler.End();
				result = map;
			}
			finally
			{
				DeepProfiler.End();
				Current.ProgramState = programState;
			}
			return result;
		}

		public static void GenerateContentsIntoMap(MapGeneratorDef def, Map map)
		{
			Rand.Seed = Gen.HashCombineInt(Find.World.info.Seed, map.Tile);
			MapGenerator.genGrids = new Dictionary<string, MapGenFloatGrid>();
			RockNoises.Init(map);
			foreach (GenStepDef current in def.GenStepsInOrder)
			{
				DeepProfiler.Start("Genstep - " + current.ToString());
				try
				{
					current.genStep.Generate(map);
				}
				catch (Exception ex)
				{
					Log.Error(ex.ToString());
				}
				finally
				{
					DeepProfiler.End();
				}
			}
			DeepProfiler.Start("GenerateInitialFogGrid");
			map.fogGrid.SetAllFogged();
			FloodFillerFog.FloodUnfog(MapGenerator.PlayerStartSpot, map);
			for (int i = 0; i < MapGenerator.rootsToUnfog.Count; i++)
			{
				FloodFillerFog.FloodUnfog(MapGenerator.rootsToUnfog[i], map);
			}
			DeepProfiler.End();
			Rand.RandomizeSeedFromTime();
			RockNoises.Reset();
			MapGenerator.genGrids = null;
		}

		public static MapGenFloatGrid FloatGridNamed(string name, Map map)
		{
			if (MapGenerator.genGrids.ContainsKey(name))
			{
				return MapGenerator.genGrids[name];
			}
			MapGenFloatGrid mapGenFloatGrid = new MapGenFloatGrid(name, map);
			MapGenerator.genGrids.Add(name, mapGenFloatGrid);
			return mapGenFloatGrid;
		}
	}
}
