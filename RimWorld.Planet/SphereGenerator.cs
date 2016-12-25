using System;
using System.Collections.Generic;
using UnityEngine;

namespace RimWorld.Planet
{
	public static class SphereGenerator
	{
		private static List<TriangleIndices> tris = new List<TriangleIndices>();

		private static List<TriangleIndices> newTris = new List<TriangleIndices>();

		private static Dictionary<long, int> middlePointsCache = new Dictionary<long, int>();

		public static void Generate(int subdivisionsCount, float radius, Vector3 viewCenter, float viewAngle, out List<Vector3> outVerts, out List<int> outIndices)
		{
			SphereGenerator.middlePointsCache.Clear();
			outVerts = new List<Vector3>();
			IcosahedronGenerator.GenerateIcosahedron(outVerts, SphereGenerator.tris, radius, viewCenter, viewAngle);
			for (int i = 0; i < subdivisionsCount; i++)
			{
				SphereGenerator.newTris.Clear();
				int j = 0;
				int count = SphereGenerator.tris.Count;
				while (j < count)
				{
					TriangleIndices triangleIndices = SphereGenerator.tris[j];
					int middlePoint = SphereGenerator.GetMiddlePoint(triangleIndices.v1, triangleIndices.v2, outVerts, radius);
					int middlePoint2 = SphereGenerator.GetMiddlePoint(triangleIndices.v2, triangleIndices.v3, outVerts, radius);
					int middlePoint3 = SphereGenerator.GetMiddlePoint(triangleIndices.v3, triangleIndices.v1, outVerts, radius);
					SphereGenerator.newTris.Add(new TriangleIndices(triangleIndices.v1, middlePoint, middlePoint3));
					SphereGenerator.newTris.Add(new TriangleIndices(triangleIndices.v2, middlePoint2, middlePoint));
					SphereGenerator.newTris.Add(new TriangleIndices(triangleIndices.v3, middlePoint3, middlePoint2));
					SphereGenerator.newTris.Add(new TriangleIndices(middlePoint, middlePoint2, middlePoint3));
					j++;
				}
				SphereGenerator.tris.Clear();
				SphereGenerator.tris.AddRange(SphereGenerator.newTris);
			}
			MeshUtility.RemoveVertices(outVerts, SphereGenerator.tris, (Vector3 x) => !MeshUtility.Visible(x, radius, viewCenter, viewAngle));
			outIndices = new List<int>();
			int k = 0;
			int count2 = SphereGenerator.tris.Count;
			while (k < count2)
			{
				TriangleIndices triangleIndices2 = SphereGenerator.tris[k];
				outIndices.Add(triangleIndices2.v1);
				outIndices.Add(triangleIndices2.v2);
				outIndices.Add(triangleIndices2.v3);
				k++;
			}
		}

		private static int GetMiddlePoint(int p1, int p2, List<Vector3> verts, float radius)
		{
			long key = ((long)Mathf.Min(p1, p2) << 32) + (long)Mathf.Max(p1, p2);
			int result;
			if (SphereGenerator.middlePointsCache.TryGetValue(key, out result))
			{
				return result;
			}
			Vector3 vector = (verts[p1] + verts[p2]) / 2f;
			int count = verts.Count;
			verts.Add(vector.normalized * radius);
			SphereGenerator.middlePointsCache.Add(key, count);
			return count;
		}
	}
}
