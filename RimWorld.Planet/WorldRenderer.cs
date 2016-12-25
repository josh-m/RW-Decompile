using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class WorldRenderer
	{
		private List<WorldLayer> layers = new List<WorldLayer>();

		public WorldRenderer()
		{
			foreach (Type current in typeof(WorldLayer).AllLeafSubclasses())
			{
				this.layers.Add((WorldLayer)Activator.CreateInstance(current));
			}
		}

		public void SetAllLayersDirty()
		{
			for (int i = 0; i < this.layers.Count; i++)
			{
				this.layers[i].SetDirty();
			}
		}

		public void RegenerateAllLayersNow()
		{
			for (int i = 0; i < this.layers.Count; i++)
			{
				this.layers[i].RegenerateNow();
			}
		}

		public void Notify_StaticWorldObjectPosChanged()
		{
			for (int i = 0; i < this.layers.Count; i++)
			{
				WorldLayer_WorldObjects worldLayer_WorldObjects = this.layers[i] as WorldLayer_WorldObjects;
				if (worldLayer_WorldObjects != null)
				{
					worldLayer_WorldObjects.SetDirty();
				}
			}
		}

		public void CheckActivateWorldCamera()
		{
			if (WorldRendererUtility.WorldRenderedNow)
			{
				Find.WorldCamera.gameObject.SetActive(true);
			}
			else
			{
				Find.WorldCamera.gameObject.SetActive(false);
			}
		}

		public void DrawWorldLayers()
		{
			WorldRendererUtility.UpdateWorldShadersParams();
			for (int i = 0; i < this.layers.Count; i++)
			{
				this.layers[i].Render();
			}
		}

		public int GetTileIDFromRayHit(RaycastHit hit)
		{
			int i = 0;
			int count = this.layers.Count;
			while (i < count)
			{
				WorldLayer_Terrain worldLayer_Terrain = this.layers[i] as WorldLayer_Terrain;
				if (worldLayer_Terrain != null)
				{
					return worldLayer_Terrain.GetTileIDFromRayHit(hit);
				}
				i++;
			}
			return -1;
		}
	}
}
