using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class MaterialAtlasPool
	{
		private class MaterialAtlas
		{
			private const float TexPadding = 0.0029296875f;

			protected Material[] subMats = new Material[16];

			public MaterialAtlas(Material newRootMat)
			{
				Vector2 mainTextureScale = new Vector2(0.244140625f, 0.244140625f);
				for (int i = 0; i < 16; i++)
				{
					float x = (float)(i % 4) * 0.25f + 0.0029296875f;
					float y = (float)(i / 4) * 0.25f + 0.0029296875f;
					Vector2 mainTextureOffset = new Vector2(x, y);
					Material material = new Material(newRootMat);
					material.name = newRootMat.name + "_ASM" + i;
					material.mainTextureScale = mainTextureScale;
					material.mainTextureOffset = mainTextureOffset;
					this.subMats[i] = material;
				}
			}

			public Material SubMat(LinkDirections linkSet)
			{
				if ((int)linkSet >= this.subMats.Length)
				{
					Log.Warning("Cannot get submat of index " + (int)linkSet + ": out of range.");
					return BaseContent.BadMat;
				}
				return this.subMats[(int)linkSet];
			}
		}

		private static Dictionary<Material, MaterialAtlasPool.MaterialAtlas> atlasDict = new Dictionary<Material, MaterialAtlasPool.MaterialAtlas>();

		public static Material SubMaterialFromAtlas(Material mat, LinkDirections LinkSet)
		{
			if (!MaterialAtlasPool.atlasDict.ContainsKey(mat))
			{
				MaterialAtlasPool.atlasDict.Add(mat, new MaterialAtlasPool.MaterialAtlas(mat));
			}
			return MaterialAtlasPool.atlasDict[mat].SubMat(LinkSet);
		}
	}
}
