using System;
using UnityEngine;

namespace Verse
{
	public struct IntVec2 : IEquatable<IntVec2>
	{
		public int x;

		public int z;

		public bool IsInvalid
		{
			get
			{
				return this.x < -500;
			}
		}

		public bool IsValid
		{
			get
			{
				return this.x >= -500;
			}
		}

		public static IntVec2 Zero
		{
			get
			{
				return new IntVec2(0, 0);
			}
		}

		public static IntVec2 One
		{
			get
			{
				return new IntVec2(1, 1);
			}
		}

		public static IntVec2 Two
		{
			get
			{
				return new IntVec2(2, 2);
			}
		}

		public static IntVec2 North
		{
			get
			{
				return new IntVec2(0, 1);
			}
		}

		public static IntVec2 East
		{
			get
			{
				return new IntVec2(1, 0);
			}
		}

		public static IntVec2 South
		{
			get
			{
				return new IntVec2(0, -1);
			}
		}

		public static IntVec2 West
		{
			get
			{
				return new IntVec2(-1, 0);
			}
		}

		public float Magnitude
		{
			get
			{
				return Mathf.Sqrt((float)(this.x * this.x + this.z * this.z));
			}
		}

		public static IntVec2 Invalid
		{
			get
			{
				return new IntVec2(-1000, -1000);
			}
		}

		public IntVec3 ToIntVec3
		{
			get
			{
				return new IntVec3(this.x, 0, this.z);
			}
		}

		public IntVec2(int newX, int newZ)
		{
			this.x = newX;
			this.z = newZ;
		}

		public IntVec2(Vector2 v2)
		{
			this.x = (int)v2.x;
			this.z = (int)v2.y;
		}

		public Vector2 ToVector2()
		{
			return new Vector2((float)this.x, (float)this.z);
		}

		public Vector3 ToVector3()
		{
			return new Vector3((float)this.x, 0f, (float)this.z);
		}

		public IntVec2 Rotated()
		{
			return new IntVec2(this.z, this.x);
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"(",
				this.x.ToString(),
				", ",
				this.z.ToString(),
				")"
			});
		}

		public string ToStringCross()
		{
			return this.x.ToString() + " x " + this.z.ToString();
		}

		public static IntVec2 FromString(string str)
		{
			str = str.TrimStart(new char[]
			{
				'('
			});
			str = str.TrimEnd(new char[]
			{
				')'
			});
			string[] array = str.Split(new char[]
			{
				','
			});
			int newX = Convert.ToInt32(array[0]);
			int newZ = Convert.ToInt32(array[1]);
			return new IntVec2(newX, newZ);
		}

		public Vector2 ToVector2Shifted()
		{
			return new Vector2((float)this.x + 0.5f, (float)this.z + 0.5f);
		}

		public override bool Equals(object obj)
		{
			return obj is IntVec2 && this.Equals((IntVec2)obj);
		}

		public bool Equals(IntVec2 other)
		{
			return this.x == other.x && this.z == other.z;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineInt(this.x, this.z);
		}

		public static IntVec2 operator +(IntVec2 a, IntVec2 b)
		{
			return new IntVec2(a.x + b.x, a.z + b.z);
		}

		public static IntVec2 operator -(IntVec2 a, IntVec2 b)
		{
			return new IntVec2(a.x - b.x, a.z - b.z);
		}

		public static IntVec2 operator *(IntVec2 a, int b)
		{
			return new IntVec2(a.x * b, a.z * b);
		}

		public static IntVec2 operator /(IntVec2 a, int b)
		{
			return new IntVec2(a.x / b, a.z / b);
		}

		public static bool operator ==(IntVec2 a, IntVec2 b)
		{
			return a.x == b.x && a.z == b.z;
		}

		public static bool operator !=(IntVec2 a, IntVec2 b)
		{
			return a.x != b.x || a.z != b.z;
		}
	}
}
