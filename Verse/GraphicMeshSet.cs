using System;
using UnityEngine;

namespace Verse
{
	public class GraphicMeshSet
	{
		private Mesh[] meshes = new Mesh[4];

		public GraphicMeshSet(Mesh normalMesh, Mesh leftMesh)
		{
			Mesh[] arg_30_0 = this.meshes;
			int arg_30_1 = 0;
			Mesh[] arg_2E_0 = this.meshes;
			int arg_2E_1 = 1;
			this.meshes[2] = normalMesh;
			arg_30_0[arg_30_1] = (arg_2E_0[arg_2E_1] = normalMesh);
			this.meshes[3] = leftMesh;
		}

		public GraphicMeshSet(float size)
		{
			this.meshes[0] = (this.meshes[1] = (this.meshes[2] = MeshMakerPlanes.NewPlaneMesh(size, false, true)));
			this.meshes[3] = MeshMakerPlanes.NewPlaneMesh(size, true, true);
		}

		public GraphicMeshSet(float width, float height)
		{
			Vector2 size = new Vector2(width, height);
			this.meshes[0] = (this.meshes[1] = (this.meshes[2] = MeshMakerPlanes.NewPlaneMesh(size, false, true, false)));
			this.meshes[3] = MeshMakerPlanes.NewPlaneMesh(size, true, true, false);
		}

		public Mesh MeshAt(Rot4 rot)
		{
			return this.meshes[rot.AsInt];
		}
	}
}
