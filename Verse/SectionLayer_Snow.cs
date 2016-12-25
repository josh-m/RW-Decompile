using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	internal class SectionLayer_Snow : SectionLayer
	{
		private float[] vertDepth = new float[9];

		private static readonly Color32 ColorClear = new Color32(255, 255, 255, 0);

		private static readonly Color32 ColorWhite = new Color32(255, 255, 255, 255);

		public override bool Visible
		{
			get
			{
				return DebugViewSettings.drawSnow;
			}
		}

		public SectionLayer_Snow(Section section) : base(section)
		{
			this.relevantChangeTypes = MapMeshFlag.Snow;
		}

		private static bool Filled(int index)
		{
			Building building = Find.EdificeGrid[index];
			return building != null && building.def.Fillage == FillCategory.Full;
		}

		public override void Regenerate()
		{
			LayerSubMesh subMesh = base.GetSubMesh(MatBases.Snow);
			if (subMesh.mesh.vertexCount == 0)
			{
				SectionLayerGeometryMaker_Solid.MakeBaseGeometry(this.section, subMesh, AltitudeLayer.Terrain);
			}
			float[] depthGridDirect_Unsafe = Find.SnowGrid.DepthGridDirect_Unsafe;
			CellRect cellRect = this.section.CellRect;
			int num = Find.Map.Size.z - 1;
			int num2 = Find.Map.Size.x - 1;
			subMesh.colors = new List<Color32>(subMesh.mesh.vertexCount);
			bool flag = false;
			for (int i = cellRect.minX; i <= cellRect.maxX; i++)
			{
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					float num3 = depthGridDirect_Unsafe[CellIndices.CellToIndex(i, j)];
					int num4 = CellIndices.CellToIndex(i, j - 1);
					float num5 = (j <= 0 || SectionLayer_Snow.Filled(num4)) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = CellIndices.CellToIndex(i - 1, j - 1);
					float num6 = (j <= 0 || i <= 0 || SectionLayer_Snow.Filled(num4)) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = CellIndices.CellToIndex(i - 1, j);
					float num7 = (i <= 0 || SectionLayer_Snow.Filled(num4)) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = CellIndices.CellToIndex(i - 1, j + 1);
					float num8 = (j >= num || i <= 0 || SectionLayer_Snow.Filled(num4)) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = CellIndices.CellToIndex(i, j + 1);
					float num9 = (j >= num || SectionLayer_Snow.Filled(num4)) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = CellIndices.CellToIndex(i + 1, j + 1);
					float num10 = (j >= num || i >= num2 || SectionLayer_Snow.Filled(num4)) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = CellIndices.CellToIndex(i + 1, j);
					float num11 = (i >= num2 || SectionLayer_Snow.Filled(num4)) ? num3 : depthGridDirect_Unsafe[num4];
					num4 = CellIndices.CellToIndex(i + 1, j - 1);
					float num12 = (j <= 0 || i >= num2 || SectionLayer_Snow.Filled(num4)) ? num3 : depthGridDirect_Unsafe[num4];
					this.vertDepth[0] = (num5 + num6 + num7 + num3) / 4f;
					this.vertDepth[1] = (num7 + num3) / 2f;
					this.vertDepth[2] = (num7 + num8 + num9 + num3) / 4f;
					this.vertDepth[3] = (num9 + num3) / 2f;
					this.vertDepth[4] = (num9 + num10 + num11 + num3) / 4f;
					this.vertDepth[5] = (num11 + num3) / 2f;
					this.vertDepth[6] = (num11 + num12 + num5 + num3) / 4f;
					this.vertDepth[7] = (num5 + num3) / 2f;
					this.vertDepth[8] = num3;
					for (int k = 0; k < 9; k++)
					{
						if (this.vertDepth[k] > 0.01f)
						{
							flag = true;
						}
						subMesh.colors.Add(SectionLayer_Snow.SnowDepthColor(this.vertDepth[k]));
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

		private static Color32 SnowDepthColor(float snowDepth)
		{
			return Color32.Lerp(SectionLayer_Snow.ColorClear, SectionLayer_Snow.ColorWhite, snowDepth);
		}
	}
}
