using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public class WorldLayer_Roads : WorldLayer_Paths
	{
		private ModuleBase roadDisplacementX = new Perlin(1.0, 2.0, 0.5, 3, 74173887, QualityMode.Medium);

		private ModuleBase roadDisplacementY = new Perlin(1.0, 2.0, 0.5, 3, 67515931, QualityMode.Medium);

		private ModuleBase roadDisplacementZ = new Perlin(1.0, 2.0, 0.5, 3, 87116801, QualityMode.Medium);

		[DebuggerHidden]
		public override IEnumerable Regenerate()
		{
			foreach (object result in base.Regenerate())
			{
				yield return result;
			}
			LayerSubMesh subMesh = base.GetSubMesh(WorldMaterials.Roads);
			WorldGrid grid = Find.WorldGrid;
			List<RoadWorldLayerDef> roadLayerDefs = (from rwld in DefDatabase<RoadWorldLayerDef>.AllDefs
			orderby rwld.order
			select rwld).ToList<RoadWorldLayerDef>();
			for (int i = 0; i < grid.TilesCount; i++)
			{
				if (i % 1000 == 0)
				{
					yield return null;
				}
				if (subMesh.verts.Count > 60000)
				{
					subMesh = base.GetSubMesh(WorldMaterials.Roads);
				}
				Tile tile = grid[i];
				if (!tile.WaterCovered)
				{
					List<WorldLayer_Paths.OutputDirection> outputs = new List<WorldLayer_Paths.OutputDirection>();
					if (tile.roads != null)
					{
						bool allowSmoothTransition = true;
						for (int j = 0; j < tile.roads.Count - 1; j++)
						{
							if (tile.roads[j].road.worldTransitionGroup != tile.roads[j + 1].road.worldTransitionGroup)
							{
								allowSmoothTransition = false;
							}
						}
						for (int k = 0; k < roadLayerDefs.Count; k++)
						{
							bool flag = false;
							outputs.Clear();
							for (int l = 0; l < tile.roads.Count; l++)
							{
								RoadDef road = tile.roads[l].road;
								float layerWidth = road.GetLayerWidth(roadLayerDefs[k]);
								if (layerWidth > 0f)
								{
									flag = true;
								}
								outputs.Add(new WorldLayer_Paths.OutputDirection
								{
									neighbor = tile.roads[l].neighbor,
									width = layerWidth,
									distortionFrequency = road.distortionFrequency,
									distortionIntensity = road.distortionIntensity
								});
							}
							if (flag)
							{
								base.GeneratePaths(subMesh, i, outputs, roadLayerDefs[k].color, allowSmoothTransition);
							}
						}
					}
				}
			}
			base.FinalizeMesh(MeshParts.All);
		}

		public override Vector3 FinalizePoint(Vector3 inp, float distortionFrequency, float distortionIntensity)
		{
			Vector3 coordinate = inp * distortionFrequency;
			float magnitude = inp.magnitude;
			inp = (inp + new Vector3(this.roadDisplacementX.GetValue(coordinate), this.roadDisplacementY.GetValue(coordinate), this.roadDisplacementZ.GetValue(coordinate)) * distortionIntensity).normalized * magnitude;
			return inp + inp.normalized * 0.012f;
		}
	}
}
