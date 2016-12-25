using System;
using UnityEngine;

namespace Verse
{
	public class ColorOption
	{
		public float weight = 10f;

		public Color min = new Color(-1f, -1f, -1f, -1f);

		public Color max = new Color(-1f, -1f, -1f, -1f);

		public Color only = new Color(-1f, -1f, -1f, -1f);

		public Color RandomizedColor()
		{
			if (this.only.a >= 0f)
			{
				return this.only;
			}
			return new Color(Rand.Range(this.min.r, this.max.r), Rand.Range(this.min.g, this.max.g), Rand.Range(this.min.b, this.max.b), Rand.Range(this.min.a, this.max.a));
		}

		public void SetSingle(Color color)
		{
			this.only = color;
		}

		public void SetMin(Color color)
		{
			this.min = color;
		}

		public void SetMax(Color color)
		{
			this.max = color;
		}
	}
}
