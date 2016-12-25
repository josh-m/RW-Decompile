using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_UngeneratedPlanetParts : WorldLayer
	{
		private const int SubdivisionsCount = 4;

		private const float ViewAngleOffset = 10f;

		protected override void Regenerate()
		{
			base.Regenerate();
			Vector3 viewCenter = Find.WorldGrid.viewCenter;
			float viewAngle = Find.WorldGrid.viewAngle;
			if (viewAngle < 180f)
			{
				List<Vector3> collection;
				List<int> collection2;
				SphereGenerator.Generate(4, 99.85f, -viewCenter, 180f - Mathf.Min(viewAngle, 180f) + 10f, out collection, out collection2);
				LayerSubMesh subMesh = base.GetSubMesh(WorldMaterials.UngeneratedPlanetParts);
				subMesh.verts.AddRange(collection);
				subMesh.tris.AddRange(collection2);
			}
			base.FinalizeMesh(MeshParts.All, true);
		}
	}
}
