using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_UngeneratedPlanetParts : WorldLayer
	{
		private const int SubdivisionsCount = 4;

		private const float ViewAngleOffset = 10f;

		[DebuggerHidden]
		public override IEnumerable Regenerate()
		{
			foreach (object result in base.Regenerate())
			{
				yield return result;
			}
			Vector3 planetViewCenter = Find.WorldGrid.viewCenter;
			float planetViewAngle = Find.WorldGrid.viewAngle;
			if (planetViewAngle < 180f)
			{
				List<Vector3> collection;
				List<int> collection2;
				SphereGenerator.Generate(4, 99.85f, -planetViewCenter, 180f - Mathf.Min(planetViewAngle, 180f) + 10f, out collection, out collection2);
				LayerSubMesh subMesh = base.GetSubMesh(WorldMaterials.UngeneratedPlanetParts);
				subMesh.verts.AddRange(collection);
				subMesh.tris.AddRange(collection2);
			}
			base.FinalizeMesh(MeshParts.All);
		}
	}
}
