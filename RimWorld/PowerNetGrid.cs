using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PowerNetGrid
	{
		private Map map;

		private PowerNet[] netGrid;

		private Dictionary<PowerNet, List<IntVec3>> powerNetCells = new Dictionary<PowerNet, List<IntVec3>>();

		public PowerNetGrid(Map map)
		{
			this.map = map;
			this.netGrid = new PowerNet[map.cellIndices.NumGridCells];
		}

		public PowerNet TransmittedPowerNetAt(IntVec3 c)
		{
			return this.netGrid[this.map.cellIndices.CellToIndex(c)];
		}

		public void Notify_PowerNetCreated(PowerNet newNet)
		{
			if (this.powerNetCells.ContainsKey(newNet))
			{
				Log.Warning("Net " + newNet + " is already registered in PowerNetGrid.");
				this.powerNetCells.Remove(newNet);
			}
			List<IntVec3> list = new List<IntVec3>();
			this.powerNetCells.Add(newNet, list);
			for (int i = 0; i < newNet.transmitters.Count; i++)
			{
				CellRect cellRect = newNet.transmitters[i].parent.OccupiedRect();
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					for (int k = cellRect.minX; k <= cellRect.maxX; k++)
					{
						int num = this.map.cellIndices.CellToIndex(k, j);
						if (this.netGrid[num] != null)
						{
							Log.Warning(string.Concat(new object[]
							{
								"Two power nets on the same cell (",
								k,
								", ",
								j,
								"). First transmitters: ",
								newNet.transmitters[0].parent.LabelCap,
								" and ",
								(!this.netGrid[num].transmitters.NullOrEmpty<CompPower>()) ? this.netGrid[num].transmitters[0].parent.LabelCap : "[none]",
								"."
							}));
						}
						this.netGrid[num] = newNet;
						list.Add(new IntVec3(k, 0, j));
					}
				}
			}
		}

		public void Notify_PowerNetDeleted(PowerNet deadNet)
		{
			List<IntVec3> list;
			if (!this.powerNetCells.TryGetValue(deadNet, out list))
			{
				Log.Warning("Net " + deadNet + " does not exist in PowerNetGrid's dictionary.");
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				int num = this.map.cellIndices.CellToIndex(list[i]);
				if (this.netGrid[num] == deadNet)
				{
					this.netGrid[num] = null;
				}
				else
				{
					Log.Warning("Multiple nets on the same cell " + list[i] + ". This is probably a result of an earlier error.");
				}
			}
			this.powerNetCells.Remove(deadNet);
		}

		public void DrawDebugPowerNetGrid()
		{
			if (!DebugViewSettings.drawPowerNetGrid)
			{
				return;
			}
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (this.map != Find.VisibleMap)
			{
				return;
			}
			Rand.PushSeed();
			foreach (IntVec3 current in Find.CameraDriver.CurrentViewRect.ClipInsideMap(this.map))
			{
				PowerNet powerNet = this.netGrid[this.map.cellIndices.CellToIndex(current)];
				if (powerNet != null)
				{
					Rand.Seed = powerNet.GetHashCode();
					CellRenderer.RenderCell(current, Rand.Value);
				}
			}
			Rand.PopSeed();
		}
	}
}
