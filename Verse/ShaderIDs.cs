using System;
using UnityEngine;

namespace Verse
{
	public static class ShaderIDs
	{
		public const string MaskTexName = "_MaskTex";

		public const string ColorName = "_Color";

		public const string ColorTwoName = "_ColorTwo";

		private static int cachedMaskTexID = -1;

		private static int cachedColorID = -1;

		private static int cachedColorTwoID = -1;

		public static int MaskTexId
		{
			get
			{
				if (ShaderIDs.cachedMaskTexID == -1)
				{
					ShaderIDs.cachedMaskTexID = Shader.PropertyToID("_MaskTex");
				}
				return ShaderIDs.cachedMaskTexID;
			}
		}

		public static int ColorId
		{
			get
			{
				if (ShaderIDs.cachedColorID == -1)
				{
					ShaderIDs.cachedColorID = Shader.PropertyToID("_Color");
				}
				return ShaderIDs.cachedColorID;
			}
		}

		public static int ColorTwoId
		{
			get
			{
				if (ShaderIDs.cachedColorTwoID == -1)
				{
					ShaderIDs.cachedColorTwoID = Shader.PropertyToID("_ColorTwo");
				}
				return ShaderIDs.cachedColorTwoID;
			}
		}
	}
}
