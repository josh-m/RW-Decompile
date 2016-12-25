using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public abstract class SectionLayer
	{
		protected Section section;

		public MapMeshFlag relevantChangeTypes;

		public List<LayerSubMesh> subMeshes = new List<LayerSubMesh>();

		protected Map Map
		{
			get
			{
				return this.section.map;
			}
		}

		public virtual bool Visible
		{
			get
			{
				return true;
			}
		}

		public SectionLayer(Section section)
		{
			this.section = section;
		}

		public LayerSubMesh GetSubMesh(Material material)
		{
			for (int i = 0; i < this.subMeshes.Count; i++)
			{
				if (this.subMeshes[i].material == material)
				{
					return this.subMeshes[i];
				}
			}
			Mesh mesh = new Mesh();
			if (UnityData.isEditor)
			{
				mesh.name = string.Concat(new object[]
				{
					"SectionLayerSubMesh_",
					base.GetType().Name,
					"_",
					this.Map.Tile
				});
			}
			LayerSubMesh layerSubMesh = new LayerSubMesh(mesh, material);
			this.subMeshes.Add(layerSubMesh);
			return layerSubMesh;
		}

		protected void FinalizeMesh(MeshParts tags)
		{
			for (int i = 0; i < this.subMeshes.Count; i++)
			{
				if (this.subMeshes[i].verts.Count > 0)
				{
					this.subMeshes[i].FinalizeMesh(tags, false);
				}
			}
		}

		public virtual void DrawLayer()
		{
			if (!this.Visible)
			{
				return;
			}
			int count = this.subMeshes.Count;
			for (int i = 0; i < count; i++)
			{
				LayerSubMesh layerSubMesh = this.subMeshes[i];
				if (layerSubMesh.finalized && !layerSubMesh.disabled)
				{
					Graphics.DrawMesh(layerSubMesh.mesh, Vector3.zero, Quaternion.identity, layerSubMesh.material, 0);
				}
			}
		}

		public abstract void Regenerate();

		protected void ClearSubMeshes(MeshParts parts)
		{
			foreach (LayerSubMesh current in this.subMeshes)
			{
				current.Clear(parts);
			}
		}
	}
}
