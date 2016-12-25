using System;
using UnityEngine;

namespace Verse
{
	public struct SkyColorSet
	{
		public Color sky;

		public Color shadow;

		public Color overlay;

		public float saturation;

		public SkyColorSet(Color sky, Color shadow, Color overlay, float saturation)
		{
			this.sky = sky;
			this.shadow = shadow;
			this.overlay = overlay;
			this.saturation = saturation;
		}

		public static SkyColorSet Lerp(SkyColorSet A, SkyColorSet B, float t)
		{
			return new SkyColorSet
			{
				sky = Color.Lerp(A.sky, B.sky, t),
				shadow = Color.Lerp(A.shadow, B.shadow, t),
				overlay = Color.Lerp(A.overlay, B.overlay, t),
				saturation = Mathf.Lerp(A.saturation, B.saturation, t)
			};
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(sky=",
				this.sky,
				", shadow=",
				this.shadow,
				", overlay=",
				this.overlay,
				", sat=",
				this.saturation,
				")"
			});
		}
	}
}
