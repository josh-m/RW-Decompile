using System;
using System.Linq;
using UnityEngine;

namespace Verse
{
	internal static class TemperatureSaveLoad
	{
		private static ushort[] tempGrid;

		public static void DoExposeWork()
		{
			string compressedString = string.Empty;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				int num = Mathf.RoundToInt(GenTemperature.OutdoorTemp);
				ushort num2 = TemperatureSaveLoad.TempFloatToShort((float)num);
				ushort[] tempGrid = new ushort[CellIndices.NumGridCells];
				for (int i = 0; i < CellIndices.NumGridCells; i++)
				{
					tempGrid[i] = num2;
				}
				foreach (Region current in Find.RegionGrid.AllRegions)
				{
					if (current.Room != null)
					{
						ushort num3 = TemperatureSaveLoad.TempFloatToShort(current.Room.Temperature);
						foreach (IntVec3 current2 in current.Cells)
						{
							tempGrid[CellIndices.CellToIndex(current2)] = num3;
						}
					}
				}
				compressedString = GridSaveUtility.CompressedStringForShortGrid((IntVec3 c) => tempGrid[CellIndices.CellToIndex(c)]);
			}
			Scribe_Values.LookValue<string>(ref compressedString, "temperatures", null, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				TemperatureSaveLoad.tempGrid = new ushort[CellIndices.NumGridCells];
				foreach (GridSaveUtility.LoadedGridShort current3 in GridSaveUtility.LoadedUShortGrid(compressedString))
				{
					TemperatureSaveLoad.tempGrid[CellIndices.CellToIndex(current3.cell)] = current3.val;
				}
			}
		}

		public static void ApplyLoadedDataToRegions()
		{
			if (TemperatureSaveLoad.tempGrid != null)
			{
				foreach (Region current in Find.RegionGrid.AllRegions)
				{
					if (current.Room != null)
					{
						current.Room.Temperature = TemperatureSaveLoad.TempShortToFloat(TemperatureSaveLoad.tempGrid[CellIndices.CellToIndex(current.Cells.First<IntVec3>())]);
					}
				}
				TemperatureSaveLoad.tempGrid = null;
			}
		}

		private static ushort TempFloatToShort(float temp)
		{
			temp = Mathf.Clamp(temp, -270f, 2000f);
			temp *= 16f;
			return (ushort)((int)temp + 32768);
		}

		private static float TempShortToFloat(ushort temp)
		{
			return ((float)temp - 32768f) / 16f;
		}
	}
}
