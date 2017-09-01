using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class MatBases
	{
		public static readonly Material LightOverlay = MatLoader.LoadMat("Lighting/LightOverlay", -1);

		public static readonly Material SunShadow = MatLoader.LoadMat("Lighting/SunShadow", -1);

		public static readonly Material SunShadowFade = MatBases.SunShadow;

		public static readonly Material EdgeShadow = MatLoader.LoadMat("Lighting/EdgeShadow", -1);

		public static readonly Material IndoorMask = MatLoader.LoadMat("Misc/IndoorMask", -1);

		public static readonly Material FogOfWar = MatLoader.LoadMat("Misc/FogOfWar", -1);

		public static readonly Material Snow = MatLoader.LoadMat("Misc/Snow", -1);
	}
}
