using System;
using UnityEngine;

namespace Verse
{
	public sealed class DeepResourceGrid : IExposable
	{
		private ushort[] defGrid;

		private ushort[] countGrid;

		private int lastDrawFrame = -1;

		public DeepResourceGrid()
		{
			this.defGrid = new ushort[CellIndices.NumGridCells];
			this.countGrid = new ushort[CellIndices.NumGridCells];
		}

		public void ExposeData()
		{
			string compressedString = string.Empty;
			string compressedString2 = string.Empty;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				compressedString = GridSaveUtility.CompressedStringForShortGrid((IntVec3 c) => this.defGrid[CellIndices.CellToIndex(c)]);
				compressedString2 = GridSaveUtility.CompressedStringForShortGrid((IntVec3 c) => this.countGrid[CellIndices.CellToIndex(c)]);
			}
			Scribe_Values.LookValue<string>(ref compressedString, "defGrid", null, false);
			Scribe_Values.LookValue<string>(ref compressedString2, "countGrid", null, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				foreach (GridSaveUtility.LoadedGridShort current in GridSaveUtility.LoadedUShortGrid(compressedString))
				{
					this.defGrid[CellIndices.CellToIndex(current.cell)] = current.val;
				}
				foreach (GridSaveUtility.LoadedGridShort current2 in GridSaveUtility.LoadedUShortGrid(compressedString2))
				{
					this.countGrid[CellIndices.CellToIndex(current2.cell)] = current2.val;
				}
			}
		}

		public ThingDef ThingDefAt(IntVec3 c)
		{
			return DefDatabase<ThingDef>.GetByShortHash(this.defGrid[CellIndices.CellToIndex(c)]);
		}

		public int CountAt(IntVec3 c)
		{
			return (int)this.countGrid[CellIndices.CellToIndex(c)];
		}

		public void SetAt(IntVec3 c, ThingDef def, int count)
		{
			if (count == 0)
			{
				def = null;
			}
			ushort num;
			if (def == null)
			{
				num = 0;
			}
			else
			{
				num = def.shortHash;
			}
			ushort num2 = (ushort)count;
			if (count > 65535)
			{
				Log.Error("Cannot store count " + count + " in DeepResourceGrid: out of ushort range.");
				num2 = 65535;
			}
			if (count < 0)
			{
				Log.Error("Cannot store count " + count + " in DeepResourceGrid: out of ushort range.");
				num2 = 0;
			}
			int num3 = CellIndices.CellToIndex(c);
			if (this.defGrid[num3] == num && this.countGrid[num3] == num2)
			{
				return;
			}
			this.defGrid[num3] = num;
			this.countGrid[num3] = num2;
		}

		public void DeepResourceGridDraw(bool forceDraw)
		{
			if ((DebugViewSettings.drawDeepResources || forceDraw) && this.lastDrawFrame != Time.frameCount)
			{
				foreach (IntVec3 current in Find.Map.AllCells)
				{
					int num = this.CountAt(current);
					if (num > 0)
					{
						CellRenderer.RenderCell(current, (float)num / 500f);
					}
				}
				this.lastDrawFrame = Time.frameCount;
			}
		}
	}
}
