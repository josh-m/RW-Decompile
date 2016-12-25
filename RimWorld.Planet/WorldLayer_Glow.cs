using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Glow : WorldLayer
	{
		private const int SubdivisionsCount = 4;

		public const float GlowRadius = 8f;

		protected override void Regenerate()
		{
			base.Regenerate();
			List<Vector3> collection;
			List<int> collection2;
			SphereGenerator.Generate(4, 108.1f, Vector3.forward, 360f, out collection, out collection2);
			LayerSubMesh subMesh = base.GetSubMesh(WorldMaterials.PlanetGlow);
			subMesh.verts.AddRange(collection);
			subMesh.tris.AddRange(collection2);
			base.FinalizeMesh(MeshParts.All, true);
		}
	}
}
