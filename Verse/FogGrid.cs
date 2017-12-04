using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class FogGrid : IExposable
	{
		private Map map;

		public bool[] fogGrid;

		private const int AlwaysSendLetterIfUnfoggedMoreCellsThan = 600;

		public FogGrid(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			DataExposeUtility.BoolArray(ref this.fogGrid, this.map.Area, "fogGrid");
		}

		public void Unfog(IntVec3 c)
		{
			this.UnfogWorker(c);
			List<Thing> thingList = c.GetThingList(this.map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def.Fillage == FillCategory.Full)
				{
					foreach (IntVec3 current in thing.OccupiedRect().Cells)
					{
						this.UnfogWorker(current);
					}
				}
			}
		}

		private void UnfogWorker(IntVec3 c)
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
			if (designation != null && c.GetFirstMineable(this.map) == null)
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
			bool flag = false;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				if (c2.InBounds(this.map) && !this.IsFogged(c2))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			this.FloodUnfogAdjacent(c);
		}

		public void Notify_PawnEnteringDoor(Building_Door door, Pawn pawn)
		{
			if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
			{
				return;
			}
			this.FloodUnfogAdjacent(door.Position);
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

		private void FloodUnfogAdjacent(IntVec3 c)
		{
			this.Unfog(c);
			bool flag = false;
			FloodUnfogResult floodUnfogResult = default(FloodUnfogResult);
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = c + GenAdj.CardinalDirections[i];
				if (intVec.InBounds(this.map))
				{
					if (intVec.Fogged(this.map))
					{
						Building edifice = intVec.GetEdifice(this.map);
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
			for (int j = 0; j < 8; j++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[j];
				if (c2.InBounds(this.map))
				{
					Building edifice2 = c2.GetEdifice(this.map);
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
					Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealedWithMechanoids".Translate(), LetterDefOf.ThreatBig, new TargetInfo(c, this.map, false), null);
				}
				else if (!floodUnfogResult.allOnScreen || floodUnfogResult.cellsUnfogged >= 600)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealed".Translate(), LetterDefOf.NeutralEvent, new TargetInfo(c, this.map, false), null);
				}
			}
		}
	}
}
