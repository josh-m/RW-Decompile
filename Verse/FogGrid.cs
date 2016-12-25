using RimWorld;
using System;

namespace Verse
{
	public sealed class FogGrid : IExposable
	{
		public bool[] fogGrid;

		public void ExposeData()
		{
			ArrayExposeUtility.ExposeBoolArray(ref this.fogGrid, "fogGrid");
		}

		public void Unfog(IntVec3 c)
		{
			int num = CellIndices.CellToIndex(c);
			if (!this.fogGrid[num])
			{
				return;
			}
			this.fogGrid[num] = false;
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				Find.Map.mapDrawer.MapMeshDirty(c, MapMeshFlag.FogOfWar);
			}
			Designation designation = Find.DesignationManager.DesignationAt(c, DesignationDefOf.Mine);
			if (designation != null && MineUtility.MineableInCell(c) == null)
			{
				designation.Delete();
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				Find.RoofGrid.Drawer.SetDirty();
			}
		}

		public bool IsFogged(IntVec3 c)
		{
			return c.InBounds() && this.fogGrid[CellIndices.CellToIndex(c)];
		}

		public bool IsFogged(int index)
		{
			return this.fogGrid[index];
		}

		public void ClearAllFog()
		{
			for (int i = 0; i < Find.Map.Size.x; i++)
			{
				for (int j = 0; j < Find.Map.Size.z; j++)
				{
					this.Unfog(new IntVec3(i, 0, j));
				}
			}
		}

		public void Notify_FogBlockerRemoved(IntVec3 c)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			this.Unfog(c);
			bool flag = false;
			FloodUnfogResult floodUnfogResult = default(FloodUnfogResult);
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = c + GenAdj.CardinalDirections[i];
				if (intVec.InBounds())
				{
					if (intVec.Fogged())
					{
						Thing edifice = intVec.GetEdifice();
						if (edifice == null || !edifice.def.MakeFog)
						{
							flag = true;
							floodUnfogResult = FloodFillerFog.FloodUnfog(intVec);
						}
						else
						{
							this.Unfog(intVec);
						}
					}
				}
			}
			for (int j = 0; j < 8; j++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[j];
				if (c2.InBounds())
				{
					Thing edifice2 = c2.GetEdifice();
					if (edifice2 != null && edifice2.def.MakeFog)
					{
						this.Unfog(c2);
					}
				}
			}
			if (flag)
			{
				if (floodUnfogResult.mechanoidFound)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealedWithMechanoids".Translate(), LetterType.BadUrgent, c, null);
				}
				else
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealed".Translate(), LetterType.Good, c, null);
				}
			}
		}

		internal void SetAllFogged()
		{
			if (this.fogGrid == null)
			{
				this.fogGrid = new bool[CellIndices.NumGridCells];
			}
			foreach (IntVec3 current in Find.Map.AllCells)
			{
				this.fogGrid[CellIndices.CellToIndex(current)] = true;
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				Find.RoofGrid.Drawer.SetDirty();
			}
		}
	}
}
