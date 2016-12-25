using System;
using UnityEngine;

namespace Verse
{
	public struct FloatRange : IEquatable<FloatRange>
	{
		public float min;

		public float max;

		public static FloatRange Zero
		{
			get
			{
				return new FloatRange(0f, 0f);
			}
		}

		public static FloatRange One
		{
			get
			{
				return new FloatRange(1f, 1f);
			}
		}

		public static FloatRange ZeroToOne
		{
			get
			{
				return new FloatRange(0f, 1f);
			}
		}

		public float Average
		{
			get
			{
				return (this.min + this.max) / 2f;
			}
		}

		public float RandomInRange
		{
			get
			{
				return Rand.Range(this.min, this.max);
			}
		}

		public float TrueMin
		{
			get
			{
				return Mathf.Min(this.min, this.max);
			}
		}

		public float TrueMax
		{
			get
			{
				return Mathf.Max(this.min, this.max);
			}
		}

		public float Span
		{
			get
			{
				return this.TrueMax - this.TrueMin;
			}
		}

		public FloatRange(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public float LerpThroughRange(float lerpPct)
		{
			return (1f - lerpPct) * this.min + lerpPct * this.max;
		}

		public float InverseLerpThroughRange(float f)
		{
			return (f - this.min) / (this.max - this.min);
		}

		public bool Includes(float f)
		{
			return f >= this.min && f <= this.max;
		}

		public bool IncludesEpsilon(float f)
		{
			return f >= this.min - 1E-05f && f <= this.max + 1E-05f;
		}

		public FloatRange ExpandedBy(float f)
		{
			return new FloatRange(this.min - f, this.max + f);
		}

		public static FloatRange FromString(string s)
		{
			string[] array = s.Split(new char[]
			{
				'~'
			});
			if (array.Length == 1)
			{
				float num = Convert.ToSingle(array[0]);
				return new FloatRange(num, num);
			}
			return new FloatRange(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]));
		}

		public override string ToString()
		{
			return this.min + "~" + this.max;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineStruct<float>(this.min.GetHashCode(), this.max);
		}

		public override bool Equals(object obj)
		{
			return obj is FloatRange && this.Equals((FloatRange)obj);
		}

		public bool Equals(FloatRange other)
		{
			return other.min == this.min && other.max == this.max;
		}

		public static bool operator ==(FloatRange a, FloatRange b)
		{
			return a.min == b.min && a.max == b.max;
		}

		public static bool operator !=(FloatRange a, FloatRange b)
		{
			return a.min != b.min || a.max != b.max;
		}
	}
}
