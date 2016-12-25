using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class MeshMakerCircles
	{
		public static Mesh MakePieMesh(int DegreesWide)
		{
			List<Vector2> list = new List<Vector2>();
			list.Add(new Vector2(0f, 0f));
			for (int i = 0; i < DegreesWide; i++)
			{
				float num = (float)i / 180f * 3.14159274f;
				list.Add(new Vector2(0f, 0f)
				{
					x = (float)(0.550000011920929 * Math.Cos((double)num)),
					y = (float)(0.550000011920929 * Math.Sin((double)num))
				});
			}
			Vector3[] array = new Vector3[list.Count];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = new Vector3(list[j].x, 0f, list[j].y);
			}
			Triangulator triangulator = new Triangulator(list.ToArray());
			int[] triangles = triangulator.Triangulate();
			Mesh mesh = new Mesh();
			mesh.name = "MakePieMesh()";
			mesh.vertices = array;
			mesh.uv = new Vector2[list.Count];
			mesh.triangles = triangles;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}

		public static Mesh MakeCircleMesh(float radius)
		{
			List<Vector2> list = new List<Vector2>();
			list.Add(new Vector2(0f, 0f));
			for (int i = 0; i <= 360; i += 4)
			{
				float f = (float)i / 180f * 3.14159274f;
				list.Add(new Vector2(radius * Mathf.Cos(f), radius * Mathf.Sin(f)));
			}
			Vector3[] array = new Vector3[list.Count];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = new Vector3(list[j].x, 0f, list[j].y);
			}
			int[] array2 = new int[(array.Length - 1) * 3];
			for (int k = 1; k < array.Length; k++)
			{
				int num = (k - 1) * 3;
				array2[num] = 0;
				array2[num + 1] = (k + 1) % array.Length;
				array2[num + 2] = k;
			}
			return new Mesh
			{
				name = "MakeCircleMesh()",
				vertices = array,
				triangles = array2
			};
		}
	}
}
