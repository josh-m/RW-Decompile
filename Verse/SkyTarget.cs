using System;
using UnityEngine;

namespace Verse
{
	public struct SkyTarget
	{
		public float glow;

		public SkyColorSet colors;

		public float lightsourceShineSize;

		public float lightsourceShineIntensity;

		public SkyTarget(float glow, SkyColorSet colorSet, float lightsourceShineSize, float lightsourceShineIntensity)
		{
			this.glow = glow;
			this.lightsourceShineSize = lightsourceShineSize;
			this.lightsourceShineIntensity = lightsourceShineIntensity;
			this.colors = colorSet;
		}

		public static SkyTarget Lerp(SkyTarget A, SkyTarget B, float t)
		{
			return new SkyTarget
			{
				colors = SkyColorSet.Lerp(A.colors, B.colors, t),
				glow = Mathf.Lerp(A.glow, B.glow, t),
				lightsourceShineSize = Mathf.Lerp(A.lightsourceShineSize, B.lightsourceShineSize, t),
				lightsourceShineIntensity = Mathf.Lerp(A.lightsourceShineIntensity, B.lightsourceShineIntensity, t)
			};
		}

		public static SkyTarget LerpDarken(SkyTarget A, SkyTarget B, float t)
		{
			return new SkyTarget
			{
				colors = SkyColorSet.Lerp(A.colors, B.colors, t),
				glow = Mathf.Lerp(A.glow, Mathf.Min(A.glow, B.glow), t),
				lightsourceShineSize = Mathf.Lerp(A.lightsourceShineSize, Mathf.Min(A.lightsourceShineSize, B.lightsourceShineSize), t),
				lightsourceShineIntensity = Mathf.Lerp(A.lightsourceShineIntensity, Mathf.Min(A.lightsourceShineIntensity, B.lightsourceShineIntensity), t)
			};
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"(glow=",
				this.glow.ToString("F2"),
				", colors=",
				this.colors.ToString(),
				", lightsourceShineSize=",
				this.lightsourceShineSize.ToString(),
				", lightsourceShineIntensity=",
				this.lightsourceShineIntensity.ToString(),
				")"
			});
		}
	}
}
