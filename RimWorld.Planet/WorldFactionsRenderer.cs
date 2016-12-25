using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldFactionsRenderer
	{
		public static void DrawWorldFactions()
		{
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				if (!current.homeSquare.IsInvalid)
				{
					current.def.baseRenderMaterial.SetPass(0);
					Vector3 position = WorldRenderUtility.WorldLocToSceneLocAdjusted(current.homeSquare);
					Graphics.DrawMeshNow(MeshPool.plane10, position, Quaternion.identity);
				}
			}
		}
	}
}
