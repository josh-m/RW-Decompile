using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class DropPodAnimationUtility
	{
		private static MaterialPropertyBlock shadowPropertyBlock = new MaterialPropertyBlock();

		private static readonly Material ShadowMat = MaterialPool.MatFrom("Things/Special/DropPodShadow", ShaderDatabase.Transparent);

		public static Vector3 DrawPosAt(int ticksToImpact, IntVec3 basePos)
		{
			if (ticksToImpact < 0)
			{
				ticksToImpact = 0;
			}
			Vector3 result = basePos.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem);
			float num = (float)(ticksToImpact * ticksToImpact) * 0.01f;
			result.x -= num * 0.4f;
			result.z += num * 0.6f;
			return result;
		}

		public static void DrawDropSpotShadow(Thing dropPod, int ticksToImpact)
		{
			if (ticksToImpact < 0)
			{
				ticksToImpact = 0;
			}
			Vector3 pos = dropPod.TrueCenter();
			pos.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
			float num = 2f + (float)ticksToImpact / 100f;
			Vector3 s = new Vector3(num, 1f, num);
			Color white = Color.white;
			if (ticksToImpact > 150)
			{
				white.a = Mathf.InverseLerp(200f, 150f, (float)ticksToImpact);
			}
			DropPodAnimationUtility.shadowPropertyBlock.SetColor(ShaderIDs.ColorId, white);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, dropPod.Rotation.AsQuat, s);
			Graphics.DrawMesh(MeshPool.plane10Back, matrix, DropPodAnimationUtility.ShadowMat, 0, null, 0, DropPodAnimationUtility.shadowPropertyBlock);
		}
	}
}
