using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class PowerNetGrid
	{
		private static PowerNet[] netGrid;

		private static Dictionary<PowerNet, List<IntVec3>> powerNetCells = new Dictionary<PowerNet, List<IntVec3>>();

		public static void Reinit()
		{
			PowerNetGrid.netGrid = new PowerNet[CellIndices.NumGridCells];
			PowerNetGrid.powerNetCells.Clear();
		}

		public static PowerNet TransmittedPowerNetAt(IntVec3 c)
		{
			return PowerNetGrid.netGrid[CellIndices.CellToIndex(c)];
		}

		public static void Notify_PowerNetCreated(PowerNet newNet)
		{
			if (PowerNetGrid.powerNetCells.ContainsKey(newNet))
			{
				Log.Warning("Net " + newNet + " is already registered in PowerNetGrid.");
				PowerNetGrid.powerNetCells.Remove(newNet);
			}
			List<IntVec3> list = new List<IntVec3>();
			PowerNetGrid.powerNetCells.Add(newNet, list);
			for (int i = 0; i < newNet.transmitters.Count; i++)
			{
				CellRect cellRect = newNet.transmitters[i].parent.OccupiedRect();
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					for (int k = cellRect.minX; k <= cellRect.maxX; k++)
					{
						int num = CellIndices.CellToIndex(k, j);
						if (PowerNetGrid.netGrid[num] != null)
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
								(!PowerNetGrid.netGrid[num].transmitters.NullOrEmpty<CompPower>()) ? PowerNetGrid.netGrid[num].transmitters[0].parent.LabelCap : "[none]",
								"."
							}));
						}
						PowerNetGrid.netGrid[num] = newNet;
						list.Add(new IntVec3(k, 0, j));
					}
				}
			}
		}

		public static void Notify_PowerNetDeleted(PowerNet deadNet)
		{
			List<IntVec3> list;
			if (!PowerNetGrid.powerNetCells.TryGetValue(deadNet, out list))
			{
				Log.Warning("Net " + deadNet + " does not exist in PowerNetGrid's dictionary.");
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				int num = CellIndices.CellToIndex(list[i]);
				if (PowerNetGrid.netGrid[num] == deadNet)
				{
					PowerNetGrid.netGrid[num] = null;
				}
				else
				{
					Log.Warning("Multiple nets on the same cell " + list[i] + ". This is probably a result of an earlier error.");
				}
			}
			PowerNetGrid.powerNetCells.Remove(deadNet);
		}

		public static void DrawDebugPowerNetGrid()
		{
			if (!DebugViewSettings.drawPowerNetGrid)
			{
				return;
			}
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			Rand.PushSeed();
			foreach (IntVec3 current in Find.CameraDriver.CurrentViewRect.ClipInsideMap())
			{
				PowerNet powerNet = PowerNetGrid.netGrid[CellIndices.CellToIndex(current)];
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
