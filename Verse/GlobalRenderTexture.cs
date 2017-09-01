using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class GlobalRenderTexture
	{
		private static RenderTexture waterLight;

		public static RenderTexture WaterLight
		{
			get
			{
				if (GlobalRenderTexture.waterLight != null && (GlobalRenderTexture.waterLight.width != Screen.width || GlobalRenderTexture.waterLight.height != Screen.height))
				{
					UnityEngine.Object.Destroy(GlobalRenderTexture.waterLight);
					GlobalRenderTexture.waterLight = null;
				}
				if (GlobalRenderTexture.waterLight == null)
				{
					GlobalRenderTexture.waterLight = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat);
				}
				if (!GlobalRenderTexture.waterLight.IsCreated())
				{
					GlobalRenderTexture.waterLight.Create();
				}
				return GlobalRenderTexture.waterLight;
			}
		}
	}
}
