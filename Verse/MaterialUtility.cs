using System;
using UnityEngine;

namespace Verse
{
	public static class MaterialUtility
	{
		public static Texture2D GetMaskTexture(this Material mat)
		{
			if (!mat.HasProperty(ShaderIDs.MaskTexId))
			{
				return null;
			}
			return (Texture2D)mat.GetTexture(ShaderIDs.MaskTexId);
		}

		public static Color GetColorTwo(this Material mat)
		{
			if (!mat.HasProperty(ShaderIDs.ColorTwoId))
			{
				return Color.white;
			}
			return mat.GetColor(ShaderIDs.ColorTwoId);
		}
	}
}
