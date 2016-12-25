using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public sealed class MapDrawer
	{
		private Map map;

		private Section[,] sections;

		private IntVec2 SectionCount
		{
			get
			{
				return new IntVec2
				{
					x = Mathf.CeilToInt((float)this.map.Size.x / 17f),
					z = Mathf.CeilToInt((float)this.map.Size.z / 17f)
				};
			}
		}

		public MapDrawer(Map map)
		{
			this.map = map;
		}

		public void MapMeshDirty(IntVec3 loc, MapMeshFlag dirtyFlags)
		{
			bool regenAdjacentCells = (dirtyFlags & (MapMeshFlag.FogOfWar | MapMeshFlag.Buildings)) != MapMeshFlag.None;
			bool regenAdjacentSections = (dirtyFlags & MapMeshFlag.GroundGlow) != MapMeshFlag.None;
			this.MapMeshDirty(loc, dirtyFlags, regenAdjacentCells, regenAdjacentSections);
		}

		public void MapMeshDirty(IntVec3 loc, MapMeshFlag dirtyFlags, bool regenAdjacentCells, bool regenAdjacentSections)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			Section section = this.SectionAt(loc);
			section.dirtyFlags |= dirtyFlags;
			if (regenAdjacentCells)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec = loc + GenAdj.AdjacentCells[i];
					if (intVec.InBounds(this.map))
					{
						this.SectionAt(intVec).dirtyFlags |= dirtyFlags;
					}
				}
			}
			if (regenAdjacentSections)
			{
				IntVec2 a = this.SectionCoordsAt(loc);
				for (int j = 0; j < 8; j++)
				{
					IntVec3 intVec2 = GenAdj.AdjacentCells[j];
					IntVec2 intVec3 = a + new IntVec2(intVec2.x, intVec2.z);
					IntVec2 sectionCount = this.SectionCount;
					if (intVec3.x >= 0 && intVec3.z >= 0 && intVec3.x <= sectionCount.x - 1 && intVec3.z <= sectionCount.z - 1)
					{
						Section section2 = this.sections[intVec3.x, intVec3.z];
						section2.dirtyFlags |= dirtyFlags;
					}
				}
			}
		}

		public void MapMeshDrawerUpdate_First()
		{
			CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
			currentViewRect.ClipInsideMap(this.map);
			IntVec2 intVec = this.SectionCoordsAt(currentViewRect.BottomLeft);
			IntVec2 intVec2 = this.SectionCoordsAt(currentViewRect.TopRight);
			if (intVec2.x < intVec.x || intVec2.z < intVec.z)
			{
				return;
			}
			CellRect cellRect = CellRect.FromLimits(intVec.x, intVec.z, intVec2.x, intVec2.z);
			bool flag = false;
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				Section sect = this.sections[current.x, current.z];
				if (this.TryUpdateSection(sect))
				{
					flag = true;
				}
				iterator.MoveNext();
			}
			if (!flag)
			{
				for (int i = 0; i < this.SectionCount.x; i++)
				{
					for (int j = 0; j < this.SectionCount.z; j++)
					{
						if (this.TryUpdateSection(this.sections[i, j]))
						{
							return;
						}
					}
				}
			}
		}

		private bool TryUpdateSection(Section sect)
		{
			if (sect.dirtyFlags == MapMeshFlag.None)
			{
				return false;
			}
			for (int i = 0; i < MapMeshFlagUtility.allFlags.Count; i++)
			{
				MapMeshFlag mapMeshFlag = MapMeshFlagUtility.allFlags[i];
				if ((sect.dirtyFlags & mapMeshFlag) != MapMeshFlag.None)
				{
					sect.RegenerateLayers(mapMeshFlag);
				}
			}
			sect.dirtyFlags = MapMeshFlag.None;
			return true;
		}

		public void DrawMapMesh()
		{
			CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
			currentViewRect.minX -= 17;
			currentViewRect.minZ -= 17;
			CellRect sunShadowsViewRect = this.GetSunShadowsViewRect(currentViewRect);
			Section[,] array = this.sections;
			int length = array.GetLength(0);
			int length2 = array.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					Section section = array[i, j];
					if (sunShadowsViewRect.Contains(section.botLeft))
					{
						section.DrawSection(!currentViewRect.Contains(section.botLeft));
					}
				}
			}
		}

		private IntVec2 SectionCoordsAt(IntVec3 loc)
		{
			return new IntVec2(Mathf.FloorToInt((float)(loc.x / 17)), Mathf.FloorToInt((float)(loc.z / 17)));
		}

		public Section SectionAt(IntVec3 loc)
		{
			IntVec2 intVec = this.SectionCoordsAt(loc);
			return this.sections[intVec.x, intVec.z];
		}

		public void RegenerateEverythingNow()
		{
			if (this.sections == null)
			{
				this.sections = new Section[this.SectionCount.x, this.SectionCount.z];
			}
			for (int i = 0; i < this.SectionCount.x; i++)
			{
				for (int j = 0; j < this.SectionCount.z; j++)
				{
					if (this.sections[i, j] == null)
					{
						this.sections[i, j] = new Section(new IntVec3(i, 0, j), this.map);
					}
					this.sections[i, j].RegenerateAllLayers();
				}
			}
		}

		public void WholeMapChanged(MapMeshFlag change)
		{
			for (int i = 0; i < this.SectionCount.x; i++)
			{
				for (int j = 0; j < this.SectionCount.z; j++)
				{
					this.sections[i, j].dirtyFlags |= change;
				}
			}
		}

		private CellRect GetSunShadowsViewRect(CellRect rect)
		{
			Vector2 vector = GenCelestial.CurShadowVector(this.map);
			if (vector.x < 0f)
			{
				rect.maxX -= Mathf.FloorToInt(vector.x);
			}
			else
			{
				rect.minX -= Mathf.CeilToInt(vector.x);
			}
			if (vector.y < 0f)
			{
				rect.maxZ -= Mathf.FloorToInt(vector.y);
			}
			else
			{
				rect.minZ -= Mathf.CeilToInt(vector.y);
			}
			return rect;
		}
	}
}
