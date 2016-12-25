using System;
using UnityEngine;

namespace Verse
{
	public static class Printer_Shadow
	{
		private static readonly Color32 LowVertexColor = new Color32(0, 0, 0, 0);

		public static void PrintShadow(SectionLayer layer, Vector3 center, ShadowData shadow)
		{
			LayerSubMesh subMesh = layer.GetSubMesh(MatBases.SunShadowFade);
			Color32 item = new Color32(255, 0, 0, (byte)(255f * shadow.BaseY));
			float num = shadow.BaseX / 2f;
			float num2 = shadow.BaseZ / 2f;
			float x = center.x;
			float z = center.z;
			int count = subMesh.verts.Count;
			subMesh.verts.Add(new Vector3(x - num, 0f, z - num2));
			subMesh.verts.Add(new Vector3(x - num, 0f, z + num2));
			subMesh.verts.Add(new Vector3(x + num, 0f, z + num2));
			subMesh.verts.Add(new Vector3(x + num, 0f, z - num2));
			subMesh.colors.Add(Printer_Shadow.LowVertexColor);
			subMesh.colors.Add(Printer_Shadow.LowVertexColor);
			subMesh.colors.Add(Printer_Shadow.LowVertexColor);
			subMesh.colors.Add(Printer_Shadow.LowVertexColor);
			subMesh.tris.Add(count);
			subMesh.tris.Add(count + 1);
			subMesh.tris.Add(count + 2);
			subMesh.tris.Add(count);
			subMesh.tris.Add(count + 2);
			subMesh.tris.Add(count + 3);
			int count2 = subMesh.verts.Count;
			subMesh.verts.Add(new Vector3(x - num, 0f, z - num2));
			subMesh.verts.Add(new Vector3(x - num, 0f, z + num2));
			subMesh.colors.Add(item);
			subMesh.colors.Add(item);
			subMesh.tris.Add(count);
			subMesh.tris.Add(count2);
			subMesh.tris.Add(count2 + 1);
			subMesh.tris.Add(count);
			subMesh.tris.Add(count2 + 1);
			subMesh.tris.Add(count + 1);
			int count3 = subMesh.verts.Count;
			subMesh.verts.Add(new Vector3(x + num, 0f, z + num2));
			subMesh.verts.Add(new Vector3(x + num, 0f, z - num2));
			subMesh.colors.Add(item);
			subMesh.colors.Add(item);
			subMesh.tris.Add(count + 2);
			subMesh.tris.Add(count3);
			subMesh.tris.Add(count3 + 1);
			subMesh.tris.Add(count3 + 1);
			subMesh.tris.Add(count + 3);
			subMesh.tris.Add(count + 2);
			int count4 = subMesh.verts.Count;
			subMesh.verts.Add(new Vector3(x - num, 0f, z - num2));
			subMesh.verts.Add(new Vector3(x + num, 0f, z - num2));
			subMesh.colors.Add(item);
			subMesh.colors.Add(item);
			subMesh.tris.Add(count);
			subMesh.tris.Add(count + 3);
			subMesh.tris.Add(count4);
			subMesh.tris.Add(count + 3);
			subMesh.tris.Add(count4 + 1);
			subMesh.tris.Add(count4);
		}
	}
}
