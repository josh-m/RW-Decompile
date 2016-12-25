using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class CellBoolDrawer
	{
		private const float Opacity = 0.33f;

		private const int MaxCellsPerMesh = 16383;

		public ICellBoolGiver giver;

		private bool wantDraw;

		private Material material;

		private bool dirty = true;

		private List<Mesh> meshes = new List<Mesh>();

		private static List<Vector3> verts = new List<Vector3>();

		private static List<int> tris = new List<int>();

		public CellBoolDrawer(ICellBoolGiver area)
		{
			this.giver = area;
			this.material = SolidColorMaterials.SimpleSolidColorMaterial(new Color(area.Color.r, area.Color.g, area.Color.b, 0.33f));
			this.material.renderQueue = 3600;
		}

		public void MarkForDraw()
		{
			this.wantDraw = true;
		}

		public void CellBoolDrawerUpdate()
		{
			if (this.wantDraw)
			{
				this.ActuallyDraw();
				this.wantDraw = false;
			}
		}

		private void ActuallyDraw()
		{
			if (this.dirty)
			{
				this.RegenerateMesh();
			}
			for (int i = 0; i < this.meshes.Count; i++)
			{
				Graphics.DrawMesh(this.meshes[i], Vector3.zero, Quaternion.identity, this.material, 0);
			}
		}

		public void SetDirty()
		{
			this.dirty = true;
		}

		public void RegenerateMesh()
		{
			for (int i = 0; i < this.meshes.Count; i++)
			{
				this.meshes[i].Clear();
			}
			int num = 0;
			int num2 = 0;
			if (this.meshes.Count < num + 1)
			{
				this.meshes.Add(new Mesh());
			}
			Mesh mesh = this.meshes[num];
			CellRect wholeMap = CellRect.WholeMap;
			float y = Altitudes.AltitudeFor(AltitudeLayer.WorldDataOverlay);
			for (int j = wholeMap.minX; j <= wholeMap.maxX; j++)
			{
				for (int k = wholeMap.minZ; k <= wholeMap.maxZ; k++)
				{
					if (this.giver.GetCellBool(CellIndices.CellToIndex(j, k)))
					{
						CellBoolDrawer.verts.Add(new Vector3((float)j, y, (float)k));
						CellBoolDrawer.verts.Add(new Vector3((float)j, y, (float)(k + 1)));
						CellBoolDrawer.verts.Add(new Vector3((float)(j + 1), y, (float)(k + 1)));
						CellBoolDrawer.verts.Add(new Vector3((float)(j + 1), y, (float)k));
						int count = CellBoolDrawer.verts.Count;
						CellBoolDrawer.tris.Add(count - 4);
						CellBoolDrawer.tris.Add(count - 3);
						CellBoolDrawer.tris.Add(count - 2);
						CellBoolDrawer.tris.Add(count - 4);
						CellBoolDrawer.tris.Add(count - 2);
						CellBoolDrawer.tris.Add(count - 1);
						num2++;
						if (num2 >= 16383)
						{
							this.FinalizeWorkingDataIntoMesh(mesh);
							num++;
							if (this.meshes.Count < num + 1)
							{
								this.meshes.Add(new Mesh());
							}
							mesh = this.meshes[num];
							num2 = 0;
						}
					}
				}
			}
			this.FinalizeWorkingDataIntoMesh(mesh);
			this.dirty = false;
		}

		private void FinalizeWorkingDataIntoMesh(Mesh mesh)
		{
			if (CellBoolDrawer.verts.Count > 0)
			{
				mesh.vertices = CellBoolDrawer.verts.ToArray();
				CellBoolDrawer.verts.Clear();
				mesh.SetTriangles(CellBoolDrawer.tris.ToArray(), 0);
				CellBoolDrawer.tris.Clear();
			}
		}
	}
}
