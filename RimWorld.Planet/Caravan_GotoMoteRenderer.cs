using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class Caravan_GotoMoteRenderer
	{
		private const float Duration = 0.5f;

		private const float BaseSize = 0.8f;

		private int tile;

		private float lastOrderedToTileTime = -0.51f;

		private Material material;

		public static readonly Material FeedbackGoto = MaterialPool.MatFrom("Things/Mote/FeedbackGoto", ShaderDatabase.WorldOverlayTransparent);

		public void RenderMote()
		{
			float num = (Time.time - this.lastOrderedToTileTime) / 0.5f;
			if (num > 1f)
			{
				return;
			}
			Color color = new Color(1f, 1f, 1f, 1f - num);
			if (this.material == null || this.material.color != color)
			{
				this.material = MaterialPool.MatFrom((Texture2D)Caravan_GotoMoteRenderer.FeedbackGoto.mainTexture, Caravan_GotoMoteRenderer.FeedbackGoto.shader, color);
			}
			WorldGrid worldGrid = Find.WorldGrid;
			Vector3 tileCenter = worldGrid.GetTileCenter(this.tile);
			WorldRendererUtility.DrawQuadTangentialToPlanet(tileCenter, 0.8f * worldGrid.averageTileSize, 0.01f, this.material, false, false, null);
		}

		public void OrderedToTile(int tile)
		{
			this.tile = tile;
			this.lastOrderedToTileTime = Time.time;
		}
	}
}
