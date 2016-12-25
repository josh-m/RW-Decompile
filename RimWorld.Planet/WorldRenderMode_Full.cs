using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldRenderMode_Full : WorldRenderMode
	{
		public override string Label
		{
			get
			{
				return "WorldRenderModeFull".Translate();
			}
		}

		public override void DrawWorldMeshes()
		{
			foreach (BiomeDef current in DefDatabase<BiomeDef>.AllDefs)
			{
				current.DrawMaterial.SetPass(0);
				foreach (IntVec2 current2 in Find.World.AllSquares)
				{
					if (Find.World.grid.Get(current2).biome == current)
					{
						Vector3 position = WorldRenderUtility.WorldLocToSceneLocAdjusted(current2);
						Graphics.DrawMeshNow(MeshPool.plane10, position, Quaternion.identity);
					}
				}
			}
			WorldMaterials.matHill.SetPass(0);
			foreach (IntVec2 current3 in Find.World.AllSquares)
			{
				Hilliness hilliness = Find.World.grid.Get(current3).hilliness;
				if ((hilliness == Hilliness.SmallHills || hilliness == Hilliness.LargeHills) && !Find.World.grid.Get(current3).biome.hideTerrain)
				{
					Graphics.DrawMeshNow(MeshPool.plane10, WorldRenderUtility.WorldLocToSceneLocAdjusted(current3), Quaternion.identity);
				}
			}
			WorldMaterials.matMountain.SetPass(0);
			foreach (IntVec2 current4 in Find.World.AllSquares)
			{
				if (Find.World.grid.Get(current4).hilliness == Hilliness.Mountainous && !Find.World.grid.Get(current4).biome.hideTerrain)
				{
					int num = Rand.Range(2, 4);
					for (int i = 0; i < num; i++)
					{
						Vector3 vector = WorldRenderUtility.WorldLocToSceneLocAdjusted(current4);
						vector += this.RandomFeatureOffset(0.4f);
						Graphics.DrawMeshNow(MeshPool.plane10, vector, Quaternion.identity);
					}
				}
			}
		}

		private Vector3 RandomFeatureOffset(float dist)
		{
			return new Vector3(1f, 0f, 1f) * dist * Rand.Value;
		}
	}
}
