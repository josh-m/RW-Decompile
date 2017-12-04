using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Glow : WorldLayer
	{
		private const int SubdivisionsCount = 4;

		public const float GlowRadius = 8f;

		[DebuggerHidden]
		public override IEnumerable Regenerate()
		{
			foreach (object result in base.Regenerate())
			{
				yield return result;
			}
			List<Vector3> tmpVerts;
			List<int> tmpIndices;
			SphereGenerator.Generate(4, 108.1f, Vector3.forward, 360f, out tmpVerts, out tmpIndices);
			LayerSubMesh subMesh = base.GetSubMesh(WorldMaterials.PlanetGlow);
			subMesh.verts.AddRange(tmpVerts);
			subMesh.tris.AddRange(tmpIndices);
			base.FinalizeMesh(MeshParts.All);
		}
	}
}
