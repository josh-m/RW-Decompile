using System;
using System.Linq;
using UnityEngine;

namespace Verse
{
	internal class TemperatureSaveLoad
	{
		private Map map;

		private ushort[] tempGrid;

		public TemperatureSaveLoad(Map map)
		{
			this.map = map;
		}

		public void DoExposeWork()
		{
			byte[] arr = null;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				int num = Mathf.RoundToInt(this.map.mapTemperature.OutdoorTemp);
				ushort num2 = this.TempFloatToShort((float)num);
				ushort[] tempGrid = new ushort[this.map.cellIndices.NumGridCells];
				for (int i = 0; i < this.map.cellIndices.NumGridCells; i++)
				{
					tempGrid[i] = num2;
				}
				foreach (Region current in this.map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
				{
					if (current.Room != null)
					{
						ushort num3 = this.TempFloatToShort(current.Room.Temperature);
						foreach (IntVec3 current2 in current.Cells)
						{
							tempGrid[this.map.cellIndices.CellToIndex(current2)] = num3;
						}
					}
				}
				arr = MapSerializeUtility.SerializeUshort(this.map, (IntVec3 c) => tempGrid[this.map.cellIndices.CellToIndex(c)]);
			}
			DataExposeUtility.ByteArray(ref arr, "temperatures");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.tempGrid = new ushort[this.map.cellIndices.NumGridCells];
				MapSerializeUtility.LoadUshort(arr, this.map, delegate(IntVec3 c, ushort val)
				{
					this.tempGrid[this.map.cellIndices.CellToIndex(c)] = val;
				});
			}
		}

		public void ApplyLoadedDataToRegions()
		{
			if (this.tempGrid != null)
			{
				CellIndices cellIndices = this.map.cellIndices;
				foreach (Region current in this.map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
				{
					if (current.Room != null)
					{
						current.Room.Group.Temperature = this.TempShortToFloat(this.tempGrid[cellIndices.CellToIndex(current.Cells.First<IntVec3>())]);
					}
				}
				this.tempGrid = null;
			}
		}

		private ushort TempFloatToShort(float temp)
		{
			temp = Mathf.Clamp(temp, -270f, 2000f);
			temp *= 16f;
			return (ushort)((int)temp + 32768);
		}

		private float TempShortToFloat(ushort temp)
		{
			return ((float)temp - 32768f) / 16f;
		}
	}
}
