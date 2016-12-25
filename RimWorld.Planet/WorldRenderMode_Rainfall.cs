using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldRenderMode_Rainfall : WorldRenderMode
	{
		public override string Label
		{
			get
			{
				return "WorldRenderModeRainfall".Translate();
			}
		}

		public override void DrawWorldMeshes()
		{
			foreach (IntVec2 current in Find.World.AllSquares)
			{
				WorldSquare worldSquare = Find.World.grid.Get(current);
				if (worldSquare.biome == BiomeDefOf.Ocean)
				{
					WorldMaterials.OverlayModeMatOcean.SetPass(0);
				}
				else
				{
					WorldMaterials.MatForRainfallOverlay(worldSquare.rainfall).SetPass(0);
				}
				Vector3 position = WorldRenderUtility.WorldLocToSceneLocAdjusted(current);
				Graphics.DrawMeshNow(MeshPool.plane10, position, Quaternion.identity);
			}
		}
	}
}
