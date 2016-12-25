using System;
using System.Collections.Generic;

namespace Verse
{
	public static class MapGenerator
	{
		[Unsaved]
		private static Dictionary<string, MapGenFloatGrid> genGrids;

		private static IntVec3 playerStartSpotInt = IntVec3.Invalid;

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
					return Find.Map.Center;
				}
				return MapGenerator.playerStartSpotInt;
			}
			set
			{
				MapGenerator.playerStartSpotInt = value;
			}
		}

		public static void GenerateContentsIntoCurrentMap(MapGeneratorDef def)
		{
			int num = Find.World.info.Seed;
			num = num * 31 + Find.Map.WorldCoords.x;
			num = num * 91 + Find.Map.WorldCoords.z;
			Rand.Seed = num;
			MapGenerator.genGrids = new Dictionary<string, MapGenFloatGrid>();
			RockNoises.Init();
			foreach (GenStepDef current in def.GenStepsInOrder)
			{
				DeepProfiler.Start("Genstep - " + current.ToString());
				try
				{
					current.genStep.Generate();
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
			Find.FogGrid.SetAllFogged();
			FloodFillerFog.FloodUnfog(MapGenerator.PlayerStartSpot);
			DeepProfiler.End();
			Rand.RandomizeSeedFromTime();
			RockNoises.Reset();
			MapGenerator.genGrids = null;
		}

		public static MapGenFloatGrid FloatGridNamed(string name)
		{
			if (MapGenerator.genGrids.ContainsKey(name))
			{
				return MapGenerator.genGrids[name];
			}
			MapGenFloatGrid mapGenFloatGrid = new MapGenFloatGrid(name);
			MapGenerator.genGrids.Add(name, mapGenFloatGrid);
			return mapGenFloatGrid;
		}
	}
}
