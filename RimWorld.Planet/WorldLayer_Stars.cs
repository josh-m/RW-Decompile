using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Stars : WorldLayer
	{
		public const float DistanceToStars = 10f;

		private const int StarsCount = 1500;

		private const float SunDrawSize = 15f;

		private const float DistToSunToReduceStarSize = 0.8f;

		private static readonly FloatRange StarsDrawSize = new FloatRange(1f, 3.8f);

		protected override int Layer
		{
			get
			{
				return WorldCameraManager.WorldSkyboxLayer;
			}
		}

		protected override Quaternion Rotation
		{
			get
			{
				return Quaternion.LookRotation(GenCelestial.CurSunPositionInWorldSpace());
			}
		}

		protected override void Regenerate()
		{
			base.Regenerate();
			Rand.PushSeed();
			Rand.Seed = Find.World.info.Seed;
			for (int i = 0; i < 1500; i++)
			{
				Vector3 pointOnSphere = Rand.PointOnSphere;
				Vector3 pos = pointOnSphere * 10f;
				LayerSubMesh subMesh = base.GetSubMesh(WorldMaterials.Stars);
				float num = WorldLayer_Stars.StarsDrawSize.RandomInRange;
				float num2 = Vector3.Dot(pointOnSphere, Vector3.forward);
				if (num2 > 0.8f)
				{
					num *= GenMath.LerpDouble(0.8f, 1f, 1f, 0.35f, num2);
				}
				WorldRendererUtility.PrintQuadTangentialToPlanet(pos, num, 0f, subMesh, true, true, true);
			}
			LayerSubMesh subMesh2 = base.GetSubMesh(WorldMaterials.Sun);
			WorldRendererUtility.PrintQuadTangentialToPlanet(Vector3.forward * 10f, 15f, 0f, subMesh2, true, false, true);
			Rand.PopSeed();
			base.FinalizeMesh(MeshParts.All, true);
		}
	}
}
