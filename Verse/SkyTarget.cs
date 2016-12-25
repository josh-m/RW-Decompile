using System;
using UnityEngine;

namespace Verse
{
	public struct SkyTarget
	{
		public float glow;

		public SkyColorSet colors;

		public SkyTarget(SkyColorSet colorSet)
		{
			this.glow = 0f;
			this.colors = colorSet;
		}

		public static SkyTarget Lerp(SkyTarget A, SkyTarget B, float t)
		{
			return new SkyTarget
			{
				colors = SkyColorSet.Lerp(A.colors, B.colors, t),
				glow = Mathf.Lerp(A.glow, A.glow * B.glow, t)
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
				")"
			});
		}
	}
}
