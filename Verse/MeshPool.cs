using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class MeshPool
	{
		private const int MaxGridMeshSize = 15;

		private const float HumanlikeBodyWidth = 1.5f;

		private const float HumanlikeHeadAverageWidth = 1.5f;

		private const float HumanlikeHeadNarrowWidth = 1.3f;

		public static readonly GraphicMeshSet humanlikeBodySet;

		public static readonly GraphicMeshSet humanlikeHeadSet;

		public static readonly GraphicMeshSet humanlikeHairSetAverage;

		public static readonly GraphicMeshSet humanlikeHairSetNarrow;

		public static readonly Mesh plane025;

		public static readonly Mesh plane03;

		public static readonly Mesh plane05;

		public static readonly Mesh plane08;

		public static readonly Mesh plane10;

		public static readonly Mesh plane10Back;

		public static readonly Mesh plane10Flip;

		public static readonly Mesh plane14;

		public static readonly Mesh plane20;

		public static readonly Mesh wholeMapPlane;

		private static Dictionary<Vector2, Mesh> planes;

		private static Dictionary<Vector2, Mesh> planesFlip;

		public static readonly Mesh circle;

		public static readonly Mesh[] pies;

		static MeshPool()
		{
			MeshPool.humanlikeBodySet = new GraphicMeshSet(1.5f);
			MeshPool.humanlikeHeadSet = new GraphicMeshSet(1.5f);
			MeshPool.humanlikeHairSetAverage = new GraphicMeshSet(1.5f);
			MeshPool.humanlikeHairSetNarrow = new GraphicMeshSet(1.3f, 1.5f);
			MeshPool.plane025 = MeshMakerPlanes.NewPlaneMesh(0.25f);
			MeshPool.plane03 = MeshMakerPlanes.NewPlaneMesh(0.3f);
			MeshPool.plane05 = MeshMakerPlanes.NewPlaneMesh(0.5f);
			MeshPool.plane08 = MeshMakerPlanes.NewPlaneMesh(0.8f);
			MeshPool.plane10 = MeshMakerPlanes.NewPlaneMesh(1f);
			MeshPool.plane10Back = MeshMakerPlanes.NewPlaneMesh(1f, false, true);
			MeshPool.plane10Flip = MeshMakerPlanes.NewPlaneMesh(1f, true);
			MeshPool.plane14 = MeshMakerPlanes.NewPlaneMesh(1.4f);
			MeshPool.plane20 = MeshMakerPlanes.NewPlaneMesh(2f);
			MeshPool.planes = new Dictionary<Vector2, Mesh>(FastVector2Comparer.Instance);
			MeshPool.planesFlip = new Dictionary<Vector2, Mesh>(FastVector2Comparer.Instance);
			MeshPool.circle = MeshMakerCircles.MakeCircleMesh(1f);
			MeshPool.pies = new Mesh[361];
			for (int i = 0; i < 361; i++)
			{
				MeshPool.pies[i] = MeshMakerCircles.MakePieMesh(i);
			}
			MeshPool.wholeMapPlane = MeshMakerPlanes.NewWholeMapPlane();
		}

		public static Mesh GridPlane(Vector2 size)
		{
			Mesh mesh;
			if (!MeshPool.planes.TryGetValue(size, out mesh))
			{
				mesh = MeshMakerPlanes.NewPlaneMesh(size, false, false, false);
				MeshPool.planes.Add(size, mesh);
			}
			return mesh;
		}

		public static Mesh GridPlaneFlip(Vector2 size)
		{
			Mesh mesh;
			if (!MeshPool.planesFlip.TryGetValue(size, out mesh))
			{
				mesh = MeshMakerPlanes.NewPlaneMesh(size, true, false, false);
				MeshPool.planesFlip.Add(size, mesh);
			}
			return mesh;
		}

		private static Vector2 RoundedToHundredths(this Vector2 v)
		{
			return new Vector2((float)((int)(v.x * 100f)) / 100f, (float)((int)(v.y * 100f)) / 100f);
		}

		public static void LogStats()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("MeshPool stats:");
			stringBuilder.AppendLine("Planes: " + MeshPool.planes.Count);
			stringBuilder.AppendLine("PlanesFlip: " + MeshPool.planesFlip.Count);
			Log.Message(stringBuilder.ToString());
		}
	}
}
