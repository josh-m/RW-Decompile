using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class WorldRendererUtility
	{
		private static readonly string SunLightDirectionPropertyName = "_PlanetSunLightDirection";

		private static readonly string SunLightEnabledPropertyName = "_PlanetSunLightEnabled";

		private static readonly string PlanetRadiusPropertyName = "_PlanetRadius";

		private static readonly string GlowRadiusPropertyName = "_GlowRadius";

		private static readonly string ColorPropertyName = "_Color";

		private static int SunLightDirectionPropertyID = Shader.PropertyToID(WorldRendererUtility.SunLightDirectionPropertyName);

		private static int SunLightEnabledPropertyID = Shader.PropertyToID(WorldRendererUtility.SunLightEnabledPropertyName);

		private static int PlanetRadiusPropertyID = Shader.PropertyToID(WorldRendererUtility.PlanetRadiusPropertyName);

		private static int GlowRadiusPropertyID = Shader.PropertyToID(WorldRendererUtility.GlowRadiusPropertyName);

		public static int ColorPropertyID = Shader.PropertyToID(WorldRendererUtility.ColorPropertyName);

		public static bool WorldRenderedNow
		{
			get
			{
				return Find.World != null && ((Current.ProgramState == ProgramState.Playing && Find.VisibleMap == null) || Find.WindowStack.AnyWindowWantsRenderedWorld);
			}
		}

		public static void UpdateWorldShadersParams()
		{
			Vector3 v = -GenCelestial.CurSunPositionInWorldSpace();
			bool flag = Current.ProgramState == ProgramState.Playing && Find.PlaySettings.usePlanetDayNightSystem;
			float value = (!flag) ? 0f : 1f;
			Shader.SetGlobalVector(WorldRendererUtility.SunLightDirectionPropertyID, v);
			Shader.SetGlobalFloat(WorldRendererUtility.SunLightEnabledPropertyID, value);
			WorldMaterials.PlanetGlow.SetFloat(WorldRendererUtility.PlanetRadiusPropertyID, 100f);
			WorldMaterials.PlanetGlow.SetFloat(WorldRendererUtility.GlowRadiusPropertyID, 8f);
		}

		public static void PrintQuadTangentialToPlanet(Vector3 pos, float size, float altOffset, LayerSubMesh subMesh, bool counterClockwise = false, bool randomizeRotation = false, bool printUVs = true)
		{
			WorldRendererUtility.PrintQuadTangentialToPlanet(pos, pos, size, altOffset, subMesh, counterClockwise, randomizeRotation, printUVs);
		}

		public static void PrintQuadTangentialToPlanet(Vector3 pos, Vector3 posForTangents, float size, float altOffset, LayerSubMesh subMesh, bool counterClockwise = false, bool randomizeRotation = false, bool printUVs = true)
		{
			Vector3 a;
			Vector3 a2;
			WorldRendererUtility.GetTangentsToPlanet(posForTangents, out a, out a2, randomizeRotation);
			Vector3 normalized = posForTangents.normalized;
			float d = size * 0.5f;
			Vector3 item = pos - a * d - a2 * d + normalized * altOffset;
			Vector3 item2 = pos - a * d + a2 * d + normalized * altOffset;
			Vector3 item3 = pos + a * d + a2 * d + normalized * altOffset;
			Vector3 item4 = pos + a * d - a2 * d + normalized * altOffset;
			int count = subMesh.verts.Count;
			subMesh.verts.Add(item);
			subMesh.verts.Add(item2);
			subMesh.verts.Add(item3);
			subMesh.verts.Add(item4);
			if (printUVs)
			{
				subMesh.uvs.Add(new Vector2(0f, 0f));
				subMesh.uvs.Add(new Vector2(0f, 1f));
				subMesh.uvs.Add(new Vector2(1f, 1f));
				subMesh.uvs.Add(new Vector2(1f, 0f));
			}
			if (counterClockwise)
			{
				subMesh.tris.Add(count + 2);
				subMesh.tris.Add(count + 1);
				subMesh.tris.Add(count);
				subMesh.tris.Add(count + 3);
				subMesh.tris.Add(count + 2);
				subMesh.tris.Add(count);
			}
			else
			{
				subMesh.tris.Add(count);
				subMesh.tris.Add(count + 1);
				subMesh.tris.Add(count + 2);
				subMesh.tris.Add(count);
				subMesh.tris.Add(count + 2);
				subMesh.tris.Add(count + 3);
			}
		}

		public static void DrawQuadTangentialToPlanet(Vector3 pos, float size, float altOffset, Material material, bool counterClockwise = false, bool useSkyboxLayer = false, MaterialPropertyBlock propertyBlock = null)
		{
			if (material == null)
			{
				Log.Warning("Tried to draw quad with null material.");
				return;
			}
			Vector3 normalized = pos.normalized;
			Vector3 vector;
			if (counterClockwise)
			{
				vector = -normalized;
			}
			else
			{
				vector = normalized;
			}
			Quaternion q = Quaternion.LookRotation(Vector3.Cross(vector, Vector3.up), vector);
			Vector3 s = new Vector3(size, 1f, size);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos + normalized * altOffset, q, s);
			int layer = (!useSkyboxLayer) ? WorldCameraManager.WorldLayer : WorldCameraManager.WorldSkyboxLayer;
			if (propertyBlock != null)
			{
				Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer, null, 0, propertyBlock);
			}
			else
			{
				Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer);
			}
		}

		public static void GetTangentsToPlanet(Vector3 pos, out Vector3 first, out Vector3 second, bool randomizeRotation = false)
		{
			Vector3 upwards;
			if (randomizeRotation)
			{
				upwards = Rand.PointOnSphere;
			}
			else
			{
				upwards = Vector3.up;
			}
			Quaternion rotation = Quaternion.LookRotation(pos.normalized, upwards);
			first = rotation * Vector3.up;
			second = rotation * Vector3.right;
		}

		public static Vector3 ProjectOnQuadTangentialToPlanet(Vector3 center, Vector2 point)
		{
			Vector3 a;
			Vector3 a2;
			WorldRendererUtility.GetTangentsToPlanet(center, out a, out a2, false);
			return point.x * a + point.y * a2;
		}

		public static void GetTangentialVectorFacing(Vector3 root, Vector3 pointToFace, out Vector3 forward, out Vector3 right)
		{
			Quaternion rotation = Quaternion.LookRotation(root, pointToFace);
			forward = rotation * Vector3.up;
			right = rotation * Vector3.left;
		}

		public static void PrintTextureAtlasUVs(int indexX, int indexY, int numX, int numY, LayerSubMesh subMesh)
		{
			float num = 1f / (float)numX;
			float num2 = 1f / (float)numY;
			float num3 = (float)indexX * num;
			float num4 = (float)indexY * num2;
			subMesh.uvs.Add(new Vector2(num3, num4));
			subMesh.uvs.Add(new Vector2(num3, num4 + num2));
			subMesh.uvs.Add(new Vector2(num3 + num, num4 + num2));
			subMesh.uvs.Add(new Vector2(num3 + num, num4));
		}
	}
}
