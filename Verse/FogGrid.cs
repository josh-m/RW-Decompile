using RimWorld;
using System;

namespace Verse
{
	public sealed class FogGrid : IExposable
	{
		private Map map;

		public bool[] fogGrid;

		public FogGrid(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			ArrayExposeUtility.ExposeBoolArray(ref this.fogGrid, this.map.Size.x, this.map.Size.z, "fogGrid");
		}

		public void Unfog(IntVec3 c)
		{
			int num = this.map.cellIndices.CellToIndex(c);
			if (!this.fogGrid[num])
			{
				return;
			}
			this.fogGrid[num] = false;
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.map.mapDrawer.MapMeshDirty(c, MapMeshFlag.FogOfWar);
			}
			Designation designation = this.map.designationManager.DesignationAt(c, DesignationDefOf.Mine);
			if (designation != null && MineUtility.MineableInCell(c, this.map) == null)
			{
				designation.Delete();
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.map.roofGrid.Drawer.SetDirty();
			}
		}

		public bool IsFogged(IntVec3 c)
		{
			return c.InBounds(this.map) && this.fogGrid != null && this.fogGrid[this.map.cellIndices.CellToIndex(c)];
		}

		public bool IsFogged(int index)
		{
			return this.fogGrid[index];
		}

		public void ClearAllFog()
		{
			for (int i = 0; i < this.map.Size.x; i++)
			{
				for (int j = 0; j < this.map.Size.z; j++)
				{
					this.Unfog(new IntVec3(i, 0, j));
				}
			}
		}

		public void Notify_FogBlockerRemoved(IntVec3 c)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				if (!this.IsFogged(c2))
				{
					this.Unfog(c);
					bool flag = false;
					FloodUnfogResult floodUnfogResult = default(FloodUnfogResult);
					for (int j = 0; j < 4; j++)
					{
						IntVec3 intVec = c + GenAdj.CardinalDirections[j];
						if (intVec.InBounds(this.map))
						{
							if (intVec.Fogged(this.map))
							{
								Thing edifice = intVec.GetEdifice(this.map);
								if (edifice == null || !edifice.def.MakeFog)
								{
									flag = true;
									floodUnfogResult = FloodFillerFog.FloodUnfog(intVec, this.map);
								}
								else
								{
									this.Unfog(intVec);
								}
							}
						}
					}
					for (int k = 0; k < 8; k++)
					{
						IntVec3 c3 = c + GenAdj.AdjacentCells[k];
						if (c3.InBounds(this.map))
						{
							Thing edifice2 = c3.GetEdifice(this.map);
							if (edifice2 != null && edifice2.def.MakeFog)
							{
								this.Unfog(c3);
							}
						}
					}
					if (flag)
					{
						if (floodUnfogResult.mechanoidFound)
						{
							Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealedWithMechanoids".Translate(), LetterType.BadUrgent, new TargetInfo(c, this.map, false), null);
						}
						else
						{
							Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealed".Translate(), LetterType.Good, new TargetInfo(c, this.map, false), null);
						}
					}
					return;
				}
			}
		}

		internal void SetAllFogged()
		{
			CellIndices cellIndices = this.map.cellIndices;
			if (this.fogGrid == null)
			{
				this.fogGrid = new bool[cellIndices.NumGridCells];
			}
			foreach (IntVec3 current in this.map.AllCells)
			{
				this.fogGrid[cellIndices.CellToIndex(current)] = true;
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.map.roofGrid.Drawer.SetDirty();
			}
		}
	}
}
