using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class SectionLayer_FogOfWar : SectionLayer
	{
		private const byte FogBrightness = 35;

		private bool[] vertsCovered = new bool[9];

		public override bool Visible
		{
			get
			{
				return DebugViewSettings.drawFog;
			}
		}

		public SectionLayer_FogOfWar(Section section) : base(section)
		{
			this.relevantChangeTypes = MapMeshFlag.FogOfWar;
		}

		public override void Regenerate()
		{
			LayerSubMesh subMesh = base.GetSubMesh(MatBases.FogOfWar);
			if (subMesh.mesh.vertexCount == 0)
			{
				SectionLayerGeometryMaker_Solid.MakeBaseGeometry(this.section, subMesh, AltitudeLayer.FogOfWar);
			}
			bool[] fogGrid = Find.FogGrid.fogGrid;
			CellRect cellRect = this.section.CellRect;
			int num = Find.Map.Size.z - 1;
			int num2 = Find.Map.Size.x - 1;
			subMesh.colors = new List<Color32>(subMesh.mesh.vertexCount);
			bool flag = false;
			for (int i = cellRect.minX; i <= cellRect.maxX; i++)
			{
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					if (fogGrid[CellIndices.CellToIndex(i, j)])
					{
						for (int k = 0; k < 9; k++)
						{
							this.vertsCovered[k] = true;
						}
					}
					else
					{
						for (int l = 0; l < 9; l++)
						{
							this.vertsCovered[l] = false;
						}
						if (j < num && fogGrid[CellIndices.CellToIndex(i, j + 1)])
						{
							this.vertsCovered[2] = true;
							this.vertsCovered[3] = true;
							this.vertsCovered[4] = true;
						}
						if (j > 0 && fogGrid[CellIndices.CellToIndex(i, j - 1)])
						{
							this.vertsCovered[6] = true;
							this.vertsCovered[7] = true;
							this.vertsCovered[0] = true;
						}
						if (i < num2 && fogGrid[CellIndices.CellToIndex(i + 1, j)])
						{
							this.vertsCovered[4] = true;
							this.vertsCovered[5] = true;
							this.vertsCovered[6] = true;
						}
						if (i > 0 && fogGrid[CellIndices.CellToIndex(i - 1, j)])
						{
							this.vertsCovered[0] = true;
							this.vertsCovered[1] = true;
							this.vertsCovered[2] = true;
						}
						if (j > 0 && i > 0 && fogGrid[CellIndices.CellToIndex(i - 1, j - 1)])
						{
							this.vertsCovered[0] = true;
						}
						if (j < num && i > 0 && fogGrid[CellIndices.CellToIndex(i - 1, j + 1)])
						{
							this.vertsCovered[2] = true;
						}
						if (j < num && i < num2 && fogGrid[CellIndices.CellToIndex(i + 1, j + 1)])
						{
							this.vertsCovered[4] = true;
						}
						if (j > 0 && i < num2 && fogGrid[CellIndices.CellToIndex(i + 1, j - 1)])
						{
							this.vertsCovered[6] = true;
						}
					}
					for (int m = 0; m < 9; m++)
					{
						byte a;
						if (this.vertsCovered[m])
						{
							a = 255;
							flag = true;
						}
						else
						{
							a = 0;
						}
						subMesh.colors.Add(new Color32(255, 255, 255, a));
					}
				}
			}
			if (flag)
			{
				subMesh.disabled = false;
				subMesh.FinalizeMesh(MeshParts.Colors);
			}
			else
			{
				subMesh.disabled = true;
			}
		}
	}
}
