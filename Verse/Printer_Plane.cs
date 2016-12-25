using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class Printer_Plane
	{
		private static Color32[] defaultColors = new Color32[]
		{
			new Color32(255, 255, 255, 255),
			new Color32(255, 255, 255, 255),
			new Color32(255, 255, 255, 255),
			new Color32(255, 255, 255, 255)
		};

		private static Vector2[] defaultUvs = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};

		private static Vector2[] defaultUvsFlipped = new Vector2[]
		{
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f),
			new Vector2(0f, 0f)
		};

		public static void PrintPlane(SectionLayer layer, Vector3 center, Vector2 size, Material mat, float rot = 0f, bool flipUv = false, Vector2[] uvs = null, Color32[] colors = null, float topVerticesAltitudeBias = 0.01f)
		{
			if (colors == null)
			{
				colors = Printer_Plane.defaultColors;
			}
			if (uvs == null)
			{
				if (!flipUv)
				{
					uvs = Printer_Plane.defaultUvs;
				}
				else
				{
					uvs = Printer_Plane.defaultUvsFlipped;
				}
			}
			LayerSubMesh subMesh = layer.GetSubMesh(mat);
			int count = subMesh.verts.Count;
			subMesh.verts.Add(new Vector3(-0.5f * size.x, 0f, -0.5f * size.y));
			subMesh.verts.Add(new Vector3(-0.5f * size.x, topVerticesAltitudeBias, 0.5f * size.y));
			subMesh.verts.Add(new Vector3(0.5f * size.x, topVerticesAltitudeBias, 0.5f * size.y));
			subMesh.verts.Add(new Vector3(0.5f * size.x, 0f, -0.5f * size.y));
			if (rot != 0f)
			{
				float num = rot * 0.0174532924f;
				num *= -1f;
				for (int i = 0; i < 4; i++)
				{
					float x = subMesh.verts[count + i].x;
					float z = subMesh.verts[count + i].z;
					float num2 = Mathf.Cos(num);
					float num3 = Mathf.Sin(num);
					float x2 = x * num2 - z * num3;
					float z2 = x * num3 + z * num2;
					subMesh.verts[count + i] = new Vector3(x2, subMesh.verts[count + i].y, z2);
				}
			}
			for (int j = 0; j < 4; j++)
			{
				List<Vector3> verts;
				List<Vector3> expr_1C0 = verts = subMesh.verts;
				int index;
				int expr_1C7 = index = count + j;
				Vector3 a = verts[index];
				expr_1C0[expr_1C7] = a + center;
				subMesh.uvs.Add(uvs[j]);
				subMesh.colors.Add(colors[j]);
			}
			subMesh.tris.Add(count);
			subMesh.tris.Add(count + 1);
			subMesh.tris.Add(count + 2);
			subMesh.tris.Add(count);
			subMesh.tris.Add(count + 2);
			subMesh.tris.Add(count + 3);
		}
	}
}
