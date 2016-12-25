using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldRenderUtility
	{
		public static Vector3 WorldLocToSceneLocAdjusted(IntVec2 c)
		{
			return new Vector3((float)c.x + 0.5f, 0f, (float)c.z + 0.5f);
		}
	}
}
