using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public class WorldLayer_Rivers : WorldLayer_Paths
	{
		private Color32 riverColor = new Color32(73, 82, 100, 255);

		private const float PerlinFrequency = 0.6f;

		private const float PerlinMagnitude = 0.1f;

		private ModuleBase riverDisplacementX = new Perlin(0.60000002384185791, 2.0, 0.5, 3, 84905524, QualityMode.Medium);

		private ModuleBase riverDisplacementY = new Perlin(0.60000002384185791, 2.0, 0.5, 3, 37971116, QualityMode.Medium);

		private ModuleBase riverDisplacementZ = new Perlin(0.60000002384185791, 2.0, 0.5, 3, 91572032, QualityMode.Medium);

		public WorldLayer_Rivers()
		{
			this.pointyEnds = true;
		}

		[DebuggerHidden]
		public override IEnumerable Regenerate()
		{
			foreach (object result in base.Regenerate())
			{
				yield return result;
			}
			LayerSubMesh subMesh = base.GetSubMesh(WorldMaterials.Rivers);
			LayerSubMesh subMeshBorder = base.GetSubMesh(WorldMaterials.RiversBorder);
			WorldGrid grid = Find.WorldGrid;
			List<WorldLayer_Paths.OutputDirection> outputs = new List<WorldLayer_Paths.OutputDirection>();
			List<WorldLayer_Paths.OutputDirection> outputsBorder = new List<WorldLayer_Paths.OutputDirection>();
			for (int i = 0; i < grid.TilesCount; i++)
			{
				if (i % 1000 == 0)
				{
					yield return null;
				}
				if (subMesh.verts.Count > 60000)
				{
					subMesh = base.GetSubMesh(WorldMaterials.Rivers);
					subMeshBorder = base.GetSubMesh(WorldMaterials.RiversBorder);
				}
				Tile tile = grid[i];
				if (tile.rivers != null)
				{
					outputs.Clear();
					outputsBorder.Clear();
					for (int j = 0; j < tile.rivers.Count; j++)
					{
						outputs.Add(new WorldLayer_Paths.OutputDirection
						{
							neighbor = tile.rivers[j].neighbor,
							width = tile.rivers[j].river.widthOnWorld - 0.2f
						});
						outputsBorder.Add(new WorldLayer_Paths.OutputDirection
						{
							neighbor = tile.rivers[j].neighbor,
							width = tile.rivers[j].river.widthOnWorld
						});
					}
					base.GeneratePaths(subMesh, i, outputs, this.riverColor, true);
					base.GeneratePaths(subMeshBorder, i, outputsBorder, this.riverColor, true);
				}
			}
			base.FinalizeMesh(MeshParts.All);
		}

		public override Vector3 FinalizePoint(Vector3 inp, float distortionFrequency, float distortionIntensity)
		{
			float magnitude = inp.magnitude;
			inp = (inp + new Vector3(this.riverDisplacementX.GetValue(inp), this.riverDisplacementY.GetValue(inp), this.riverDisplacementZ.GetValue(inp)) * 0.1f).normalized * magnitude;
			return inp + inp.normalized * 0.008f;
		}
	}
}
