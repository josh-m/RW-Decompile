using System;
using UnityEngine;

namespace Verse
{
	public struct CurvePoint
	{
		private Vector2 loc;

		public Vector2 Loc
		{
			get
			{
				return this.loc;
			}
		}

		public float x
		{
			get
			{
				return this.loc.x;
			}
		}

		public float y
		{
			get
			{
				return this.loc.y;
			}
		}

		public CurvePoint(float x, float y)
		{
			this.loc = new Vector2(x, y);
		}

		public CurvePoint(Vector2 loc)
		{
			this.loc = loc;
		}

		public static CurvePoint FromString(string str)
		{
			return new CurvePoint((Vector2)ParseHelper.FromString(str, typeof(Vector2)));
		}

		public override string ToString()
		{
			return this.loc.ToStringTwoDigits();
		}

		public static implicit operator Vector2(CurvePoint pt)
		{
			return pt.loc;
		}
	}
}
