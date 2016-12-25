using System;
using UnityEngine;

namespace RimWorld.Planet
{
	public class WorldRenderCam : MonoBehaviour
	{
		public WorldRenderMode renderMode;

		public void OnPostRender()
		{
			this.renderMode.DrawWorldMeshes();
			WorldFactionsRenderer.DrawWorldFactions();
		}
	}
}
