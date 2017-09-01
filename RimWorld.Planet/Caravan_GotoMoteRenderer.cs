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

		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

		private static Material cachedMaterial;

		public static readonly Material FeedbackGoto = MaterialPool.MatFrom("Things/Mote/FeedbackGoto", ShaderDatabase.WorldOverlayTransparent, WorldMaterials.DynamicObjectRenderQueue);

		public void RenderMote()
		{
			float num = (Time.time - this.lastOrderedToTileTime) / 0.5f;
			if (num > 1f)
			{
				return;
			}
			if (Caravan_GotoMoteRenderer.cachedMaterial == null)
			{
				Caravan_GotoMoteRenderer.cachedMaterial = MaterialPool.MatFrom((Texture2D)Caravan_GotoMoteRenderer.FeedbackGoto.mainTexture, Caravan_GotoMoteRenderer.FeedbackGoto.shader, Color.white, WorldMaterials.DynamicObjectRenderQueue);
			}
			WorldGrid worldGrid = Find.WorldGrid;
			Vector3 tileCenter = worldGrid.GetTileCenter(this.tile);
			Color value = new Color(1f, 1f, 1f, 1f - num);
			Caravan_GotoMoteRenderer.propertyBlock.SetColor(ShaderPropertyIDs.Color, value);
			MaterialPropertyBlock materialPropertyBlock = Caravan_GotoMoteRenderer.propertyBlock;
			WorldRendererUtility.DrawQuadTangentialToPlanet(tileCenter, 0.8f * worldGrid.averageTileSize, 0.018f, Caravan_GotoMoteRenderer.cachedMaterial, false, false, materialPropertyBlock);
		}

		public void OrderedToTile(int tile)
		{
			this.tile = tile;
			this.lastOrderedToTileTime = Time.time;
		}
	}
}
