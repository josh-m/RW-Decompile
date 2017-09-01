using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class CellBoolDrawer
	{
		private const float DefaultOpacity = 0.33f;

		private const int MaxCellsPerMesh = 16383;

		public ICellBoolGiver giver;

		private bool wantDraw;

		private Material material;

		private bool materialCaresAboutVertexColors;

		private bool dirty = true;

		private List<Mesh> meshes = new List<Mesh>();

		private int mapSizeX;

		private int mapSizeZ;

		private float opacity = 0.33f;

		private static List<Vector3> verts = new List<Vector3>();

		private static List<int> tris = new List<int>();

		private static List<Color> colors = new List<Color>();

		public CellBoolDrawer(ICellBoolGiver giver, int mapSizeX, int mapSizeZ, float opacity = 0.33f)
		{
			this.giver = giver;
			this.mapSizeX = mapSizeX;
			this.mapSizeZ = mapSizeZ;
			this.opacity = opacity;
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
				Mesh mesh = new Mesh();
				mesh.name = "CellBoolDrawer";
				this.meshes.Add(mesh);
			}
			Mesh mesh2 = this.meshes[num];
			CellRect cellRect = new CellRect(0, 0, this.mapSizeX, this.mapSizeZ);
			float y = Altitudes.AltitudeFor(AltitudeLayer.WorldDataOverlay);
			bool careAboutVertexColors = false;
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
				{
					int index = CellIndicesUtility.CellToIndex(j, k, this.mapSizeX);
					if (this.giver.GetCellBool(index))
					{
						CellBoolDrawer.verts.Add(new Vector3((float)j, y, (float)k));
						CellBoolDrawer.verts.Add(new Vector3((float)j, y, (float)(k + 1)));
						CellBoolDrawer.verts.Add(new Vector3((float)(j + 1), y, (float)(k + 1)));
						CellBoolDrawer.verts.Add(new Vector3((float)(j + 1), y, (float)k));
						Color cellExtraColor = this.giver.GetCellExtraColor(index);
						CellBoolDrawer.colors.Add(cellExtraColor);
						CellBoolDrawer.colors.Add(cellExtraColor);
						CellBoolDrawer.colors.Add(cellExtraColor);
						CellBoolDrawer.colors.Add(cellExtraColor);
						if (cellExtraColor != Color.white)
						{
							careAboutVertexColors = true;
						}
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
							this.FinalizeWorkingDataIntoMesh(mesh2);
							num++;
							if (this.meshes.Count < num + 1)
							{
								Mesh mesh3 = new Mesh();
								mesh3.name = "CellBoolDrawer";
								this.meshes.Add(mesh3);
							}
							mesh2 = this.meshes[num];
							num2 = 0;
						}
					}
				}
			}
			this.FinalizeWorkingDataIntoMesh(mesh2);
			this.CreateMaterialIfNeeded(careAboutVertexColors);
			this.dirty = false;
		}

		private void FinalizeWorkingDataIntoMesh(Mesh mesh)
		{
			if (CellBoolDrawer.verts.Count > 0)
			{
				mesh.SetVertices(CellBoolDrawer.verts);
				CellBoolDrawer.verts.Clear();
				mesh.SetTriangles(CellBoolDrawer.tris, 0);
				CellBoolDrawer.tris.Clear();
				mesh.SetColors(CellBoolDrawer.colors);
				CellBoolDrawer.colors.Clear();
			}
		}

		private void CreateMaterialIfNeeded(bool careAboutVertexColors)
		{
			if (this.material == null || this.materialCaresAboutVertexColors != careAboutVertexColors)
			{
				this.material = SolidColorMaterials.SimpleSolidColorMaterial(new Color(this.giver.Color.r, this.giver.Color.g, this.giver.Color.b, this.opacity * this.giver.Color.a), careAboutVertexColors);
				this.materialCaresAboutVertexColors = careAboutVertexColors;
				this.material.renderQueue = 3600;
			}
		}
	}
}
