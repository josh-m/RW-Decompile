using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldRenderMode_Biomes : WorldRenderMode
	{
		public override string Label
		{
			get
			{
				return "WorldRenderModeBiomes".Translate();
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
		}
	}
}
