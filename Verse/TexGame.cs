using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class TexGame
	{
		public static readonly Texture2D AlphaAddTex;

		public static readonly Texture2D RippleTex;

		public static readonly Texture2D NoiseTex;

		static TexGame()
		{
			TexGame.AlphaAddTex = ContentFinder<Texture2D>.Get("Other/RoughAlphaAdd", true);
			TexGame.RippleTex = ContentFinder<Texture2D>.Get("Other/Ripples", true);
			TexGame.NoiseTex = ContentFinder<Texture2D>.Get("Other/Noise", true);
			Shader.SetGlobalTexture("_NoiseTex", TexGame.NoiseTex);
			Shader.SetGlobalTexture("_RippleTex", TexGame.RippleTex);
		}
	}
}
